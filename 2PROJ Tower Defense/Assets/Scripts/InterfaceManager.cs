using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using System;

public class InterfaceManager : MonoBehaviour
{

    [SerializeField] private GameObject WaveText;
    [SerializeField] private GameObject MonstersLeft;
    [SerializeField] private GameObject PlayerUIRightPrefab;
    [SerializeField] private GameObject PlayerListGrid;


    //Get using GameObject.Find("InterfaceManager").GetComponentInChildren<InterfaceManager>.SelectedTowerType
    public TowerType SelectedTowerType = TowerType.Arrow;

    // Update is called once per frame
    void Update()
    {

        //TODO WaveNumber


        try {
            WaveText.GetComponent<TMP_Text>().text = "Wave : "+ GameObject.FindObjectOfType<WaveSpawner>().waveIndex.ToString();
        }
        catch (Exception e)
        {
            WaveText.GetComponent<TMP_Text>().text = "Wave : ?";

        }

        //TODO MonstersLeft


        UpdatePlayerList();

    }


   
    //Get using GameObject.Find("InterfaceManager").GetComponentInChildren<InterfaceManager>.SelectedTowerType
    public void SelectTower(string towerType)
    {
        SelectedTowerType = (TowerType)Enum.Parse(typeof(TowerType), towerType);
        Debug.Log($"Selected > {SelectedTowerType}");
    }

    public void UpdatePlayerList()
    {

        try
        {

            // Delete all the children of the PlayerListGrid
            HelperFunctions.remove_all_childs_from_gameobject(PlayerListGrid);

            //Add all player prefabs
            foreach (PlayerData player in GameObject.Find("PlayerManager").GetComponent<PlayerManager>().SyncedPlayers )
            {

                GameObject obj = Instantiate(PlayerUIRightPrefab);
                obj.transform.SetParent(PlayerListGrid.transform, false);
                //Player Name
                string playername = player.name.ToString();
                if (playername == PlayerPrefs.GetString("PLAYER_NAME"))
                {
                    obj.GetComponentsInChildren<TMP_Text>()[0].text = "> " + playername;
                }
                else
                {
                    obj.GetComponentsInChildren<TMP_Text>()[0].text = playername;

                }

                //Player Gold
                obj.GetComponentsInChildren<TMP_Text>()[1].text = player.money.ToString();


            }

        }
        catch
        {

        }
    }

}
