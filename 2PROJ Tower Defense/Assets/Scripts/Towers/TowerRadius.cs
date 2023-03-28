using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public enum TowerTarget
{
    First,
    Last,
    Strongest,
    Weakest,
    Closest
}

public class TowerRadius : NetworkBehaviour
{
    private List<GameObject> enemiesInRadius = new List<GameObject>();

    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float detectionInterval = 0.5f;
    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private TowerTarget targetType = TowerTarget.First;



    private void Start()
    {
        if (IsServer)
        {
            StartCoroutine(DetectEnemiesInRadius());
            StartCoroutine(SpawnAttacks());
        }
    }

    private IEnumerator DetectEnemiesInRadius()
    {
        while (true)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.layer == LayerMask.GetMask("Enemy"))
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

    private IEnumerator SpawnAttacks()
    {
        while (true)
        {
            if (enemiesInRadius.Count > 0)
            {

                Quaternion rotation = Quaternion.Euler(-45f, 0f, 0f);
                Vector3 attackPos = new Vector3(0f, 0f, 0f);

                GameObject target = GetTarget(enemiesInRadius);

                GameObject newAttack = Instantiate(attackPrefab, attackPos, rotation);
                //newAttack.GetComponent<NAME>.target = target;
                NetworkObject newAttackNetObject = newAttack.GetComponent<NetworkObject>();

                NetworkObject.Spawn(newAttackNetObject);

                yield return new WaitForSeconds(shootInterval);

            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer && other.gameObject.layer == LayerMask.GetMask("Enemy"))
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

    private float GetEnemyDistance(GameObject target)
    {
        return Vector3.Distance(transform.position, target.transform.position);
    }

    private GameObject GetTarget(List<GameObject> targets)
    {

        GameObject target = null;

        switch (targetType)
        {
            case TowerTarget.First:
                return GetFirstEnemy(targets);
            case TowerTarget.Last:
                return GetLastEnemy(targets);
            case TowerTarget.Strongest:
                return GetStrongEnemy(targets);
            case TowerTarget.Weakest:
                return GetWeakEnemy(targets);
            case TowerTarget.Closest:
                return GetCloseEnemy(targets);

        }

        return target;
    }

    private GameObject GetFirstEnemy(List<GameObject> targets)
    {
        GameObject FirstTarget = targets[0];

        for (int i = 1; i < targets.Count; i++)
        {
            if (FirstTarget.GetComponent<Enemy>().GetRemainingDistance() > targets[i].GetComponent<Enemy>().GetRemainingDistance())
            {
                 FirstTarget = targets[i];
            } 
        }

        return FirstTarget;

    }

    private GameObject GetLastEnemy(List<GameObject> targets)
    {
        GameObject LastTarget = targets[0];
        for (int i = 1; i < targets.Count; i++)
        {
            if (LastTarget.GetComponent<Enemy>().GetRemainingDistance() < targets[i].GetComponent<Enemy>().GetRemainingDistance())
            {
                LastTarget = targets[i];
            }
        }
        return LastTarget;
    }

    private GameObject GetStrongEnemy(List<GameObject> targets)
    {
        GameObject StrongTarget = targets[0];
        for (int i = 1; i < targets.Count; i++)
        {
            if (StrongTarget.GetComponent<Enemy>().GetDifficulty() < targets[i].GetComponent<Enemy>().GetDifficulty())
            {
                StrongTarget = targets[i];
            }
        }
        return StrongTarget;
    }

    private GameObject GetWeakEnemy(List<GameObject> targets)
    {
        GameObject WeakTarget = targets[0];
        for (int i = 1; i < targets.Count; i++)
        {
            if (WeakTarget.GetComponent<Enemy>().GetDifficulty() > targets[i].GetComponent<Enemy>().GetDifficulty())
            {
                WeakTarget = targets[i];
            }
        }
        return WeakTarget;
    }

    private GameObject GetCloseEnemy(List<GameObject> targets)
    {
        GameObject CloseTarget = targets[0];
        for (int i = 1; i < targets.Count; i++)
        {
            if (GetEnemyDistance(CloseTarget) > GetEnemyDistance(targets[i]))
            {
                CloseTarget = targets[i];
            }
        }
        return CloseTarget;
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
