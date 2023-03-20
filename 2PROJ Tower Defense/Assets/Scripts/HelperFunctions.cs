
using System;
using UnityEngine;
using UnityEngine.Internal;

public class HelperFunctions : MonoBehaviour
{
    public static void remove_all_childs_from_gameobject(GameObject obj)
    {
        while (obj.transform.childCount > 0)
        {
            UnityEngine.Object.DestroyImmediate(obj.transform.GetChild(0).gameObject);
        }
    }

}
