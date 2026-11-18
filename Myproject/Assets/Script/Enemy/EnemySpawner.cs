using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] Enemy[] enemys;

    [SerializeField] Transform[] enemySpawnPoints;

    [SerializeField] Transform[] enemyWaveSpawnPoints;

    int enemyCount = 20; 

    [SerializeField] Transform portal;
    [SerializeField] BossEnemy boss;

    int enemyIdCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
        SpawnEnemy(enemySpawnPoints);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.isEnemyWave)
        {
            Debug.Log("is enemy wave");
            //enemyCount *= 2;
            // SpawnEnemy(enemyWaveSpawnPoints, 50f);
            GameManager.Instance.isEnemyWave = false;

            SpawnBoss();
        }
    }

    void SpawnEnemy(Transform[] spawnPoints, float radius = 30f)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            Vector3 randomSpawnPoint = randomPoint.position + Random.insideUnitSphere * radius;

            randomSpawnPoint.y = randomPoint.position.y;

            Enemy newEnemy = Instantiate(enemys[Random.Range(0, enemys.Length)], randomSpawnPoint, randomPoint.rotation);
            newEnemy.EnemyID = enemyIdCounter++;
        }
        return;
    }

    void SpawnBoss()
    {
        Vector3 bossSpawnPoints = portal.position;
        bossSpawnPoints.y += 5;
        Instantiate(boss, bossSpawnPoints, portal.rotation);
    }
}