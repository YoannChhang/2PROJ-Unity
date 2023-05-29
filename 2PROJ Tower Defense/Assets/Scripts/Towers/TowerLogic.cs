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

public enum TowerType
{
    Arrow,
    Cannon,
    Twin,
    Mage,
}

public class TowerLogic : NetworkBehaviour
{
    private List<GameObject> enemiesInRadius = new List<GameObject>();

    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float detectionInterval = 0.25f;

    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private float shootInterval = 2f;
    private int attackDamage = 5;

    [SerializeField] private TowerType type;
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

            GameObject target = GetTarget(enemiesInRadius);
            if (target != null && !GameObject.ReferenceEquals(target, null)){


                if (target.GetComponent<NetworkObject>().IsSpawned)
                {
                    Vector3 relativePosition = target.transform.position - transform.position;
                    Quaternion targetRotation = Quaternion.LookRotation(relativePosition, Vector3.up);
                    float rotationDegree = targetRotation.eulerAngles.y;

                    transform.rotation = Quaternion.Euler(-45f, rotationDegree, 0f);

                    if (!shooting)
                    {
                        shooting = true;
                        StartCoroutine(SpawnAttacks(target));
                    }
                }

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

    private IEnumerator SpawnAttacks(GameObject target)
    {

        switch (type)
        {
            case TowerType.Arrow:

                GameObject newAttack = Instantiate(attackPrefab, transform.position, transform.rotation, transform);
                newAttack.GetComponent<NetworkObject>().Spawn(true);
                StartCoroutine(ProjectileHandler(target, newAttack));
                break;

            case TowerType.Twin:

                MuzzleActiveClientRpc();
                yield return new WaitForSeconds(0.1f);
                MuzzleDeactiveClientRpc();
                DealDamageToEnemy(target);

                yield return new WaitForSeconds(0.1f);

                MuzzleActiveClientRpc();
                yield return new WaitForSeconds(0.1f);
                MuzzleDeactiveClientRpc();
                DealDamageToEnemy(target);

                break;

            case TowerType.Cannon:

                MuzzleActiveClientRpc();
                yield return new WaitForSeconds(0.1f);
                MuzzleDeactiveClientRpc();
                DealDamageToEnemy(target);
                break;

            default:
                DealDamageToEnemy(target);
                break;

        }
        
        yield return new WaitForSeconds(shootInterval);

        shooting = false;

    }
    IEnumerator ProjectileHandler(GameObject target, GameObject projectile)
    {
        float speed = 15f; // Adjust the speed as needed

        Vector3 startPosition = projectile.transform.position;
        Quaternion startRotation = projectile.transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - startPosition, Vector3.up);
        targetRotation.x = -45f;

        float journeyLength = Vector3.Distance(startPosition, target.transform.position);
        float startTime = Time.time;

        while (projectile != null)
        {
            float distCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distCovered / journeyLength;

            projectile.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, fractionOfJourney);
            projectile.transform.position = Vector3.Lerp(startPosition, target.transform.position, fractionOfJourney);

            if (fractionOfJourney >= 1f)
            {
                // Perform the collision action here
                DealDamageToEnemy(target);

                // Destroy the projectile
                projectile.GetComponent<NetworkObject>().Despawn(true);
                yield break;
            }

            yield return null;
        }
    }

    [ClientRpc]
    private void MuzzleActiveClientRpc()
    {
        GameObject muzzleFlash = transform.Find("MuzzleFlash").gameObject;
        muzzleFlash.SetActive(true);
    }

    [ClientRpc]
    private void MuzzleDeactiveClientRpc()
    {
        GameObject muzzleFlash = transform.Find("MuzzleFlash").gameObject;
        muzzleFlash.SetActive(false);
    }

    void DealDamageToEnemy(GameObject enemy)
    {
        Enemy e = enemy.GetComponent<Enemy>();

        e.TakeDamageServerRpc(attackDamage);

        
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


public static class TowerTypeExtension
{
    public static TowerProperty GetProperty(this TowerType towerType)
    {
        switch (towerType)
        {
            case TowerType.Arrow:
                return new ArrowProperty();

            case TowerType.Mage:
                return new MageProperty();

            case TowerType.Cannon:
                return new CannonProperty();

            case TowerType.Twin:
                return new TwinProperty();

            default:
                return null;
        }
    }
}

public abstract class TowerProperty
{
    public TowerType Type { get; protected set; }
    public int Level { get; protected set; }
    public int Damage { get; protected set; }
    public int Cost { get; protected set; }
    public int[] TopCost { get; protected set; }
    public int[] BaseCost { get; protected set; }
    public int[] WeaponCost { get; protected set; }

    public TowerProperty(int level = 0)
    {
        Level = level;
        BaseCost = new int[2] { 100, 150 };
        TopCost = new int[2] { 200, 300 };
    }
}
public class ArrowProperty : TowerProperty
{
    public ArrowProperty() 
    {
        Type = TowerType.Arrow;
        Damage = 1;
        Cost = 120;
        WeaponCost = new int[2] { 160, 200 };
    }
}
public class MageProperty : TowerProperty
{
    public MageProperty()
    {
        Type = TowerType.Mage;
        Damage = 1;
        Cost = 180;
        WeaponCost = new int[2] { 220, 280 };
    }
}
public class CannonProperty : TowerProperty
{
    public CannonProperty()
    {
        Type = TowerType.Cannon;
        Damage = 1;
        Cost = 220;
        WeaponCost = new int[2] { 280, 300 };
    }
}
public class TwinProperty : TowerProperty
{
    public TwinProperty()
    {
        Type = TowerType.Twin;
        Damage = 1;
        Cost = 100;
        WeaponCost = new int[2] { 140, 200 };
    }
}