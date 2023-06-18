
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
            playerData.money = 800;
            GameObject.Find("PlayerManager").GetComponentInChildren<PlayerManager>().AddPlayerServerRpc(playerData);

        }
    }

    public void OnDestroy()
    {
        if (IsServer){
            SyncedPlayers.Clear();
        }
        SyncedPlayers.Dispose();
        
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

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerSpecificStatServerRpc(
        FixedString32Bytes name,
        int enemy_killed = 0,
        int damage_dealt = 0,
        int tower_placed = 0,
        int money_made = 0,
        int money_spent = 0)
    {

        for (int i = 0; i < SyncedPlayers.Count; i++)
        {
            if (SyncedPlayers[i].name == name)
            {
                PlayerData playerData = SyncedPlayers[i];

                playerData.stats.enemy_killed += enemy_killed;
                playerData.stats.damage_dealt += damage_dealt;
                playerData.stats.tower_placed += tower_placed;
                playerData.stats.money_made += money_made;
                playerData.stats.money_spent += money_spent;


                SyncedPlayers[i] = playerData;
                // Debug.Log("money made = " + playerData.stats.money_made);
                // Debug.Log("money spent = " + playerData.stats.money_spent);
                // Debug.Log("enemy killed = " + playerData.stats.enemy_killed);
                // Debug.Log("damage dealt = " + playerData.stats.damage_dealt);
                // Debug.Log("tower placed = " + playerData.stats.tower_placed);
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
    
    public PlayerData[] GetAllPlayerData()
    {
        PlayerData[] playerDataArray = new PlayerData[SyncedPlayers.Count];
        for (int i = 0; i < SyncedPlayers.Count; i++)
        {
            playerDataArray[i] = SyncedPlayers[i];
        }
        return playerDataArray;
    }

}

public struct PlayerData : INetworkSerializable, System.IEquatable<PlayerData>
{
    public FixedString32Bytes name;
    public int money;
    public PlayerStats stats;

    public PlayerData(FixedString32Bytes name, int money, PlayerStats stats)
    {
        this.name = name;
        this.money = money;
        this.stats = stats;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out name);
            reader.ReadValueSafe(out money);
            reader.ReadValueSafe(out stats);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(name);
            writer.WriteValueSafe(money);
            writer.WriteValueSafe(stats);
        }
    }

    public bool Equals(PlayerData other)
    {
        return name == other.name && money == other.money && stats.Equals(other.stats);
    }
}
public struct PlayerStats : INetworkSerializable, System.IEquatable<PlayerStats>
{
    public int enemy_killed;
    public int damage_dealt;
    public int tower_placed;
    public int money_made;
    public int money_spent;

    public PlayerStats(int enemy_killed, int damage_dealt, int tower_placed, int money_made, int money_spent)
    {
        this.enemy_killed = enemy_killed;
        this.damage_dealt = damage_dealt;
        this.tower_placed = tower_placed;
        this.money_made = money_made;
        this.money_spent = money_spent;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out enemy_killed);
            reader.ReadValueSafe(out damage_dealt);
            reader.ReadValueSafe(out tower_placed);
            reader.ReadValueSafe(out money_made);
            reader.ReadValueSafe(out money_spent);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(enemy_killed);
            writer.WriteValueSafe(damage_dealt);
            writer.WriteValueSafe(tower_placed);
            writer.WriteValueSafe(money_made);
            writer.WriteValueSafe(money_spent);
        }
    }

    public bool Equals(PlayerStats other)
    {
        return enemy_killed == other.enemy_killed && damage_dealt == other.damage_dealt
            && tower_placed == other.tower_placed && money_made == other.money_made
            && money_spent == other.money_spent;
    }
}
