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
    [SerializeField] private GameObject pauseMenuPrefab;
    [SerializeField] private GameObject winMenuPrefab;
    [SerializeField] private GameObject loseMenuPrefab;


    // Start is called before the first frame update
    void Start()
    {
        //GameObject parent = GameObject.Find("UI");
        //Debug.Log(parent);

        //GameObject pauseMenu = Instantiate(pauseMenuPrefab, Vector3.zero, Quaternion.identity, parent.transform);
        //pauseMenu.gameObject.name = "PauseMenu";

        //GameObject winMenu = Instantiate(winMenuPrefab, Vector3.zero, Quaternion.identity,parent.transform);
        //winMenu.gameObject.name = "WinMenu";

        //GameObject loseMenu = Instantiate(loseMenuPrefab, Vector3.zero, Quaternion.identity, parent.transform);
        //loseMenu.gameObject.name = "LoseMenu";

        //pauseMenu.SetActive(false);
        //winMenu.SetActive(false);
        //loseMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //if (NetworkManager.Singleton != null)
        //{
        //    if (!NetworkObject.IsSpawned && NetworkManager.Singleton.IsServer)
        //    {
        //        try
        //        {
        //            this.NetworkObject.Spawn();
        //        }
        //        catch
        //        {
        //        }
        //    }
        //}
       
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

            if (Input.GetKeyUp(KeyCode.E))
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

        GameObject pauseMenu = Instantiate(pauseMenuPrefab, default, Quaternion.identity, parent.transform);
        pauseMenu.gameObject.name = "PauseMenu";
        setRectTransformForUI(pauseMenu);

        GameObject winMenu = Instantiate(winMenuPrefab, Vector3.zero, Quaternion.identity, parent.transform);
        winMenu.gameObject.name = "WinMenu";
        setRectTransformForUI(winMenu);

        GameObject loseMenu = Instantiate(loseMenuPrefab, Vector3.zero, Quaternion.identity, parent.transform);
        loseMenu.gameObject.name = "LoseMenu";
        setRectTransformForUI(loseMenu);


        pauseMenu.SetActive(false);
        winMenu.SetActive(false);
        loseMenu.SetActive(false);

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
        isOver = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartMenu");
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void retryServerRpc()
    {
        retryClientRpc();
    }


    [ClientRpc]
    private void endlessClientRpc() 
    {

        GameObject winMenu = GameObject.Find("UI").transform.Find("WinMenu").gameObject;

        isOver = false;
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


}
