using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject enemyPrefab;
    [SerializeField]
    private GameObject enemyPrefab2;
    [SerializeField]
    private Waypoints waypoints;

    private float countdown = 5f;

    [SerializeField]
    private float timeBetweenWaves = 5f;

    private Vector3 pos;
    public int waveIndex=0;
    int[][] myArray = new int[][] {
        new int[] {1,2},
        new int[] {1,2},
    };

    // Start is called before the first frame update
    private void Start()
    {
        pos = waypoints.waypoints[0];
    }

    // Update is called once per frame
    private void Update()
    {
        if(countdown <=0f && NetworkManager.Singleton.IsServer && waveIndex < myArray.Length)
        {
            StartCoroutine(SpawnWave());
            countdown = timeBetweenWaves;
        }
        //NoEnemiesLeft();
        countdown -= Time.deltaTime;
    }

    public void SetPath(Waypoints path)
    {
        waypoints = path;
    }

    public Waypoints GetPath()
    {
        return waypoints;
    }

    private IEnumerator SpawnWave()
    {
        for (int i=0;i<myArray[waveIndex].Length;i++)
        {
            
            SpawnEnemy(waveIndex, i, myArray[waveIndex][i]);

            //Debug.LogError("Index d'ennemi invalide: " + myArray[waveIndex][i]);
                   
            
            yield return new WaitForSeconds(0.5f);
        }
        waveIndex++;
        yield return new WaitForSeconds(0.5f);
        
    }

    private void SpawnEnemy(int wave, int numInWave, int enemyType)
    {
        
        GameObject enemy;
        if (enemyType == 1)
        {
            enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
        }
        else if (enemyType == 2)
        {
            enemy = Instantiate(enemyPrefab2, pos, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Invalid enemy type: " + enemyType);
            return;
        }
        // Use enemyType to identify the correct type of the enemy.

        //GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);
        enemy.name = "Enemy " + wave + " " + numInWave;
        enemy.GetComponent<Enemy>().SetPath(waypoints);
        enemy.GetComponent<NetworkObject>().Spawn();
    }
        
    // private void NoEnemiesLeft()
    // {
    //     int enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
    //     if(enemyCount == 0)
    //     {
    //         Debug.Log("Plus aucun ennemis sur la map");
    //     }
    // }

}
        
