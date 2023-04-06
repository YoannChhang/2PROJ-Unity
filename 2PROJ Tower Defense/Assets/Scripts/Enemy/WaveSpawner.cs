using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject enemyPrefab;
    [SerializeField]
    private Waypoints waypoints;

    private float countdown = 2f;

    [SerializeField]
    private float timeBetweenWaves = 5f;

    private int waveIndex=0;
    int[][] myArray = new int[][] {
        new int[] {1},
        new int[] {1},
    };


    
    // Start is called before the first frame update
    private void Start()
    {
        enemyPrefab.GetComponent<Enemy>().path = waypoints;
    }

    // Update is called once per frame
    private void Update()
    {
        if(countdown <=0f)
        {
            StartCoroutine(SpawnWave());
            countdown = timeBetweenWaves;
        }

        countdown -= Time.deltaTime;
    }

    private IEnumerator SpawnWave()
    {
        Vector3 pos = waypoints.waypoints[0];
        for (int i=0;i<myArray[waveIndex].Length;i++)
        {
            switch(myArray[waveIndex][i])
            {
                case 1:
                    Instantiate(enemyPrefab, pos, Quaternion.identity);
                    break;
                default:
                    Debug.LogError("Index d'ennemi invalide: " + myArray[waveIndex][i]);
                    break;
            }
            yield return new WaitForSeconds(0.5f);
        }
        waveIndex++;
        yield return new WaitForSeconds(0.5f);
        
    }
        
}
        
