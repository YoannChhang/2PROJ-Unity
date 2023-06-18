using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class NetworkObjectSpawner : MonoBehaviour
{


    [SerializeField] private GameObject towerMapPrefab;
    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private GameObject spawnerPrefab;
    [SerializeField] private GameObject basePrefab;
    [SerializeField] private GameObject soundManagerPrefab;


    void Start()
    {
        if (FindObjectOfType<SoundManager>() == null)
        {
            GameObject soundManager = Instantiate(soundManagerPrefab, Vector3.zero, Quaternion.identity);
            soundManager.name = "SoundManager";
        }
        

        //TO RUN GAME QUICKER WHEN DEV
        if (NetworkManager.Singleton == null)
        {
            SceneManager.LoadScene("Lobby");
            return;
        }
        if (NetworkManager.Singleton.IsServer)
        {
            GameObject gameManager = Instantiate(gameManagerPrefab, Vector3.zero, Quaternion.identity);
            gameManager.GetComponent<NetworkObject>().Spawn();
            gameManager.gameObject.name = "GameManager";

            GameObject towerMap = Instantiate(towerMapPrefab, Vector3.zero, Quaternion.identity);
            towerMap.GetComponent<NetworkObject>().Spawn();
            towerMap.gameObject.name = "TowerMap";

            Grid grid = towerMap.GetComponent<Grid>();
            Quaternion rotation = Quaternion.Euler(-45f, 0f, 0f);
            Vector3 cellWorldPos = grid.GetCellCenterWorld(new Vector3Int(-19, -43, -1));
            cellWorldPos.z = -1;
            GameObject @base = Instantiate(basePrefab, cellWorldPos, rotation);
            @base.GetComponent<Base>().SetGame(gameManager.GetComponent<GameManager>());
            @base.GetComponent<NetworkObject>().Spawn();
            @base.name = "Base";
            @base.transform.SetParent(grid.transform);

            PlayerManager playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();

            int count = playerManager.SyncedPlayers.Count;
            Debug.Log(count);

            // Define a mapping between count and spawner names and waypoints
            Dictionary<int, (string spawnerName, string waypointName)> spawnerMapping = new Dictionary<int, (string, string)>
            {
                { 1, ("Spawner UpperLeft", "Waypoints Upper Left") },
                { 2, ("Spawner UpperRight", "Waypoints Upper Right") },
                { 3, ("Spawner LowUpperLeft", "Waypoints Low Upper Left") },
                { 4, ("Spawner Low UpperRight", "Waypoints Low Upper Right") },
                { 5, ("Spawner Low LowerLeft", "Waypoints High Lower Left") },
                { 6, ("Spawner Low LowerRight", "Waypoints High Lower Right") },
                { 7, ("Spawner High LowerLeft", "Waypoints Low Lower Left") },
                { 8, ("Spawner High LowerRight", "Waypoints Low Lower Right") },
                { 9, ("Spawner Middle", "Waypoints Middle") },
            };

            
            // Loop through the count and create spawners accordingly
            for (int i = 1; i <= count; i++)
            {
                // Get the spawner name and waypoint name based on the count
                (string spawnerName, string waypointName) = spawnerMapping[i];

                // Instantiate the spawner
                GameObject spawner = Instantiate(spawnerPrefab, Vector3.zero, Quaternion.identity);
                spawner.gameObject.name = spawnerName;

                // Set the path for the spawner
                Waypoints waypoints = GameObject.Find(waypointName).GetComponent<Waypoints>();

                WaveSpawner waves = spawner.gameObject.GetComponent<WaveSpawner>();
                waves.SetPath(waypoints);

            }

        }
    }
}
    
