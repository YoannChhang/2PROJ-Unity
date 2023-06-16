using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class CircleWaveSpawner : WaveSpawner
{
    
    protected Waypoints loop;
    protected GameManager gameManager;

    public void SetLoop(Waypoints path)
    {
        loop = path;
    }

    public Waypoints GetLoop() { return loop; }

    public void SetGame(GameManager game)
    {
        gameManager = game;
    }

    protected override void SpawnEnemy(int wave, int numInWave, int enemyType)
    {
        
        GameObject enemy;

        enemy = Instantiate(enemyList[enemyType], pos, Quaternion.identity);

        // Use enemyType to identify the correct type of the enemy.

        //GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);
        enemy.name = "Enemy " + wave + " " + numInWave;
        enemy.GetComponent<CircleEnemy>().SetPath(waypoints);
        enemy.GetComponent<CircleEnemy>().SetLoop(loop);
        enemy.GetComponent<NetworkObject>().Spawn();
    }

    protected int BonusGold(int waveIndex)
    {
        int golds =  10; 
        return golds;
    }

}
        
