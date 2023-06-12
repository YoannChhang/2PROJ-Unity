using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField]
    protected GameObject enemyPrefab;
    [SerializeField]
    protected GameObject enemyPrefab2;
    [SerializeField]
    protected GameObject enemyPrefab3;

    protected Waypoints waypoints;

    protected float countdown = 5f;
    
    protected PlayerManager playerManager;

    [SerializeField]
    protected float timeBetweenWaves = 5f;

    public static bool boolStart = false;
    public static bool boolAuto = false;
    protected Vector3 pos;
    public int waveIndex=0;
    protected bool isWaveGenerating = false;
    protected int[][] myArray;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
        pos = waypoints.waypoints[0];
    }

    // Update is called once per frame
     protected void Update()
    {
        // if(countdown <=0f && NetworkManager.Singleton.IsServer && waveIndex < myArray.Length)
        // {
        //     StartCoroutine(SpawnWave());
        //     countdown = timeBetweenWaves;
        // }
        // //NoEnemiesLeft();
        // countdown -= Time.deltaTime;
        if (countdown <= 0f && waveIndex < myArray.Length)
        {
            if (boolStart)
            {
                StartCoroutine(SpawnWave());
                countdown = timeBetweenWaves;
                boolStart = false;
            }
            else if (boolAuto)
            {
                StartCoroutine(SpawnWave());
                countdown = timeBetweenWaves;
            }
        }
        countdown -= Time.deltaTime;
    }

    public void SetPath(Waypoints path)
    {
        waypoints = path;
    }

    public Waypoints GetPath() { return waypoints; }

    public void SetWaves(int[][] waves)
    {
        myArray = waves;
    }

    public int[][] GetWaves() { return myArray; }

    protected virtual IEnumerator SpawnWave()
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

    protected virtual void SpawnEnemy(int wave, int numInWave, int enemyType)
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
        enemy.GetComponent<Enemy>().SetPath(waypoints);
        enemy.GetComponent<NetworkObject>().Spawn();
    }
        

    protected int BonusGold(int waveIndex)
    {
        int golds =  10; 
        return golds;
    }

}
        
