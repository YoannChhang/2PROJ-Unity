using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkObjectSpawner : MonoBehaviour
{


    [SerializeField] public GameObject towerMapPrefab;


    void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            GameObject towerMap = Instantiate(towerMapPrefab, Vector3.zero, Quaternion.identity);
            towerMap.gameObject.name = "TowerMap";
            towerMap.GetComponent<NetworkObject>().Spawn(true);

        }

    }
}
    
