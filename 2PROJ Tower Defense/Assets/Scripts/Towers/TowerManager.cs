using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;





public class TowerManager : NetworkBehaviour
{
    private GameObject game;
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap baseTilemap;
    [SerializeField] private Tilemap midTilemap;
    [SerializeField] private Tilemap weaponTilemap;

    [SerializeField] private Tilemap paths;
    [SerializeField] private Tilemap towers;
    
    private NetworkList<TowerData> SyncedTowers;

    void Awake()
    {
        SyncedTowers = new NetworkList<TowerData>();
    }

    void Start()
    {
        
    }

    private void Update()
    {
        if (!this.NetworkObject.IsSpawned && NetworkManager.Singleton.IsServer)
        {
            try
            {
                this.NetworkObject.Spawn();
                Debug.Log("Spawned");
            }
            catch
            {

            }
        }

        if (gameObject.name != "TowerMap")
        {
            gameObject.name = "TowerMap";
        }

        if (game == null)
        {
            game = GameObject.Find("GameManager");
        }

        else
        
        {

            if (!game.GetComponent<GameManager>().IsPaused() || !game.GetComponent<GameManager>().IsOver())
            {

                if (Input.GetMouseButtonDown(0))
                {


                    Vector3Int cellIndex = getCellIndexFromMouse();

                    var newTower = new TowerData();
                    newTower.cellIndex = cellIndex;
                    AddTowerServerRpc(newTower);


                }
            }
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

        TileBase tile = MapManager.getTileInMap(towers, 0, 0);
        TileBase mid = MapManager.getTileInMap(towers, 2, 0);
        TileBase top = MapManager.getTileInMap(towers, 4, 0);

        int nbChildren = grid.transform.childCount;

        for (int i = nbChildren - 1; i >= 0; i--)
        {
            DestroyImmediate(grid.transform.GetChild(i).gameObject);
        }

        foreach (TowerData tower in SyncedTowers)
        {
            GameObject sortObject = new GameObject();
            sortObject.name = "Tower " + -tower.cellIndex.x + "," + -tower.cellIndex.y;
            sortObject.transform.SetParent(grid.transform, false);

            Vector3 cellWorldPos = grid.GetCellCenterWorld(new Vector3Int(tower.cellIndex.x, tower.cellIndex.y));
            Quaternion rotation = Quaternion.Euler(-45f, 0f, 0f);


            GameObject TowerLogic = Instantiate(towerPrefab, cellWorldPos, rotation, sortObject.transform);
            TowerLogic.name = "Logic";

            Tilemap baseTile = Instantiate(baseTilemap);
            baseTile.SetTile(new Vector3Int(tower.cellIndex.x, tower.cellIndex.y), tile);
            baseTile.name = "Base";
            baseTile.transform.SetParent(sortObject.transform, false);
            baseTile.GetComponent<TilemapRenderer>().sortingOrder = -tower.cellIndex.x + -tower.cellIndex.y;

            Tilemap midTile = Instantiate(midTilemap);
            midTile.SetTile(new Vector3Int(tower.cellIndex.x, tower.cellIndex.y), mid);
            midTile.name = "Mid";
            midTile.transform.SetParent(sortObject.transform, false);
            midTile.GetComponent<TilemapRenderer>().sortingOrder = -tower.cellIndex.x + -tower.cellIndex.y;

            Tilemap weaponTile = Instantiate(weaponTilemap);
            weaponTile.SetTile(new Vector3Int(tower.cellIndex.x, tower.cellIndex.y), top);
            weaponTile.name = "Weapon";
            weaponTile.transform.SetParent(sortObject.transform, false);
            weaponTile.GetComponent<TilemapRenderer>().sortingOrder = -tower.cellIndex.x + -tower.cellIndex.y;
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
