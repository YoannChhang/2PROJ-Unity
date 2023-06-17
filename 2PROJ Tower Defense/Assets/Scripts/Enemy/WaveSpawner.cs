using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class WaveSpawner : MonoBehaviour
{

    [SerializeField] protected List<GameObject> enemyList;

    protected Waypoints waypoints;

    protected float countdown = 5f;
    
    protected PlayerManager playerManager;

    [SerializeField]
    protected float timeBetweenWaves = 5f;

    public static bool boolStart = false;
    public static bool boolAuto = false;
    public static bool boolEndless = false;
    public static int waveIndex=0;
    protected Vector3 pos;
    protected bool isWaveGenerating = false;

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
        //if (countdown <= 0f && waveIndex < myArray.Length)

        if (countdown <= 0f)
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

    public void SetPath(Waypoints path) {  waypoints = path; }

    public Waypoints GetPath() { return waypoints; }

    protected IEnumerator SpawnWave()
    {
        if (isWaveGenerating)
        {
            yield break; 
        }
        isWaveGenerating = true;

        //int enemyCount = boolAuto ? 1 : myArray[waveIndex].Length;

        int enemyCount = 5 * (waveIndex + 1);
        for (int i = 0; i < enemyCount; i++)
        {
            
            // Random number between 1 and wave number for enemy type.
            int enemyType = UnityEngine.Random.Range(0, waveIndex);
            enemyType = enemyType % enemyList.Count;
            SpawnEnemy(waveIndex, i, enemyType);

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

        Quaternion rotation = Quaternion.Euler(-45f, 0f, 0f);

        enemy = Instantiate(enemyList[enemyType], pos, rotation);
        
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
        
