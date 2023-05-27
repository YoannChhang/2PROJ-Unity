using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Auto_Button : MonoBehaviour
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
        if (WaveSpawner.boolAuto)
        {
            WaveSpawner.boolAuto=false;
        }else
        {
            WaveSpawner.boolAuto=true;
        }
        
    }
}
