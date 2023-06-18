using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeSelection : MonoBehaviour
{
    public TMP_InputField textInputFieldForUsername;
    public static string SELECTED_MODE = "0";

    public void Awake()
    {
        string player_name = PlayerPrefs.GetString("PLAYER_NAME");
        if (player_name == null || player_name == "")
        {
            player_name = "Player" + UnityEngine.Random.Range(10, 99);
        }
        textInputFieldForUsername.text = player_name;
    }
    //When clicking on a level
    public void OnModeClick(string level)
    {
        Debug.Log("test");
        Debug.Log(FindObjectOfType<SoundManager>());

        FindObjectOfType<SoundManager>().PlaySound("click");
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
        FindObjectOfType<SoundManager>().PlaySound("click");

        SceneManager.LoadScene("StartMenu");
    }

    public void UpdatePlayerUsername()
    {
        Debug.Log(textInputFieldForUsername.text);
        PlayerPrefs.SetString("PLAYER_NAME", textInputFieldForUsername.text);
    }
}
