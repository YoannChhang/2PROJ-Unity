using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public Transform target;
    public float range = 15f;
    public GameObject PartToRotate;
    private Transform partTransform;

    public string ennemyTag ="Ennemy";

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("UpdateTarget", 0f,0.5f);
        partTransform = PartToRotate.GetComponent<Transform>();
    }

    void UpdateTarget(){
        GameObject[] ennemies = GameObject.FindGameObjectsWithTag(ennemyTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnnemy = null;
        foreach (GameObject ennemy in ennemies)
        {
            float distanceToEnnemy = Vector3.Distance(transform.position, ennemy.transform.position);
            if (distanceToEnnemy < shortestDistance)
            {
                shortestDistance = distanceToEnnemy;
                nearestEnnemy = ennemy;
            }
        }
        if (nearestEnnemy != null && shortestDistance <= range)
        {
            target = nearestEnnemy.transform;
        }else
        {
            target = null;
        }
    }
 // Update is called once per frame
    void Update() {
        if (target == null)
        {
            return;
        }

        Vector3 dir = target.position - transform.position;
        Debug.DrawRay(transform.position, dir, Color.red);
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = lookRotation.eulerAngles;
        partTransform.rotation = Quaternion.Euler(0f, rotation.y, 0f);

        Debug.Log("Tourelle: " + transform.position + " | Cible: " + target.position + " | Direction: " + dir);
    }



}
