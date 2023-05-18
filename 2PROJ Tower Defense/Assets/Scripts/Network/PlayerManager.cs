
using UnityEngine;
using System.Collections;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Collections;
using TMPro;
using Unity.Services.Lobbies.Models;
using System;

public class PlayerManager : NetworkBehaviour
{

    public NetworkList<PlayerData> SyncedPlayers;

    



    void Awake()
    {
        SyncedPlayers = new NetworkList<PlayerData>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!NetworkManager.Singleton.IsServer)
        {
            var playerData = new PlayerData();
            playerData.name = PlayerPrefs.GetString("PLAYER_NAME");
            playerData.money = 150;
            GameObject.Find("PlayerManager").GetComponentInChildren<PlayerManager>().AddPlayerServerRpc(playerData);

        }
    }



    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerServerRpc(PlayerData playerData)
    {
        SyncedPlayers.Add(playerData);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetMoneyAllServerRpc(int amount)
    {
        for (int i = 0; i < SyncedPlayers.Count; i++)
        {
            PlayerData playerData = SyncedPlayers[i];
            playerData.money = amount;
            SyncedPlayers[i] = playerData;
        }
    }

    /// <summary>
    /// Sets the attribute of a specific player,
    /// available : money
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerAttributeServerRpc(FixedString32Bytes name, int money)
    {
        
        for (int i = 0; i < SyncedPlayers.Count; i++)
        {
            if (SyncedPlayers[i].name == name)
            {
                PlayerData playerData = SyncedPlayers[i];
                playerData.money = (int)money;
                SyncedPlayers[i] = playerData;
                break;
            }
        }
    }


    public Nullable<PlayerData> GetCurrentPlayerData()
    {
        foreach(PlayerData playerData in SyncedPlayers)
        {
            if (playerData.name == PlayerPrefs.GetString("PLAYER_NAME"))
            {
                return playerData;
            }
        }
        return default(PlayerData?);
    }

    

}

public struct PlayerData : INetworkSerializable, System.IEquatable<PlayerData>
{
    public FixedString32Bytes name;
    public int money;

    public PlayerData(
        FixedString32Bytes name,
        int money)
    {
        this.name = name;

        this.money = money;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out name);
            reader.ReadValueSafe(out money);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(name);
            writer.WriteValueSafe(money);
        }
    }

    public bool Equals(PlayerData other)
    {
        return name == other.name && money == other.money;
    }
}