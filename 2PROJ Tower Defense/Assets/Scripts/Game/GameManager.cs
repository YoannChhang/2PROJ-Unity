using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public bool isPaused = false;
    [SerializeField] private GameObject pauseMenu;

    // Start is called before the first frame update
    void Start()
    {
        pauseMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (!isPaused)
            {
                GamePaused();
            } else if (isPaused)
            {
                GameResumed();
            }
        }
        
    }

    public void GameOver()
    { 
        SceneManager.LoadScene("Lobby");
    }

    public void GameWon()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void GamePaused()
    {
        isPaused = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void GameResumed()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }
}
