using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using TMPro;

public class CopyTextOnClick : MonoBehaviour
{
    public TMP_Text textToCopy;


    public void OnClick()
    {
        Debug.Log("clicked");
        GUIUtility.systemCopyBuffer = textToCopy.text;
    }
}
