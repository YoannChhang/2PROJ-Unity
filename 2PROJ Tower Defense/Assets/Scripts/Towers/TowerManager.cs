using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
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


        //TODO : To be removed, this is so when game start you have money but this should be correctly implemented in WaveManager
        GameObject.Find("PlayerManager").GetComponentInChildren<PlayerManager>().SetMoneyAllServerRpc(150);
        
    }

    void Start()
    {
        
    }

    private void Update()
    {

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

                    ///Checks before placing tower

                    //Get Tower Info
                    TowerType SelectedTower = GameObject.Find("Interface").GetComponentInChildren<InterfaceManager>().SelectedTowerType;
                    TowerProperty tp = SelectedTower.GetProperty();

                    //Get Current Money
                    PlayerManager playerManager = GameObject.Find("PlayerManager").GetComponentInChildren<PlayerManager>();

                    PlayerData player = playerManager.GetCurrentPlayerData().GetValueOrDefault();

                    //If balance >= 0 when purchasing tower/upgrade
                    if (player.money - tp.Cost[tp.Level] >= 0)
                    {


                        var newTower = new TowerData();
                        newTower.cellIndex = cellIndex;
                        AddTowerServerRpc(newTower);



                        //Remove player money
                        playerManager.SetPlayerAttributeServerRpc(player.name, player.money - tp.Cost[tp.Level]);

                    }
                    else
                    {
                        Debug.Log("Not enough money to place tower");
                    }




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

        string name = "Tower " + -removedTower.cellIndex.x + "," + -removedTower.cellIndex.y;
        GameObject existingTower = GameObject.Find("TowerMap").transform.Find(name)?.gameObject;

        if (existingTower != null)
        {
            var weapon = existingTower.transform.Find("Weapon")?.gameObject;
            if (weapon != null)
            {
                weapon.transform.SetParent(null);
                weapon.GetComponent<NetworkObject>().Despawn(true);
            }
            existingTower.transform.SetParent(null);
            existingTower.GetComponent<NetworkObject>().Despawn(true);
        }

        // Check if the tower is in the dictionary
        if (towerData.ContainsValue(removedTower))
        { 
            // If so, remove it from the dictionary
            towerData.Remove(removedTower.cellIndex);
        }

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
            var weapon = existingTower.transform.Find("Weapon")?.gameObject;
            if (weapon != null)
            {
                weapon.transform.SetParent(null);
                weapon.GetComponent<NetworkObject>().Despawn(true);
            }
            existingTower.transform.SetParent(null);
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
        newWeapon.name = "Weapon";
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

    public void cleanTowers()
    {
        // Create a new NativeList to hold the towers to remove
        using (var towersToRemove = new NativeList<TowerData>(Allocator.Temp))
        {
            // Find and add all towers to remove to the NativeList
            foreach (Transform child in towers.transform)
            {
                if (child.CompareTag("Tower"))
                {
                    var towerPos = grid.WorldToCell(child.position);
                    if (towerData.ContainsKey(towerPos))
                    {
                        var tower = towerData[towerPos];
                        towersToRemove.Add(tower);
                    }
                }
            }

            // Remove all towers from the SyncedTowers list
            foreach (var tower in towersToRemove)
            {
                SyncedTowers.Remove(tower);

                string name = "Tower " + -tower.cellIndex.x + "," + -tower.cellIndex.y;
                GameObject existingTower = GameObject.Find("TowerMap").transform.Find(name)?.gameObject;

                if (existingTower != null)
                {
                    var weapon = existingTower.transform.Find("Weapon")?.gameObject;
                    if (weapon != null)
                    {
                        weapon.transform.SetParent(null);
                        weapon.GetComponent<NetworkObject>().Despawn(true);
                    }
                    existingTower.transform.SetParent(null);
                    existingTower.GetComponent<NetworkObject>().Despawn(true);
                }

                // Remove the tower from the dictionary
                towerData.Remove(tower.cellIndex);
            }
        }
    }


    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        if (NetworkManager.Singleton.IsClient)
        {
            SyncedTowers.OnListChanged -= OnClientListChanged;
        }
        if (NetworkManager.Singleton.IsServer)
        {
            SyncedTowers.OnListChanged -= OnServerListChanged;
        }

        cleanTowers();
        GetComponent<NetworkObject>().Despawn(true);
    }
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
