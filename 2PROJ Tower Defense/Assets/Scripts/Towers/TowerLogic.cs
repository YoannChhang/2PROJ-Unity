using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum TowerTarget
{
    First,
    Last,
    Strongest,
    Weakest,
    Closest
}

public class TowerLogic : MonoBehaviour
{
    private List<GameObject> enemiesInRadius = new List<GameObject>();

    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float detectionInterval = 0.25f;

    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private float shootInterval = 2f;
    private int attackDamage = 5;

    [SerializeField] private TowerTarget targetType = TowerTarget.First;
    private bool shooting = false;



    private void Start()
    {

        if (NetworkManager.Singleton.IsServer)
        {
            InvokeRepeating("DetectEnemiesInRadius", 0f, detectionInterval);
        }
    }
    private void Update()
    {

        if (enemiesInRadius.Count > 0 && NetworkManager.Singleton.IsServer)
        {
            // There are enemies in the detection radius, do something
            
            if (!shooting)
            {
                shooting = true;
                StartCoroutine(SpawnAttacks());
            }

        }
    }

    #region AttackTarget

    private void DetectEnemiesInRadius()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        //Debug.Log("Enemies : " + enemies.Length);
        enemiesInRadius.Clear();

        foreach (GameObject enemy in enemies)
        {

            //Debug.Log(GetEnemyDistance(enemy));
            if (GetEnemyDistance(enemy) <= detectionRadius)
            {
                if (enemiesInRadius.Contains(enemy) == false)
                {
                    enemiesInRadius.Add(enemy);
                }
            }

            else
            {
                if (enemiesInRadius.Contains(enemy))
                {
                    enemiesInRadius.Remove(enemy);
                }
            }

        }

    }

    private IEnumerator SpawnAttacks()
    {

        GameObject target = GetTarget(enemiesInRadius);

        //Quaternion rotation = Quaternion.Euler(-45f, 0f, 0f);
        //Vector3 attackPos = new Vector3(0f, 0f, 0f);
        //GameObject newAttack = Instantiate(attackPrefab, attackPos, rotation);
        //newAttack.GetComponent<NAME>().target = target;
        //newAttack.GetComponent<NetworkObject>().Spawn(newAttackNetObject);

        DealDamageToEnemy(target);

        yield return new WaitForSeconds(shootInterval);

        shooting = false;

    }

    void DealDamageToEnemy(GameObject enemy)
    {
        Enemy e = enemy.GetComponent<Enemy>();
        if (e != null){

            e.TakeDamageServerRpc(attackDamage);

        } else {
            Debug.LogError("No script ennemy");
        }
        
    }

    #endregion

    #region GetTarget
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
    private float GetEnemyDistance(GameObject target)
    {
        return HelperFunctions.get_isometric_distance(transform.position, target.transform.position);
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
        float currDistance = HelperFunctions.get_isometric_distance(transform.position, CloseTarget.transform.position);
        for (int i = 1; i < targets.Count; i++)
        {

            float newDistance = GetEnemyDistance(targets[i]);

            if (currDistance > newDistance)
            {
                CloseTarget = targets[i];
                currDistance = newDistance;
            }
        }
        return CloseTarget;
    }

    #endregion



    
    private void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.Euler(-45f, 0f, 0f), Vector3.one);
        Gizmos.DrawWireSphere(Vector3.zero, detectionRadius);
    }

}


public enum TowerType
{
    Arrow,
    Mage,
    Rocket,
    Soldier
}