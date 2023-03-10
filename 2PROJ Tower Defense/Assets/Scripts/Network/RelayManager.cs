using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
// ReSharper disable SuggestVarOrType_SimpleTypes
// ReSharper disable SuggestVarOrType_BuiltInTypes



public class RelayManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _joinCodeText;
    [SerializeField] private TMP_InputField _joinInput;
    [SerializeField] private GameObject _connectionModeScreen;
    [SerializeField] private GameObject _lobbyScreen;


    private UnityTransport _transport;
    private const int MaxPlayers = 4;

    public static RelayManager instance;


    private void Awake()
    {

        _transport = FindObjectOfType<UnityTransport>();

        if (instance != null && instance != this)
        {
            // Destroy this instance if there's already another one
            Destroy(gameObject);
            return;
        }

        // Set this instance as the singleton instance
        instance = this;

        // Keep this object across scenes
        DontDestroyOnLoad(gameObject);

    }

    public async Task<string> CreateRelay()
    {

        Allocation a = await RelayService.Instance.CreateAllocationAsync(MaxPlayers);
        _joinCodeText.text = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

        _transport.SetRelayServerData(new RelayServerData(a, "dtls"));

        NetworkManager.Singleton.StartHost();

        Debug.Log("Created Relay");


        return _joinCodeText.text;
    }

    public async void JoinRelay(string relayCode)
    {

        JoinAllocation a = await RelayService.Instance.JoinAllocationAsync(relayCode);

        _transport.SetRelayServerData(new RelayServerData(a, "dtls"));

        NetworkManager.Singleton.StartClient();

        Debug.Log("Joined Relay");
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
}