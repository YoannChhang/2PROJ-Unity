using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using System;

public class InterfaceManager : MonoBehaviour
{

    [SerializeField] private GameObject HealthText;
    [SerializeField] private GameObject WaveText;
    [SerializeField] private GameObject MonstersLeft;
    [SerializeField] private GameObject PlayerUIRightPrefab;
    [SerializeField] private GameObject PlayerListGrid;
    [SerializeField] private GameObject WaveAutoButtonText;


    //Get using GameObject.Find("InterfaceManager").GetComponentInChildren<InterfaceManager>.SelectedTowerType
    public TowerType SelectedTowerType = TowerType.Twin;

    // Update is called once per frame
    void Update()
    {

        if (HealthText != null)
        {
            try
            {
                HealthText.GetComponent<TMP_Text>().text = "Health : " + FindObjectOfType<Base>().health.Value.ToString();
            }
            catch (Exception e)
            {
                HealthText.GetComponent<TMP_Text>().text = "Health : ?";
            }
        }
        try
        {
            WaveText.GetComponent<TMP_Text>().text = "Wave : "+ (WaveSpawner.waveIndex+1).ToString();
        }
        catch (Exception e)
        {
            WaveText.GetComponent<TMP_Text>().text = "Wave : ?";
        }
        try
        {
            MonstersLeft.GetComponent<TMP_Text>().text = "Monsters : " + GameObject.FindGameObjectsWithTag("Enemy").Length;
        }
        catch (Exception e)
        {
            MonstersLeft.GetComponent<TMP_Text>().text = "Monsters : ?";
        }



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

    public void StartWaveButton()
    {
        WaveSpawner.boolStart = true;
    }

    public void ToggleAutomaticWaveButton()
    {
        if (WaveSpawner.boolAuto)
        {
            WaveSpawner.boolAuto = false;
            Color c = WaveAutoButtonText.GetComponent<TMP_Text>().color;
            c.a = 0.5f;
            WaveAutoButtonText.GetComponent<TMP_Text>().color = c;
        }
        else
        {
            WaveSpawner.boolAuto = true;
            Color c = WaveAutoButtonText.GetComponent<TMP_Text>().color;
            c.a = 1f;
            WaveAutoButtonText.GetComponent<TMP_Text>().color = c;
        }
    }
}
