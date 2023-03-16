using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelection : MonoBehaviour
{
    public static int SELECTED_LEVEL = 0;


    //When clicking on a level
    public void OnLevelClick(int level)
    {

        //TODO : Open a UI with level info and then starting the game.

        SELECTED_LEVEL = level;

        SceneManager.LoadScene("Lobby");
    }
    void OnDisable()
    {
        PlayerPrefs.SetInt("SELECTED_LEVEL", SELECTED_LEVEL);
    }

    //When clicking on return button
    public void OnReturnClick()
    {
        SceneManager.LoadScene("StartMenu");
    }
}
