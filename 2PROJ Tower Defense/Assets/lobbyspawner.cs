using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class lobbyspawner : MonoBehaviour
{


    [SerializeField] public GameObject playerManagerPrefab;


    void Update()
    {
        
            if (NetworkManager.Singleton.IsServer)
            {
                if (GameObject.Find("PlayerManager") != null)
                {
                    GameObject gameManager = Instantiate(playerManagerPrefab, Vector3.zero, Quaternion.identity);
                    gameManager.gameObject.name = "PlayerManager";
                    gameManager.GetComponent<NetworkObject>().Spawn();
                }
            }
        

    }
}

