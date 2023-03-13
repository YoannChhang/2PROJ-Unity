using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using TMPro;
using UnityEngine.UI;
using System.Runtime.InteropServices.ComTypes;
using Unity.IO.LowLevel.Unsafe;
using static UnityEditor.Progress;

public class LobbyManager : MonoBehaviour
{

    [SerializeField] private TMP_InputField _lobbyCodeInput;
    [SerializeField] private TMP_Text _lobbyCodeDisplay;

    [SerializeField] private GameObject _userInLobbyPrefab;
    [SerializeField] private GameObject _userInLobbyContainer;
    [SerializeField] private GameObject _connectionModeScreen;
    [SerializeField] private GameObject _lobbyScreen;
    [SerializeField] private Button _readyButton;

    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private float lobbyPlayerListUpdateTimer;
    private string playerName;
    private bool readyStatus;

    private void Awake()
    {
        readyStatus = false;
        CloseLobby();
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        playerName = "TDPlayer" + UnityEngine.Random.Range(10, 99);
        Debug.Log($"Player name {playerName}");



    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
        //HandleLobbyUpdatePlayerList();

    }
    private async void HandleLobbyHeartbeat()
    {
        if (IsLobbyHost())
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15;
                heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }
    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 1.1f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;

                UpdateLobbyPlayerList();

                if (joinedLobby.Data["GAME_STARTED"].Value != "0")
                {
                    if (!IsLobbyHost())
                    {
                        RelayManager.instance.JoinRelay(joinedLobby.Data["GAME_STARTED"].Value);
                    }
                    joinedLobby = null;

                }
            }
        }
    }
    //private async void HandleLobbyUpdatePlayerList()
    //{
    //    if (hostLobby == null)
    //    {
    //        lobbyPlayerListUpdateTimer -= Time.deltaTime;
    //        if (lobbyPlayerListUpdateTimer < 0f)
    //        {
    //            float lobbyPlayerListUpdateTimerMax = 1.1f;
    //            lobbyUpdateTimer = lobbyPlayerListUpdateTimerMax;

    //            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
    //            joinedLobby = lobby;

    //            UpdateLobbyPlayerList();
    //        }
    //    }
    //}


    public async void CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 4;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false, //Set to true if using code, if quickjoin then false
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "SELECTED_LEVEL", new DataObject(DataObject.VisibilityOptions.Public, "1") },
                    { "GAME_STARTED", new DataObject(DataObject.VisibilityOptions.Member, "0") },
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions); ;

            hostLobby = lobby;
            joinedLobby = hostLobby;

            Debug.Log($"Created the lobby {lobby.Id} | {lobby.LobbyCode}");

            //UI
            _lobbyCodeDisplay.text = lobby.LobbyCode;

            SetReadyButton();
            OpenLobby();

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }


    public async void JoinLobby()
    {
        try
        {
            string inputedLobbyCode = _lobbyCodeInput.text;

            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(inputedLobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;

            Debug.Log($"Joined the lobby {inputedLobbyCode}");

            _lobbyCodeDisplay.text = lobby.LobbyCode;


            SetReadyButton();
            OpenLobby();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }


    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                {"ReadyStatus", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, readyStatus.ToString()) },

            }
        };
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log($"Players in Lobby {lobby.Name} | {lobby.Name} | " + lobby.Data["SELECTED_LEVEL"]);
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }

    private void UpdateLobbyPlayerList()
    {
        // Delete all the children of the _userInLobbyContainer
        foreach (Transform child in _userInLobbyContainer.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Player player in joinedLobby.Players)
        {
            GameObject obj = Instantiate(_userInLobbyPrefab);
            obj.transform.SetParent(_userInLobbyContainer.transform, false);
            obj.GetComponentInChildren<TMP_Text>().text = player.Data["PlayerName"].Value;
        }
    }

    private void OpenLobby()
    {
        _connectionModeScreen.SetActive(false);
        _lobbyScreen.SetActive(true);
    }
    private void CloseLobby()
    {
        _connectionModeScreen.SetActive(true);
        _lobbyScreen.SetActive(false);
    }
    private bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }
    public async void StartGame()
    {
        if (IsLobbyHost())
        {
            try
            {
                Debug.Log("StartGame");

                string relayCode = await RelayManager.instance.CreateRelay();

                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>{
                        { "GAME_STARTED", new DataObject(DataObject.VisibilityOptions.Member, relayCode) },
                    }
                });

                joinedLobby = lobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"StartGame Error : " + e);
            }
        }
    }

    public async void ToggleReadyStatus()
    {
        try
        {
            readyStatus = !readyStatus;

            Player player = GetCurrentPlayerInLobby();
            Debug.Log(player.Data);


            UpdatePlayerOptions options = new UpdatePlayerOptions();

            options.Data = new Dictionary<string, PlayerDataObject>() {
                    {"ReadyStatus", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, readyStatus.ToString()) },
                };

            Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, player.Id, options);

            SetReadyButton();

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"ToggleReadyStatus Error : " + e);
        }

        SetReadyButton();
    }

    public void SetReadyButton()
    {

        string readyText = readyStatus ? "Unready" : "Ready";
        Color readyColor = readyStatus ? Color.green : Color.red;
        _readyButton.GetComponentInChildren<TMP_Text>().text = readyText;
        _readyButton.GetComponentInChildren<Image>().color = readyColor;

    }

    private Player GetCurrentPlayerInLobby()
    {
        string playerId = AuthenticationService.Instance.PlayerId;
        foreach (Player player in joinedLobby.Players)
        {
            if (player.Id == playerId)
            {
                return player;
            }
        }
        return null;
    }

}