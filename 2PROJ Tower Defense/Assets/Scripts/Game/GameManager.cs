using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;

public class GameManager : NetworkBehaviour
{

    private bool isPaused = false;
    private bool isOver = false;
    public String name;
    [SerializeField] private GameObject pauseMenuPrefab;
    [SerializeField] private GameObject winMenuPrefab;
    [SerializeField] private GameObject loseMenuPrefab;
    [SerializeField] private GameObject towerOptionsPrefab;


    // Start is called before the first frame update
    void Start()
    {
        name = SceneManager.GetActiveScene().name;
    }

    // Update is called once per frame
    void Update()
    {
       
        if (gameObject.name != "GameManager")
        {
            gameObject.name = "GameManager";
        }

        if (!isOver)
        {

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Debug.Log(!isPaused);
                if (!isPaused)
                {
                    Debug.Log("Test");
                    GamePausedServerRpc();

                } else if (isPaused)
                {
                    Debug.Log("Test2");

                    GameResumedServerRpc();
                }
            }

            // Only works in Green TD and not Circle TD
            if (Input.GetKeyUp(KeyCode.E) || (WaveSpawner.waveIndex > 10 && !WaveSpawner.boolEndless))
            {
                GameWonServerRpc();
            }

            if (Input.GetKey(KeyCode.R))
            {
                GameOverServerRpc();
            }
        }

        
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        void setRectTransformForUI(GameObject menu)
        {
            RectTransform rectTransform = menu.GetComponent<RectTransform>();
            rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, 0);
            rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        GameObject parent = GameObject.Find("UI");
        Debug.Log(parent);

        GameObject pauseMenu = Instantiate(pauseMenuPrefab, Vector3.zero, Quaternion.identity);
        pauseMenu.transform.SetParent(parent.transform, false);
        pauseMenu.gameObject.name = "PauseMenu";
        setRectTransformForUI(pauseMenu);

        GameObject winMenu = Instantiate(winMenuPrefab, Vector3.zero, Quaternion.identity);
        winMenu.transform.SetParent(parent.transform, false);
        winMenu.gameObject.name = "WinMenu";
        setRectTransformForUI(winMenu);

        GameObject loseMenu = Instantiate(loseMenuPrefab, Vector3.zero, Quaternion.identity);
        loseMenu.transform.SetParent(parent.transform, false);
        loseMenu.gameObject.name = "LoseMenu";
        setRectTransformForUI(loseMenu);

        //GameObject towerOptions = Instantiate(towerOptionsPrefab, Vector3.zero, Quaternion.identity, parent.transform);
        //towerOptions.gameObject.name = "TowerOptions";
        //setRectTransformForUI(towerOptions);

        pauseMenu.SetActive(false);
        winMenu.SetActive(false);
        loseMenu.SetActive(false);
        //towerOptions.SetActive(false);

    }

    [ClientRpc]
    private void GameOverClientRpc()
    {
        GameObject loseMenu = GameObject.Find("UI").transform.Find("LoseMenu").gameObject;

        isOver = true;
        loseMenu.SetActive(true);

        if (!NetworkManager.Singleton.IsServer)
        {
            // Disable the buttons for clients
            DisableChildButton(loseMenu, "MenuReturn");
            DisableChildButton(loseMenu, "Retry");
        }

        Time.timeScale = 0f;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void GameOverServerRpc()
    {
        GameOverClientRpc();
    }


    [ClientRpc]
    private void GameWonClientRpc()
    {
        GameObject winMenu = GameObject.Find("UI").transform.Find("WinMenu").gameObject;

        isOver = true;
        winMenu.SetActive(true);

        if (!NetworkManager.Singleton.IsServer)
        {
            // Disable the buttons for clients
            DisableChildButton(winMenu, "MenuReturn");
            DisableChildButton(winMenu, "Continue");
        }

        Time.timeScale = 0f;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void GameWonServerRpc()
    {
        GameWonClientRpc();
    }


    [ClientRpc]
    private void GamePausedClientRpc()
    {
        Debug.Log("Pausing");

        GameObject pauseMenu = GameObject.Find("UI").transform.Find("PauseMenu").gameObject;

        isPaused = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void GamePausedServerRpc()
    {
        Debug.Log("Pausing");

        GamePausedClientRpc();
    }


    [ClientRpc]
    private void GameResumedClientRpc()
    {
        Debug.Log("Resuming");

        GameObject pauseMenu = GameObject.Find("UI").transform.Find("PauseMenu").gameObject;

        isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    [ServerRpc(RequireOwnership = false)]
    public void GameResumedServerRpc()
    {
        Debug.Log("Resuming");

        GameResumedClientRpc();
    }


    [ClientRpc]
    private void returnToMenuClientRpc()
    {

        if (IsServer)
        {
            GameObject.Find("LobbyManager").GetComponent<LobbyManager>().CloseLobby();

        }

        Destroy(GameObject.Find("SceneChangeForDev"));
        Destroy(GameObject.Find("PlayerManager"));
        Destroy(GameObject.Find("LobbyManager"));
        Destroy(GameObject.Find("RelayManager"));
        Destroy(GameObject.Find("TowerMap"));

        NetworkManager.Shutdown();
        Destroy(GameObject.Find("NetworkManager"));

        SceneManager.LoadScene("StartMenu", LoadSceneMode.Single);




        isOver = false;
        Time.timeScale = 1f;

    }


    [ServerRpc(RequireOwnership = false)]
    public void returnToMenuServerRpc()
    {

        returnToMenuClientRpc();
      



    }


    [ClientRpc]
    private void retryClientRpc()
    { 
        isOver = false;
        Time.timeScale = 1f;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void retryServerRpc()
    {
        retryClientRpc();
        destroyTowers();
        NetworkManager.Singleton.SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);

    }


    [ClientRpc]
    private void endlessClientRpc() 
    {

        GameObject winMenu = GameObject.Find("UI").transform.Find("WinMenu").gameObject;

        isOver = false;
        WaveSpawner.boolEndless = true;
        winMenu.SetActive(false);
        Time.timeScale = 1f;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void endlessServerRpc()
    {
        endlessClientRpc();
    }


    public bool IsPaused() { return isPaused; }

    public bool IsOver() { return isOver; }

    private void DisableChildButton(GameObject parent, string buttonName)
    {
        // Find the child button game object
        Transform childTransform = parent.transform.Find(buttonName);
        if (childTransform != null)
        {
            // Get the Button component on the child game object
            Button childButton = childTransform.GetComponent<Button>();
            if (childButton != null)
            {
                // Disable the child button
                childButton.interactable = false;
            }
        }
    }

    private void destroyTowers()
    {
        //Find all objects with the "Tower" tag
        GameObject towers = GameObject.Find("TowerMap").gameObject;

        // Loop through all the towers and destroy them on the network
        towers.GetComponent<TowerManager>().cleanTowers();
        // wait for the towers to be destroyed
        System.Threading.Thread.Sleep(1000);

        towers.GetComponent<NetworkObject>().Despawn(true);
    }

    // Only called in Cricle TD
    public void startEnemyCheck()
    {
        InvokeRepeating("endOnEnemyLimit", 0f, 0.05f);
    }

    private void endOnEnemyLimit()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            int enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
            if (enemyCount > 10)
            {
                GameOverServerRpc();
            }
        }
    }
}
