using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manual_Button: MonoBehaviour
{
    private Button button;
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }
 
    // Update is called once per frame
    void OnClick()
    {
        WaveSpawner.boolStart=true;
    }
}