using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkObjectSpawner : MonoBehaviour
{


    [SerializeField] private GameObject towerMapPrefab;
    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private GameObject basePrefab;
    [SerializeField] private GameObject spawnerPrefab;


    void Start()
    {
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
            gameManager.GetComponent<NetworkObject>().Spawn();

            GameObject towerMap = Instantiate(towerMapPrefab, Vector3.zero, Quaternion.identity);
            towerMap.gameObject.name = "TowerMap";
            towerMap.GetComponent<NetworkObject>().Spawn();

            GameObject baseTower = Instantiate(basePrefab, Vector3.zero, Quaternion.identity);
            baseTower.gameObject.name = "Base";
            baseTower.GetComponent<Base>().SetGame(GameObject.Find("GameManager").GetComponent<GameManager>());
            baseTower.GetComponent<NetworkObject>().Spawn();

            
            GameObject spawner1 = Instantiate(spawnerPrefab, Vector3.zero, Quaternion.identity);
            spawner1.gameObject.name = "Spawner UpperLeft";
            spawner1.gameObject.GetComponent<WaveSpawner>().SetPath(GameObject.Find("Waypoints Upper Left").GetComponent<Waypoints>());

            
            GameObject spawner2 = Instantiate(spawnerPrefab, Vector3.zero, Quaternion.identity);
            spawner2.gameObject.name = "Spawner UpperRight";
            spawner2.gameObject.GetComponent<WaveSpawner>().SetPath(GameObject.Find("Waypoints Upper Right").GetComponent<Waypoints>());
        }
    }
}
    
