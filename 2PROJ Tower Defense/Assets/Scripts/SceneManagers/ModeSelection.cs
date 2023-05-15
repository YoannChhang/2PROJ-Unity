using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeSelection : MonoBehaviour
{
    public static string SELECTED_MODE = "0";


    //When clicking on a level
    public void OnModeClick(string level)
    {

        //TODO : Open a UI with level info and then starting the game.

        SELECTED_MODE = level;

        SceneManager.LoadScene("Lobby");
    }
    void OnDisable()
    {
        //Debug.Log("Setting SELECTED_MODE :" + SELECTED_MODE);
        PlayerPrefs.SetString("SELECTED_MODE", SELECTED_MODE);
    }

    //When clicking on return button
    public void OnReturnClick()
    {
        SceneManager.LoadScene("StartMenu");
    }
}
