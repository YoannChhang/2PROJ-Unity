using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class CircleNetworkObjectSpawner : MonoBehaviour
{


    [SerializeField] private GameObject towerMapPrefab;
    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private GameObject spawnerPrefab;

    [SerializeField] private GameObject soundManagerPrefab;

    [SerializeField] private Waypoints outerLoop;
    [SerializeField] private Waypoints innerLoop;

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
            gameManager.gameObject.name = "GameManager";
            gameManager.GetComponent<GameManager>().startEnemyCheck();
            gameManager.GetComponent<NetworkObject>().Spawn();

            GameObject towerMap = Instantiate(towerMapPrefab, Vector3.zero, Quaternion.identity);
            towerMap.GetComponent<NetworkObject>().Spawn();
            towerMap.gameObject.name = "TowerMap";
            
            PlayerManager playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();

            int count = playerManager.SyncedPlayers.Count;

            // Define a mapping between count and spawner names and waypoints
            Dictionary<int, (string spawnerName, string waypointName)> spawnerMapping = new Dictionary<int, (string, string)>
            {
                { 1, ("Spawner Outer North", "Outer North Main Waypoint") },
                { 2, ("Spawner Outer South", "Outer South Main Waypoint") },
                { 3, ("Spawner Outer East", "Outer East Main Waypoint") },
                { 4, ("Spawner Outer West", "Outer West Main Waypoint") },
                { 5, ("Spawner Center North", "Center North Main Waypoint") },
                { 6, ("Spawner Center South", "Center South Main Waypoint") },
                { 7, ("Spawner Center East", "Center East Main Waypoint") },
                { 8, ("Spawner Center West", "Center West Main Waypoint") },
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

                CircleWaveSpawner waves = spawner.gameObject.GetComponent<CircleWaveSpawner>();
                waves.SetPath(waypoints);

                if (i <= 4) { waves.SetLoop(innerLoop); }
                else if (i >= 5) { waves.SetLoop(outerLoop); }

            }

        }
    }
}
    
