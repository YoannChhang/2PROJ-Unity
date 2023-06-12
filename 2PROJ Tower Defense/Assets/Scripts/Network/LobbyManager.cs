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
using Unity.Netcode;
using UnityEngine.Events;
using Unity.Collections;
using System.Threading.Tasks;

public class LobbyManager : MonoBehaviour
{

    [SerializeField] private GameObject _userInLobbyPrefab;
    [SerializeField] private GameObject lobbySelectionItemPrefab;
    [SerializeField] private GameObject lobbySelectionItemContainer;
    [SerializeField] private GameObject _userInLobbyContainer;
    [SerializeField] private Button _readyButton;
    [SerializeField] private GameObject lobbyTitleText;


    //Screens to toggle
    [SerializeField] private GameObject _connectionModeScreen;
    [SerializeField] private GameObject _lobbyScreen;
    [SerializeField] private GameObject selectLobbyScreen;
    private string currentScreen = "SelectConnectionMode";

    //Storing all info related to lobby
    private Lobby hostLobby;
    public Lobby joinedLobby;
    //private List<> lobbyList;

    //timers
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    //private float lobbyPlayerListUpdateTimer;
    private float handleUpdatesTimer;


    //Other variables
    private string playerName;
    private bool readyStatus;
    private string SELECTED_MODE = "0";


    //Toggles, used to prevent 429
    private bool eventStartGame = false;
    private bool eventReadyToggle = false;


    private void Awake()
    {
        readyStatus = false;
        goto_SelectConnMode();
    }
    void OnEnable()
    {
        SELECTED_MODE = PlayerPrefs.GetString("SELECTED_MODE");
    }
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            playerName = "Player" + UnityEngine.Random.Range(10, 99);

