using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TowerRadius : NetworkBehaviour
{
    private List<GameObject> enemiesInRadius = new List<GameObject>();

    public float detectionRadius = 5f;
    public string enemySortingLayerName;
    public float detectionInterval = 0.5f;

    private void Start()
    {
        if (IsServer)
        {
            StartCoroutine(DetectEnemiesInRadius());
        }
    }

    private IEnumerator DetectEnemiesInRadius()
    {
        while (true)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.GetComponent<Renderer>() != null && collider.gameObject.GetComponent<Renderer>().sortingLayerName == enemySortingLayerName)
                {
                    if (!enemiesInRadius.Contains(collider.gameObject))
                    {
                        enemiesInRadius.Add(collider.gameObject);
                    }
                }
            }

            yield return new WaitForSeconds(detectionInterval);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer && other.gameObject.GetComponent<Renderer>() != null && other.gameObject.GetComponent<Renderer>().sortingLayerName == enemySortingLayerName)
        {
            enemiesInRadius.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsServer && enemiesInRadius.Contains(other.gameObject))
        {
            enemiesInRadius.Remove(other.gameObject);
        }
    }

    private void Update()
    {
        if (enemiesInRadius.Count > 0)
        {
            // There are enemies in the detection radius, do something
            Debug.Log("Enemy detected!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
