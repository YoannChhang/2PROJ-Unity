using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class CircleWaveSpawner : WaveSpawner
{
    
    protected Waypoints loop;
    protected GameManager gameManager;

    protected override void Start()
    {
        base.Start();
        InvokeRepeating("endOnEnemyLimit", 0f, 0.05f);
    }
    public void SetLoop(Waypoints path)
    {
        loop = path;
    }


    public Waypoints GetLoop() { return loop; }

    public void SetGame(GameManager game)
    {
        gameManager = game;
    }

    protected override IEnumerator SpawnWave()
    {
        if (isWaveGenerating)
        {
            yield break; 
        }
        isWaveGenerating = true;

        int enemyCount = boolAuto ? 1 : myArray[waveIndex].Length;
        for (int i = 0; i < enemyCount; i++)
        {
            
            SpawnEnemy( waveIndex, i, myArray[waveIndex][i]);

            //Debug.LogError("Index d'ennemi invalide: " + myArray[waveIndex][i]);    
            
            yield return new WaitForSeconds(0.5f);
        }

        int AllEnemyKilled = GameObject.FindGameObjectsWithTag("Enemy").Length;

        while(AllEnemyKilled>0){
            yield return null;
            AllEnemyKilled = GameObject.FindGameObjectsWithTag("Enemy").Length;
        }

        yield return new WaitForSeconds(0.5f);

        if (AllEnemyKilled == 0)
        {   
            PlayerData[] allPlayerData = playerManager.GetAllPlayerData();
            Debug.Log("DANS LE COUNT");
            foreach (PlayerData playerData in allPlayerData)
            {
                Debug.Log("DANS LE FOREACH");
                int golds = BonusGold(waveIndex);
                playerManager.SetPlayerAttributeServerRpc(playerData.name, playerData.money + golds);
            }
        }

        waveIndex++;
        yield return new WaitForSeconds(0.5f);
        isWaveGenerating = false;
    }

    protected override void SpawnEnemy(int wave, int numInWave, int enemyType)
    {
        
        GameObject enemy;

        switch (enemyType)
        {
            case 1:
                enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
                break;

            case 2:
                enemy = Instantiate(enemyPrefab2, pos, Quaternion.identity);
                break;

            case 3:
                enemy = Instantiate(enemyPrefab3, pos, Quaternion.identity);
                break;

            default:
                Debug.LogError("Invalid enemy type: " + enemyType);
                return;
        }

        // Use enemyType to identify the correct type of the enemy.

        //GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);
        enemy.name = "Enemy " + wave + " " + numInWave;
        enemy.GetComponent<CircleEnemy>().SetPath(waypoints);
        enemy.GetComponent<CircleEnemy>().SetLoop(loop);
        enemy.GetComponent<NetworkObject>().Spawn();
    }

    protected void endOnEnemyLimit()
    {
        int enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if (enemyCount > 1)
        {
            gameManager.GameOverServerRpc();
        }
    }

    protected int BonusGold(int waveIndex)
    {
        int golds =  10; 
        return golds;
    }

}
        
