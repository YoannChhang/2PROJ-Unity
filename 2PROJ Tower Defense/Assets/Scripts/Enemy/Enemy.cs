using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Netcode.Components;
using System;

public class Enemy : NetworkBehaviour
{
    private GameObject target;

    private Waypoints path;
    private int currentWaypoint = 1;

    public float speed ;
    private float minDistance = 0.2f;
    public int damage ;
    public int gold ;
    private int difficulty = 1;

    [SerializeField] private GameObject deathParticles;
    private PlayerManager playerManager;

    public float maxHealth;
    private NetworkVariable<float> currentHealth;
    [SerializeField] private Image healthbar;

    public float maxShield;
    private NetworkVariable<float> currentShield;
    [SerializeField] private Image shieldbar;

    private List<RegenerationBuff> activeBuffs = new List<RegenerationBuff>();

    private void Awake()
    {
        target = GameObject.Find("Base");
        currentHealth = new NetworkVariable<float>(maxHealth);
        currentShield = new NetworkVariable<float>(maxShield);
    }

    private void Start()
    {
        playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
        TestRegenerationBuff();
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
        shieldbar.fillAmount = currentShield.Value / maxShield;
        //gameObject.transform.Find("Sprite").GetComponent<Light>().color = Color.red;
        //Thread.Sleep(500); doesn't work
        //gameObject.transform.Find("Sprite").GetComponent<Light>().color = Color.white;

    }

    private void Update()
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


        Debug.Log("Health : " + currentHealth.Value);
        Debug.Log("Shield : " + currentShield.Value);

        string playerName = PlayerPrefs.GetString("PLAYER_NAME");
        Nullable<PlayerData> playerData = playerManager.GetCurrentPlayerData();


        if (currentShield.Value > 0)
            {
                currentShield.Value -= amount;
                shieldbar.fillAmount = currentShield.Value / maxShield;
                if (currentShield.Value <= 0)
                {
                    GameObject deathEffect = (GameObject)Instantiate(deathParticles,transform.position,Quaternion.identity);
                    deathEffect.GetComponent<NetworkObject>().Spawn();
                    deathEffect.GetComponent<NetworkObject>().Despawn(true);
                }
                
            }
            else
            {
                currentHealth.Value -= amount;
                healthbar.fillAmount = currentHealth.Value / maxHealth;
                if (currentHealth.Value <= 0)
                {
                    GameObject deathEffect = (GameObject)Instantiate(deathParticles,transform.position,Quaternion.identity);
                    deathEffect.GetComponent<NetworkObject>().Spawn();
                    deathEffect.GetComponent<NetworkObject>().Despawn(true);
                    //Destroy(deathParticles,1f);
                    gameObject.GetComponent<NetworkObject>().Despawn(true);
                    

                    if (playerData.HasValue && playerData.Value.name == playerName)
                    {
                        playerManager.SetPlayerAttributeServerRpc(playerData.Value.name, playerData.Value.money + 5);
                        playerManager.SetPlayerSpecificStatServerRpc(playerData.Value.name, enemy_killed:1, damage_dealt: amount, money_made: 5);
                        
                    }
                }else{
                    {
                        if (playerData.HasValue && playerData.Value.name == playerName)
                        {
                            playerManager.SetPlayerSpecificStatServerRpc(playerData.Value.name, damage_dealt: amount);

                        }

                    }
                }
            }





        // if (currentHealth.Value <= 0)
        // {
        //     GameObject deathEffect = (GameObject)Instantiate(deathParticles,transform.position,Quaternion.identity);
        //     deathEffect.GetComponent<NetworkObject>().Spawn();
        //     deathEffect.GetComponent<NetworkObject>().Despawn(true);
        //     //Destroy(deathParticles,1f);
        //     gameObject.GetComponent<NetworkObject>().Despawn(true);
            

        //     if (playerData.HasValue && playerData.Value.name == playerName)
        //     {
        //         playerManager.SetPlayerAttributeServerRpc(playerData.Value.name, playerData.Value.money + 5);
        //         playerManager.SetPlayerSpecificStatServerRpc(playerData.Value.name, enemy_killed:1, damage_dealt: amount, money_made: 5);
                
        //     }
        // }
        // else
        // {
        //     if (playerData.HasValue && playerData.Value.name == playerName)
        //     {
        //         playerManager.SetPlayerSpecificStatServerRpc(playerData.Value.name, damage_dealt: amount);

        //     }

        // }

        

}
    public void ActivateRegenerationBuff(float regenAmount, float duration)
        {
            RegenerationBuff regenBuff = new RegenerationBuff(regenAmount, duration);
            activeBuffs.Add(regenBuff);
        }

    private void ApplyRegenerationBuffs()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            RegenerationBuff buff = activeBuffs[i];
            buff.duration -= Time.deltaTime;

            if (buff.duration <= 0)
            {
                activeBuffs.RemoveAt(i);
            }
            else
            {
                currentHealth.Value = Mathf.Clamp(currentHealth.Value + buff.regenAmount * Time.deltaTime, 0f, maxHealth);
            }
        }
    }

    private void TestRegenerationBuff()
{
    float regenAmount = 100f; // Montant de régénération par seconde
    float duration = 5f; // Durée du buff en secondes

    ActivateRegenerationBuff(regenAmount, duration);
}

}
public class RegenerationBuff
{
    public float regenAmount;
    public float duration;

    public RegenerationBuff(float amount, float time)
    {
        regenAmount = amount;
        duration = time;
    }
}

