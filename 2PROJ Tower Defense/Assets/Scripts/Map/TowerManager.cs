using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;





public class TowerManager : NetworkBehaviour
{

    [SerializeField] public Grid grid;
    [SerializeField] public Tilemap tilemapPrefab;
    [SerializeField] public Tilemap paths;

    private NetworkList<TowerData> SyncedTowers;

    void Awake()
    {
        SyncedTowers = new NetworkList<TowerData>();
    }

    private void Update()
    {
        if (!this.NetworkObject.IsSpawned && NetworkManager.Singleton.IsServer)
        {
            try
            {
                this.NetworkObject.Spawn();
            }
            catch
            {

            }
        }



        if (Input.GetMouseButtonDown(0))
        {


            Vector3Int cellIndex = getCellIndexFromMouse();

            var newTower = new TowerData();
            newTower.cellIndex = cellIndex;
            AddTowerServerRpc(newTower);



        }
    }

    //When NetworkSpawns
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        //Add eventlisteners
        if (IsClient)
        {
            SyncedTowers.OnListChanged += OnClientListChanged;
        }
        if (IsServer)
        {
            SyncedTowers.OnListChanged += OnServerListChanged;

        }
    }

    //This is made so that the client can modify NetworkVariable
    [ServerRpc(RequireOwnership = false)]
    void AddTowerServerRpc(TowerData newTower)
    {
        // Add the new tower to the SyncedTowers list
        SyncedTowers.Add(newTower);
    }

    

    void OnServerListChanged(NetworkListEvent<TowerData> changeEvent)
    {
        HandleSyncedDataUpdates();
    }

    void OnClientListChanged(NetworkListEvent<TowerData> changeEvent)
    {
        HandleSyncedDataUpdates();
    }


    private void HandleSyncedDataUpdates()
    {
        TileBase currTile = MapManager.getTileInMap(paths, 0, 0);

        if (grid.transform.childCount != SyncedTowers.Count)
        {
            Vector3Int index = SyncedTowers[SyncedTowers.Count-1].cellIndex;

            Tilemap newTile = Instantiate(tilemapPrefab);
            newTile.SetTile(new Vector3Int(index.x, index.y), currTile); ;
            newTile.name = -index.x + "," + -index.y;
            newTile.transform.SetParent(grid.transform, false);
        }

            
            
        
    }

    //Get index relative to grid using mouse position
    private Vector3Int getCellIndexFromMouse()
    {
        Vector3 mousePos = Input.mousePosition;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));

        Vector3Int cellIndex = grid.WorldToCell(worldPos);

        cellIndex.x += 1;
        cellIndex.y += 1;

        return cellIndex;

    }
}


//Tower Data Structure, store anything related to the tower here
public struct TowerData : INetworkSerializable, System.IEquatable<TowerData>
{
    public Vector3Int cellIndex;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out cellIndex);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(cellIndex);
        }
    }
    public bool Equals(TowerData other)
    {
        return cellIndex == other.cellIndex;
    }

}
