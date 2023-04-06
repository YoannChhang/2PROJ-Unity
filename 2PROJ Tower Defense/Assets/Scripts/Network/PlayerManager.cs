
using UnityEngine;
using System.Collections;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Collections;

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
            GameObject.Find("PlayerManager").GetComponentInChildren<PlayerManager>().AddPlayerServerRpc(playerData);

        }
    }

    //private void Update()
    //{

    //    Debug.Log(SyncedPlayers.Count);
    //}


    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerServerRpc(PlayerData playerData)
    {
        SyncedPlayers.Add(playerData);
    }

    

}

public struct PlayerData : INetworkSerializable, System.IEquatable<PlayerData>
{
    public FixedString32Bytes name;
    public int money;

    public PlayerData(
        FixedString32Bytes name,
        int money = 0)
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