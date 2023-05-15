using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject enemyPrefab;
    [SerializeField]
    private Waypoints waypoints;

    private float countdown = 2f;

    [SerializeField]
    private float timeBetweenWaves = 2f;

    public static bool boolStart = false;
    public static bool boolAuto = false;
    private Vector3 pos;
    private int waveIndex=0;
    private bool isWaveGenerating = false;
    int[][] myArray = new int[][] {
        new int[] {1,1},
        new int[] {1,1},
    };

    // Start is called before the first frame update
    private void Start()
    {
        pos = waypoints.waypoints[0];
    }

    // Update is called once per frame
    private void Update()
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

    public Waypoints GetPath()
    {
        return waypoints;
    }

    private IEnumerator SpawnWave()
    {
        if (isWaveGenerating)
        {
            yield break; 
        }
        isWaveGenerating = true;

        int enemyCount = boolAuto ? 1 : myArray[waveIndex].Length;
        for (int i = 0; i < enemyCount; i++)
        {
            
            SpawnEnemy(enemyPrefab, waveIndex, i, myArray[waveIndex][i]);

            //Debug.LogError("Index d'ennemi invalide: " + myArray[waveIndex][i]);
                   
            
            yield return new WaitForSeconds(0.5f);
        }
        waveIndex++;
        yield return new WaitForSeconds(0.5f);
        isWaveGenerating = false;
    }

    private void SpawnEnemy(GameObject prefab, int wave, int numInWave, int enemyType)
    {
        // Use enemyType to identify the correct type of the enemy.
        Debug.Log("VOICI LE POS AVANT   /////////"+ waypoints.waypoints[0]);
        GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);
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
        
