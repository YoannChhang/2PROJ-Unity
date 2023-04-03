using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Enemy : NetworkBehaviour
{
    public Waypoints path;
    public float speed = 5f;
    public float minDistance = 0.1f;
    public int maxHealth = 10;
    public int damage = 1;
    public int gold = 5;
    public int difficulty = 1;

    private int currentHealth;
    private int currentWaypoint = 1;

    private void Start()
    {
        currentHealth = maxHealth;
    
    }

    private void Update()
    {
        if (currentWaypoint < path.waypoints.Length)
        {
            Vector3 targetPosition = path.waypoints[currentWaypoint];

            if (Vector3.Distance(transform.position, targetPosition) < minDistance)
            {
                currentWaypoint++;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
            }
        }
        else
        {
            
            DealDamageToBase();
            Destroy(gameObject);
        }
    }

    public float GetRemainingDistance()
    {
        float remainingDistance = 0f;
        for (int i = currentWaypoint; i < path.waypoints.Length - 1; i++)
        {
            remainingDistance += HelperFunctions.get_isometric_distance(path.waypoints[i], path.waypoints[i + 1]);
        }
        remainingDistance += HelperFunctions.get_isometric_distance(transform.position, path.waypoints[currentWaypoint]);
        return remainingDistance;
    }

    public int GetDifficulty()
    {
        return difficulty;
    }

    private void DealDamageToBase()
    {
        
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log("SSSSSEEESSSS HHHHPPP"+ currentHealth);
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
