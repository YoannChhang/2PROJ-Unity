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

    private int currentHealth;
    private int currentWaypoint = 0;

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

    private void DealDamageToBase()
    {
        
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
