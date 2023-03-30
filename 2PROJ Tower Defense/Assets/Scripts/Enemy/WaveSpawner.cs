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
        waveIndex++;

        for (int i =0; i< waveIndex;i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(0.5f);

        }

    }
    private void SpawnEnemy()
    {

        Vector3 pos = waypoints.waypoints[0];
        GameObject newEnemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
    }

}
