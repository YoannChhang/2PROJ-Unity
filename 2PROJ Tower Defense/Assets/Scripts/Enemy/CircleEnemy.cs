using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Netcode.Components;
using System;

public class CircleEnemy : Enemy
{
    protected Waypoints loop;
    protected GameManager gameManager;
    
    public void SetLoop(Waypoints loop)
    {
        this.loop = loop;
    }

    protected override void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            ApplyRegenerationBuffs();
            if (currentWaypoint < path.waypoints.Length)
            {
                Vector3 targetPosition = path.waypoints[currentWaypoint];

                if (Vector3.Distance(transform.position, targetPosition) <= minDistance)
                {
                    currentWaypoint++;
                }
                else
                {
                    //transform.position = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                    
                }
            }
            else
            {
            
                // Switch to loop path

                if (loop != null)
                {
                    path = loop;
                    currentWaypoint = 0;
                }

            }

        }
    }

}
