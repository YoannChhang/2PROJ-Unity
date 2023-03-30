
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

    // Gets the distance between two objects in isometric space (offset of -45° on the x-axis)
    public static float get_isometric_distance(Vector3 objPosCenter, Vector3 objPosOuter)
    {
        float dx = Mathf.Abs(objPosCenter.x - objPosOuter.x);
        float dy = Mathf.Abs(objPosCenter.y - objPosOuter.y);
        float dz = Mathf.Abs(objPosCenter.z - objPosOuter.z);

        float distance = Mathf.Sqrt(2) * Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
        return distance;
    }

}
