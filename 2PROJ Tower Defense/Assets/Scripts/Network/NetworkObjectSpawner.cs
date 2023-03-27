using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkObjectSpawner : MonoBehaviour
{


    [SerializeField] public GameObject towerMapPrefab;


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
            GameObject towerMap = Instantiate(towerMapPrefab, Vector3.zero, Quaternion.identity);
            towerMap.gameObject.name = "TowerMap";
            towerMap.GetComponent<NetworkObject>().Spawn(true);

            
        }

    }
}
    
