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
    [SerializeField] private HoverMouse hoverManager;

    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap baseTilemap;
    [SerializeField] private Tilemap midTilemap;
    [SerializeField] private Tilemap weaponTilemap;

    [SerializeField] private Tilemap paths;
    [SerializeField] private Tilemap towers;

    [SerializeField] private List<GameObject> towersList;
    [SerializeField] private List<GameObject> weaponsList;

    private Dictionary<Vector3Int, TowerData> towerData;
    private NetworkList<TowerData> SyncedTowers;
    private static NetworkVariable<int> lastTowerId;



    void Awake()
    {
        lastTowerId = new NetworkVariable<int>(0);
        SyncedTowers = new NetworkList<TowerData>();
        towerData = new Dictionary<Vector3Int, TowerData>();

    }

    void Start()
    {
        
    }

    private void Update()
    {
        //if (!this.NetworkObject.IsSpawned && NetworkManager.Singleton.IsServer)
        //{
        //    try
        //    {
        //        this.NetworkObject.Spawn();
        //        Debug.Log("Spawned");
        //    }
        //    catch
        //    {

        //    }
        //}

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

            if (!game.GetComponent<GameManager>().IsPaused() && !game.GetComponent<GameManager>().IsOver())
            {

                Vector3Int cellIndex = hoverManager.getCellIndexFromMouse();
                bool available = hoverManager.checkTileAvailability(cellIndex);

                if (Input.GetMouseButtonDown(0) && available)
                {

                    var newTower = new TowerData();
                    newTower.cellIndex = cellIndex;
                    newTower.type = 0;


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
        if (NetworkManager.Singleton.IsClient)
        {
            SyncedTowers.OnListChanged += OnClientListChanged;
        }
        if (NetworkManager.Singleton.IsServer)
        {
            SyncedTowers.OnListChanged += OnServerListChanged;

        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (NetworkManager.Singleton.IsClient)
        {
            SyncedTowers.OnListChanged -= OnClientListChanged;
        }
        if (NetworkManager.Singleton.IsServer)
        {
            SyncedTowers.OnListChanged -= OnServerListChanged;

        }
    }

    //This is made so that the client can modify NetworkVariable
    [ServerRpc(RequireOwnership = false)]
    void AddTowerServerRpc(TowerData newTower)
    {
        // Add the new tower to the SyncedTowers list
        newTower.id = ++lastTowerId.Value;
        SyncedTowers.Add(newTower);
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateTowerServerRpc(TowerData currTower, TowerData changeTower)
    {

        SyncedTowers.Remove(currTower);
        SyncedTowers.Add(changeTower);

    }

    [ServerRpc(RequireOwnership = false)]
    void RemoveTowerServerRpc(TowerData removedTower)
    {
        SyncedTowers.Remove(removedTower);
    }
    

    void OnServerListChanged(NetworkListEvent<TowerData> changeEvent)
    {
        UpdateDictionary();
        //HandleSyncedDataUpdates();
    }

    void OnClientListChanged(NetworkListEvent<TowerData> changeEvent)
    {
        UpdateDictionary();
        //HandleSyncedDataUpdates();
    }

    void UpdateDictionary()
    {

        foreach(TowerData tower in SyncedTowers)
        {
            // Check if the tower is in the dictionary
            if (!towerData.ContainsValue(tower))
            {
                // If not, add / update the tower data to the dictionary with the cell index as the key
                towerData[tower.cellIndex] = tower;
                UpdateDisplayTower(tower);
            }
        }

    }

    void UpdateDisplayTower(TowerData tower)
    {

        // TODO Get Tower prefab then display it with network control and ballista rotation.

        GameObject towerPrefab = GetTowerPrefab(tower.towerLevel, tower.baseLevel);
        GameObject weaponPrefab = GetWeaponPrefab(tower.type);

        string name = "Tower " + -tower.cellIndex.x + "," + -tower.cellIndex.y;
        GameObject existingTower = GameObject.Find("TowerMap").transform.Find(name)?.gameObject;

        if (existingTower != null)
        {
            existingTower.GetComponent<NetworkObject>().Despawn(true);
        }


        // Spawns Tower
        Vector3Int correctIndex = tower.cellIndex;
        correctIndex.x += 1;
        correctIndex.y -= 1;

        Vector3 cellWorldPos = grid.GetCellCenterWorld(correctIndex);
        Quaternion rotation = Quaternion.Euler(-45f, 0f, 0f);
        GameObject newTower = Instantiate(towerPrefab, cellWorldPos, rotation);
        newTower.GetComponent<NetworkObject>().Spawn(true);
        newTower.name = "Tower " + -tower.cellIndex.x + "," + -tower.cellIndex.y;
        newTower.transform.SetParent(grid.transform, false);

        // Spawns Weapon
        Vector3 weaponRelativePos = new Vector3((float)-1.52, (float)0.26);
        GameObject newWeapon = Instantiate(weaponPrefab, weaponRelativePos, Quaternion.identity, newTower.transform);
        newWeapon.GetComponent<NetworkObject>().Spawn(true);
        newWeapon.name = "weapon";
        newWeapon.transform.SetParent(newTower.transform, false);
    }

    GameObject GetTowerPrefab(int towerLevel, int baseLevel)
    {
        int index = (int)GetTowerIndex(towerLevel, baseLevel);

        return towersList[index];
    }

    int? GetTowerIndex(int towerLevel, int baseLevel)
    {
        switch (towerLevel, baseLevel)
        {
            case (0, 0):
                return 0;

            default:
                return null;
        }
    }

    GameObject GetWeaponPrefab(int type)
    {
        int index = (int)GetWeaponIndex(type);

        return weaponsList[index];
    }
        
    int? GetWeaponIndex(int type)
    {
        switch (type)
        {
            case 0:
                return 0;

            default:
                return null;
        }
    }

    //private void HandleSyncedDataUpdates()
    //{

    //    int nbChildren = grid.transform.childCount;

    //    for (int i = nbChildren - 1; i >= 0; i--)
    //    {
    //        DestroyImmediate(grid.transform.GetChild(i).gameObject);
    //    }

    //    foreach (TowerData tower in SyncedTowers)
    //    {
    //        GameObject sortObject = new GameObject();
    //        sortObject.name = "Tower " + -tower.cellIndex.x + "," + -tower.cellIndex.y;
    //        sortObject.transform.SetParent(grid.transform, false);

    //        Vector3 cellWorldPos = grid.GetCellCenterWorld(new Vector3Int(tower.cellIndex.x, tower.cellIndex.y));
    //        Quaternion rotation = Quaternion.Euler(-45f, 0f, 0f);


    //        GameObject TowerLogic = Instantiate(towerPrefab, cellWorldPos, rotation, sortObject.transform);
    //        TowerLogic.name = "Logic";

    //        Tilemap baseTile = Instantiate(baseTilemap);
    //        baseTile.SetTile(new Vector3Int(tower.cellIndex.x, tower.cellIndex.y), tile);
    //        baseTile.name = "Base";
    //        baseTile.transform.SetParent(sortObject.transform, false);
    //        baseTile.GetComponent<TilemapRenderer>().sortingOrder = -tower.cellIndex.x + -tower.cellIndex.y;

    //        Tilemap midTile = Instantiate(midTilemap);
    //        midTile.SetTile(new Vector3Int(tower.cellIndex.x, tower.cellIndex.y), mid);
    //        midTile.name = "Mid";
    //        midTile.transform.SetParent(sortObject.transform, false);
    //        midTile.GetComponent<TilemapRenderer>().sortingOrder = -tower.cellIndex.x + -tower.cellIndex.y;

    //        Tilemap weaponTile = Instantiate(weaponTilemap);
    //        weaponTile.SetTile(new Vector3Int(tower.cellIndex.x, tower.cellIndex.y), top);
    //        weaponTile.name = "Weapon";
    //        weaponTile.transform.SetParent(sortObject.transform, false);
    //        weaponTile.GetComponent<TilemapRenderer>().sortingOrder = -tower.cellIndex.x + -tower.cellIndex.y;
    //    }

    //}
}


//Tower Data Structure, store anything related to the tower here
public struct TowerData : INetworkSerializable, System.IEquatable<TowerData>
{   
    public int id;
    public int type;
    public Vector3Int cellIndex;
    public int baseLevel;
    public int towerLevel;

    public TowerData(
        int id,
        int type,
        Vector3Int cellIndex,
        int baseLevel = 0,
        int towerLevel = 0
        )
    {
        this.id = id;
        this.type = type;
        this.cellIndex = cellIndex;
        this.baseLevel = baseLevel;
        this.towerLevel = towerLevel;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();

            reader.ReadValueSafe(out id);
            reader.ReadValueSafe(out type);
            reader.ReadValueSafe(out cellIndex);
            reader.ReadValueSafe(out baseLevel);
            reader.ReadValueSafe(out towerLevel);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(id);
            writer.WriteValueSafe(type);
            writer.WriteValueSafe(cellIndex);
            writer.WriteValueSafe(baseLevel);
            writer.WriteValueSafe(towerLevel);

        }
    }
    public bool Equals(TowerData other)
    {
        return (id == other.id 
                && type == other.type 
                && cellIndex == other.cellIndex 
                && baseLevel == other.baseLevel 
                && towerLevel == other.towerLevel
               );
    }

}
