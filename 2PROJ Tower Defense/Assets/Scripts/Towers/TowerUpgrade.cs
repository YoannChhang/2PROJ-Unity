using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerUpgrade : MonoBehaviour
{

    public TowerData currTower;
    private TowerManager manager;
    private Button TopButton;
    private Button BaseButton;
    private Button WeaponButton;
    private TMPro.TextMeshProUGUI Top;
    private TMPro.TextMeshProUGUI Base;
    private TMPro.TextMeshProUGUI Weapon;

    // Start is called before the first frame update
    void Start()
    {
        TopButton = transform.Find("ButtonGrid").transform.Find("Top Upgrade").gameObject.GetComponent<Button>();
        BaseButton = transform.Find("ButtonGrid").transform.Find("Base Upgrade").gameObject.GetComponent<Button>();
        WeaponButton = transform.Find("ButtonGrid").transform.Find("Weapon Upgrade").gameObject.GetComponent<Button>();
        manager = GameObject.Find("TowerMap").GetComponent<TowerManager>();

        Top = TopButton.gameObject.transform.Find("Text (TMP)").GetComponent<TMPro.TextMeshProUGUI>();
        Base = BaseButton.gameObject.transform.Find("Text (TMP)").GetComponent<TMPro.TextMeshProUGUI>(); ;
        Weapon = WeaponButton.gameObject.transform.Find("Text (TMP)").GetComponent<TMPro.TextMeshProUGUI>();

    }

    // Update is called once per frame
    void Update()
    {
        // Checks if clicked outside of UI Frame

        if (Input.GetMouseButtonDown(0))
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), Input.mousePosition))
            {
                manager.changeSelected();
                Destroy(gameObject);
            }
        }

    }

    public void UpdateText()
    {
        string GetUpgradePrice(int[] price, int level)
        {
            switch (level)
            {
                case 2:
                    return "Max";
                default:
                    return price[level].ToString();
            }
        }

        TowerType type = currTower.type;
        TowerProperty properties = type.GetProperty();

        // Update Button Texts

        Top.text = GetUpgradePrice(properties.TopCost, currTower.topLevel);
        Base.text = GetUpgradePrice(properties.BaseCost, currTower.baseLevel);
        Weapon.text = GetUpgradePrice(properties.WeaponCost, currTower.weaponLevel);

        if (Top.text == "Max") { TopButton.interactable = false; }
        if (Base.text == "Max") { BaseButton.interactable = false; }
        if (Weapon.text == "Max") { WeaponButton.interactable = false; }

    }

    public void UpgradeTop()
    {
        TowerData newData = new TowerData(currTower.id,
                                          currTower.type, 
                                          currTower.cellIndex, 
                                          currTower.baseLevel, 
                                          currTower.topLevel + 1,
                                          currTower.weaponLevel);

        manager.UpdateTowerServerRpc(currTower, newData);
        currTower = newData;
        UpdateText();
    }

    public void UpgradeBase()
    {
        TowerData newData = new TowerData(currTower.id,
                                          currTower.type,
                                          currTower.cellIndex,
                                          currTower.baseLevel + 1,
                                          currTower.topLevel,
                                          currTower.weaponLevel);

        manager.UpdateTowerServerRpc(currTower, newData);
        currTower = newData;
        UpdateText();
    }

    public void UpgradeWeapon()
    {
        TowerData newData = new TowerData(currTower.id,
                                          currTower.type,
                                          currTower.cellIndex,
                                          currTower.baseLevel,
                                          currTower.topLevel,
                                          currTower.weaponLevel + 1);

        manager.UpdateTowerServerRpc(currTower, newData);
        currTower = newData;
        UpdateText();
    }
}
