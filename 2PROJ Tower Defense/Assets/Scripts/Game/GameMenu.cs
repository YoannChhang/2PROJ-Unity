using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMenu : MonoBehaviour
{

    GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void retry()
    {
        gameManager.retryServerRpc();
    }

    public void quitToMenu()
    {
        gameManager.returnToMenuServerRpc();
    }

    public void endless()
    {
        gameManager.endlessServerRpc();
    }


}
