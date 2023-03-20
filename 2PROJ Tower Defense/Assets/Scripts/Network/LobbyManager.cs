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
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{

    [SerializeField] private TMP_InputField _lobbyCodeInput;
    [SerializeField] private TMP_Text _lobbyCodeDisplay;

    [SerializeField] private GameObject _userInLobbyPrefab;
    [SerializeField] private GameObject _userInLobbyContainer;
    [SerializeField] private GameObject _connectionModeScreen;
    [SerializeField] private GameObject _lobbyScreen;
    [SerializeField] private Button _readyButton;

    //Storing all info related to lobby
    private Lobby hostLobby;
    private Lobby joinedLobby;

    //timers
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    //private float lobbyPlayerListUpdateTimer;
    private float handleUpdatesTimer;


    //Other variables
    private string playerName;
    private bool readyStatus;
    private int SELECTED_LEVEL = 0;


    //Toggles, used to prevent 429
    private bool eventStartGame = false;
    private bool eventReadyToggle = false;


    private void Awake()
    {
        readyStatus = false;
        CloseLobby();
    }
    void OnEnable()
    {
        SELECTED_LEVEL = PlayerPrefs.GetInt("SELECTED_LEVEL");
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

        HandleUpdates();

    }

    //Lobby heartbeat in order for it not to shutdown
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

    //Every x seconds the player will poll for the updates related to the lobby.
    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 1.3f;
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

                    SceneManager.LoadScene("GameScene");


                }
            }
        }
    }
    private async void HandleUpdates()
    {
        if (joinedLobby != null)
        {
            handleUpdatesTimer -= Time.deltaTime;
            if(handleUpdatesTimer < 0f)
            {
                float handleUpdatesTimerMax = 1.3f;
                handleUpdatesTimer = handleUpdatesTimerMax;



                //When Starting the game
                if (eventStartGame)
                {
                    if (SELECTED_LEVEL != 0)
                    {
                        try
                        {
                            eventStartGame = false;

                            Debug.Log("StartGame");

                            //Check if all players are ready

                            foreach (Player player in joinedLobby.Players)
                            {
                                if (player.Data["ReadyStatus"].Value == "False")
                                {
                                    Debug.Log("Could not start game, all players are not ready");
                                    //return;
                                }
                            }

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


                //Player click on ready
                if (eventReadyToggle)
                {
                    try
                    {
                        readyStatus = !readyStatus;

                        Player player = GetCurrentPlayerInLobby();

                        UpdatePlayerOptions options = new UpdatePlayerOptions();

                        options.Data = new Dictionary<string, PlayerDataObject>() {
                            {"ReadyStatus", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, readyStatus.ToString()) },
                        };

                        Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, player.Id, options);

                        SetReadyButton();

                        eventReadyToggle = false;

                    }
                    catch (LobbyServiceException e)
                    {
                        Debug.LogError($"ToggleReadyStatus Error : " + e);
                    }

                    SetReadyButton();
                }


            }
        }
    }
   


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

   

    private void UpdateLobbyPlayerList()
    {
        // Delete all the children of the _userInLobbyContainer
        HelperFunctions.remove_all_childs_from_gameobject(_userInLobbyContainer);


        //Add all player prefabs
        foreach (Player player in joinedLobby.Players)
        {
            GameObject obj = Instantiate(_userInLobbyPrefab);
            obj.transform.SetParent(_userInLobbyContainer.transform, false);
            obj.GetComponentInChildren<TMP_Text>().text = player.Data["PlayerName"].Value;

            //Ready Status for each players
            for (int i = 0; i < obj.transform.GetChild(0).transform.childCount; i++)
            {
                Transform child = obj.transform.GetChild(0).transform.GetChild(i);
                if (child.name == "ReadyIcon")
                {
                    if (player.Data["ReadyStatus"].Value == "True")
                    {
                        child.gameObject.SetActive(true);
                    }
                    else
                    {
                        child.gameObject.SetActive(false);
                    }

                }
            }

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
    
    private void SetReadyButton()
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


    public void StartGame()
    {
        if (IsLobbyHost())
        {
            eventStartGame = true;
        }
    }

    public void ToggleReadyStatus()
    {
        eventReadyToggle = true;
    }
}