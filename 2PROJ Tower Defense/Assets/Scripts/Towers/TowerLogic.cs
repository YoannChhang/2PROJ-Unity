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

    private int attackDamage;
    private float detectionRadius;
    private float shootInterval;
    private bool shooting = false;

    [SerializeField] private float detectionInterval = 0.1f;

    [SerializeField] private GameObject attackPrefab;

    [SerializeField] private TowerType type;
    [SerializeField] private TowerTarget targetType = TowerTarget.First;


    private int topLevel = 0;
    private int baseLevel = 0;
    private int weaponLevel = 0;


    private void Start()
    {

        if (NetworkManager.Singleton.IsServer)
        {
            InvokeRepeating("DetectEnemiesInRadius", 0f, detectionInterval);
            MuzzleDeactiveClientRpc();
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
                    Vector3 targetPos = target.transform.position;
                    targetPos.z = 0f;
                    Vector3 selfPos = transform.position;
                    selfPos.z = 0f;

                    Vector3 relativePosition = targetPos - selfPos;
                    Quaternion targetRotation = Quaternion.LookRotation(relativePosition, Vector3.up);
                    float rotationDegree = targetRotation.eulerAngles.y;    
                    float xRotation = -45 * Mathf.Cos(rotationDegree * Mathf.PI / 180);
                    float zRotation = -45 * Mathf.Sin(rotationDegree * Mathf.PI / 180);

                    transform.rotation = Quaternion.Euler(xRotation, rotationDegree, zRotation);
                    //transform.rotation = targetRotation;

                    if (!shooting)
                    {
                        shooting = true;
                        StartCoroutine(SpawnAttacks(target));
                    }
                }

            }
        }
    }

    public void SetTowerData(TowerData tower)
    {

        SetTopLevel(tower.topLevel);
        SetBaseLevel(tower.baseLevel);
        SetWeaponLevel(tower.weaponLevel);

    }

    public void SetTopLevel(int level)
    {
        topLevel = level;

        TowerProperty properties = type.GetProperty();
        detectionRadius = properties.Range[topLevel];
    }

    public void SetBaseLevel(int level)
    {
        baseLevel = level;

        TowerProperty properties = type.GetProperty();
        shootInterval = properties.Speed[baseLevel];
    }

    public void SetWeaponLevel(int level)
    {
        weaponLevel = level;

        TowerProperty properties = type.GetProperty();
        attackDamage = properties.Damage[weaponLevel];
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

    private TowerType GetTowerType()
    {
        return type;
    }

    private IEnumerator SpawnAttacks(GameObject target)
    {

        switch (type)
        {
            case TowerType.Arrow:

                Debug.Log(attackPrefab);
                GameObject newAttack = Instantiate(attackPrefab, transform.position, transform.rotation, transform);
                newAttack.GetComponent<NetworkObject>().Spawn(true);
                StartCoroutine(ProjectileHandler(target, newAttack));
                break;

            case TowerType.Twin:

                MuzzleActiveClientRpc();
                yield return new WaitForSeconds(0.05f);
                MuzzleDeactiveClientRpc();
                DealDamageToEnemy(target);

                yield return new WaitForSeconds(0.05f);

                MuzzleActiveClientRpc();
                yield return new WaitForSeconds(0.05f);
                MuzzleDeactiveClientRpc();
                DealDamageToEnemy(target);

                break;

            case TowerType.Cannon:

                MuzzleActiveClientRpc();
                yield return new WaitForSeconds(0.15f);
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
        if (enemy)
        {
            Enemy e = enemy.GetComponent<Enemy>();
            TowerProperty properties = type.GetProperty();
            e.TakeDamageServerRpc(properties.Damage[weaponLevel]);
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
        Vector3 pos = transform.position;
        pos.z = 0f;
        Gizmos.matrix = Matrix4x4.TRS(pos, Quaternion.Euler(-45f, 0f, 0f), Vector3.one);
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
    public int[] Damage { get; protected set; }
    public float[] Speed { get; protected set; }
    public float[] Range { get; protected set; }
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
        Damage = new int[3] { 10, 20, 50 };
        Range = new float[3] { 2, 3, 4 };
        Speed = new float[3] { 0.5f, 0.25f, 0.1f };

        Cost = 120;
        WeaponCost = new int[2] { 160, 200 };
        TopCost = new int[2] { 200, 300 };
        BaseCost = new int[2] { 150, 350 };
    }
}
public class MageProperty : TowerProperty
{
    public MageProperty()
    {
        Type = TowerType.Mage;

        Damage = new int[3] { 10, 25, 50 };
        Range = new float[3] { 2, 3, 4 };
        Speed = new float[3] { 0.5f, 0.25f, 0.1f };
        Cost = 180;
        WeaponCost = new int[2] { 220, 280 };
    }
}
public class CannonProperty : TowerProperty
{
    public CannonProperty()
    {
        Type = TowerType.Cannon;

        Damage = new int[3] { 30, 60, 90 };
        Range = new float[3] { 0.75f, 1.25f, 2 };
        Speed = new float[3] { 3f, 1.5f, 1f };

        Cost = 220;
        WeaponCost = new int[2] { 280, 350 };
        TopCost = new int[2] { 200, 400 };
        BaseCost = new int[2] { 250, 350 };
    }
}
public class TwinProperty : TowerProperty
{
    public TwinProperty()
    {
        Type = TowerType.Twin;

        Damage = new int[3] { 5, 10, 30 };
        Range = new float[3] { 2.5f, 3.5f, 5f };
        Speed = new float[3] { 0.5f, 0.2f, 0.05f };

        Cost = 100;
        WeaponCost = new int[2] { 140, 200 };
        TopCost = new int[2] { 250, 600 };
        BaseCost = new int[2] { 300, 500 };
    }
}