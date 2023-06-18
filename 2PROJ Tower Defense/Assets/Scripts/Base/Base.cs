using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Base : NetworkBehaviour
{

    public NetworkVariable<int> health;
    private GameManager game;

    private void Awake()
    {
        health = new NetworkVariable<int>(10);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameManager GetGame() 
    {
        return game;
    }

    public void SetGame(GameManager game)
    {
        this.game = game;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        health.OnValueChanged += OnDamageTaken;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        health.OnValueChanged -= OnDamageTaken;
    }

    public void OnDamageTaken(int prev, int curr)
    {
        
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        health.Value -= damage;
        if (health.Value <= 0)
        {
            game.GameOverServerRpc();
        }
    }
}
