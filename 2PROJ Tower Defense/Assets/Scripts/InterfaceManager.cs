using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using System;

public class InterfaceManager : MonoBehaviour
{

    [SerializeField] private GameObject LevelNameText;
    [SerializeField] private GameObject GoldText;
    [SerializeField] private GameObject PlayerText;

    //Get using GameObject.Find("InterfaceManager").GetComponentInChildren<InterfaceManager>.SelectedTowerType
    public TowerType SelectedTowerType = TowerType.Arrow;

    // Update is called once per frame
    void Update()
    {
        
        LevelNameText.GetComponentInChildren<TMP_Text>().text = PlayerPrefs.GetString("SELECTED_MODE", "Level 1");
        GoldText.GetComponentInChildren<TMP_Text>().text = "Gold : 243";
        if (NetworkManager.Singleton)
        {
            PlayerText.GetComponentInChildren<TMP_Text>().text =
                $"Players : {GameObject.Find("PlayerManager").GetComponentInChildren<PlayerManager>().SyncedPlayers.Count}";
        }

    }

    //Get using GameObject.Find("InterfaceManager").GetComponentInChildren<InterfaceManager>.SelectedTowerType
    public void SelectTower(string towerType)
    {
        SelectedTowerType = (TowerType)Enum.Parse(typeof(TowerType), towerType);
        Debug.Log($"Selected > {SelectedTowerType}");
    }
}
