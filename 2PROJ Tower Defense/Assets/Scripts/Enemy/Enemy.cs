using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Netcode.Components;

public class Enemy : NetworkBehaviour
{
    private GameObject target;

    private Waypoints path;
    private int currentWaypoint = 1;

    private float speed = 5f;
    private float minDistance = 0.2f;
    private int damage = 10;
    private int gold = 5;
    private int difficulty = 1;

    public GameObject deathParticles;
    private float maxHealth = 1;
    private NetworkVariable<float> currentHealth;
    [SerializeField] private Image healthbar;

    private void Awake()
    {
        target = GameObject.Find("Base");
        currentHealth = new NetworkVariable<float>(maxHealth);
    }

    private void Start()
    {
    }

    public void SetPath(Waypoints path)
    {
        this.path = path;
    }

    public Waypoints GetWaypoints()
    {
        return path;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        currentHealth.OnValueChanged += OnDamageTaken;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        currentHealth.OnValueChanged -= OnDamageTaken;
    }

    public void OnDamageTaken(float prev, float curr)
    {
        healthbar.fillAmount = curr / maxHealth;

        //gameObject.transform.Find("Sprite").GetComponent<Light>().color = Color.red;
        //Thread.Sleep(500); doesn't work
        //gameObject.transform.Find("Sprite").GetComponent<Light>().color = Color.white;

    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {

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
            
                DealDamageToBase();

            }

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
        target.GetComponent<Base>().TakeDamageServerRpc(damage);
        gameObject.GetComponent<NetworkObject>().Despawn(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int amount)
    {
        currentHealth.Value -= amount;

        Debug.Log("Health : "+ currentHealth.Value);

        if (currentHealth.Value <= 0)
        {
            GameObject deathEffect = (GameObject)Instantiate(deathParticles,transform.position,Quaternion.identity);
            Destroy(deathParticles,1f);
            gameObject.GetComponent<NetworkObject>().Despawn(true);
        }
    }

    
}