            PlayerPrefs.SetString("PLAYER_NAME", playerName);

        }
        else
        {
            playerName = PlayerPrefs.GetString("PLAYER_NAME");
        }

        Debug.Log($"Player name {playerName}");


        //GameObject.Find("Return").GetComponentInChildren<Button>().onClick.AddListener(Disconnect);

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;



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

                try
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
                }
                catch
                {
                    Debug.Log("Error with SendHeartbeatPingAsync");
                }
            }
        }
    }

    //Every x seconds the player will poll for the updates related to the lobby.
    private async Task HandleLobbyPollForUpdates()
    {

        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                
                float lobbyUpdateTimerMax = 1.5f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;

                try
                {
                    Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                    joinedLobby = lobby;

                    try
                    {
                        UpdateLobbyPlayerList();

                    }
                    catch
                    {
                        Debug.Log("Error with UpdateLobbyPlayerList");
                    }

                    if (joinedLobby.Data["GAME_STARTED"].Value != "0")
                    {

                        joinedLobby = null;
                        switch(SELECTED_MODE)
                        {
                            case "0":
                                SceneManager.LoadScene("GreenScene");
                                break;

                            case "1":
                                SceneManager.LoadScene("CircleScene");
                                break;

                            default:
                                break;

                        }


                    }
                }
                catch 
                {
                    Debug.Log($"PollForUpdate Error : ");
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
                    //Mode 1
                    if (SELECTED_MODE != null)
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

                            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                            {
                                Data = new Dictionary<string, DataObject>{
                                    { "GAME_STARTED", new DataObject(DataObject.VisibilityOptions.Member, "1") },
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
        else
        {
            handleUpdatesTimer -= Time.deltaTime;
            if (handleUpdatesTimer < 0f)
            {
                float handleUpdatesTimerMax = 2f;
                handleUpdatesTimer = handleUpdatesTimerMax;


                if (currentScreen == "SelectLobby")
                {
                    LoadLobbyList();

                }
                
            }
        }
    }
   


    public async void CreateLobby()
    {
        try
        {
            string lobbyName = $"{playerName}'s Lobby";
            int maxPlayers = 9;
            //Relay
            string relayCode = await RelayManager.instance.CreateRelay();

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false, //Set to true if using code, if quickjoin then false
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "SELECTED_MODE", new DataObject(DataObject.VisibilityOptions.Public, SELECTED_MODE) },
                    { "GAME_STARTED", new DataObject(DataObject.VisibilityOptions.Member, "0") },
                    { "RELAY_CODE", new DataObject(DataObject.VisibilityOptions.Member, relayCode) },
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions); ;


            hostLobby = lobby;
            joinedLobby = hostLobby;

            lobbyTitleText.GetComponentInChildren<TMP_Text>().text = lobbyName;


            var playerData = new PlayerData();
            playerData.name = playerName;
            GameObject.Find("PlayerManager").GetComponentInChildren<PlayerManager>().AddPlayerServerRpc(playerData);
            

            Debug.Log($"Created the lobby {lobby.Id}, mode : {lobby.Data.GetValueOrDefault("SELECTED_MODE").Value}");


            SetReadyButton();
            goto_Lobby();

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }


    public async void JoinLobby(string selectedLobbyId)
    {
        try
        {


            if (selectedLobbyId == null) return;

            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(selectedLobbyId, joinLobbyByIdOptions);
            joinedLobby = lobby;

            Debug.Log($"Joined the lobby {selectedLobbyId}");


            RelayManager.instance.JoinRelay(joinedLobby.Data["RELAY_CODE"].Value);

            lobbyTitleText.GetComponentInChildren<TMP_Text>().text = joinedLobby.Name;

            


            SetReadyButton();
            goto_Lobby();

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
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                Transform child = obj.transform.GetChild(i);
                if (child.name == "ReadyIcon")
                {
                    //Debug.Log(player.Data["ReadyStatus"].Value);
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

    private async void LoadLobbyList()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            // Filter for open lobbies only
            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };

            //Order by newest lobbies first
            options.Order = new List<QueryOrder>()
            {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };
            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
            

            HelperFunctions.remove_all_childs_from_gameobject(lobbySelectionItemContainer);

            foreach (Lobby lobby in lobbies.Results)
            {
                if (lobby.Data.GetValueOrDefault("SELECTED_MODE").Value == SELECTED_MODE)
                {
                    GameObject lobbyObj = Instantiate(lobbySelectionItemPrefab, lobbySelectionItemContainer.transform);
                    TMP_Text[] title_and_player_count = lobbyObj.GetComponentsInChildren<TMP_Text>();
                    title_and_player_count[0].text = lobby.Name;
                    title_and_player_count[1].text = $"{lobby.Players.Count}/{lobby.Players.Capacity}";
                    lobbyObj.GetComponentInChildren<Text>().text = lobby.Id;
                    lobbyObj.GetComponentInChildren<Button>().onClick.AddListener(() =>
                    {
                        JoinLobby(lobby.Id);
                    });
                }

                
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public void goto_SelectLobby()
    {
        currentScreen = "SelectLobby";

        selectLobbyScreen.SetActive(true);

        _lobbyScreen.SetActive(false);
        _connectionModeScreen.SetActive(false);
    }
    private void goto_Lobby()
    {
        currentScreen = "Lobby";

        _lobbyScreen.SetActive(true);

        _connectionModeScreen.SetActive(false);
        selectLobbyScreen.SetActive(false);

    }
    public void goto_SelectConnMode()
    {
        currentScreen = "SelectConnectionMode";
        _connectionModeScreen.SetActive(true);

        selectLobbyScreen.SetActive(false);
        _lobbyScreen.SetActive(false);

    }



    public bool IsLobbyHost()
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


    [ServerRpc(RequireOwnership = false)]
    public async void DisconnectUserServerRpc(string playerId)
    {
        await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
    }

    public async void CloseLobby()
    {
        if (IsLobbyHost())
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);

        }
    }

    public void Disconnect()
    {
        string id = null;
        foreach (Player p in joinedLobby.Players)
        {
            if (p.Data["PlayerName"].Value == playerName)
            {
                id = p.Id;
            }

        }
        string saveLobbyId = joinedLobby.Id;
        joinedLobby = null;
        LobbyService.Instance.RemovePlayerAsync(saveLobbyId, id);
        NetworkManager.Singleton.Shutdown();

        goto_SelectConnMode();

    }

    public void OnClientDisconnect(ulong clientId)
    {
        try
        {
            string id = null;
            foreach (Player p in joinedLobby.Players)
            {
                if (p.Data["PlayerName"].Value == playerName)
                {
                    id = p.Id;
                }

            }
            //Debug.Log("Disconnection of " + id);
            //DisconnectUserServerRpc(id);

        }
        catch
        {

        }
        
    }
}