using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] Enemy[] enemys; // 적을 담아두고 랜덤으로 생성하기 위한 리스트

    [SerializeField] Transform[] enemySpawnPoints;
    // 적을 생성할 위치 (위치를 저장해 두고 해당 위치의 반경에서 랜덤으로 생성)

    [SerializeField] Transform[] enemyWaveSpawnPoints;
    // 적 웨이브 활성화 시 적들이 생성될 위치

    int enemyCount = 100; // 적 생성 수, 웨이브가 활성화 되면 *4 정도 할 예정

    [SerializeField] Transform potal;
    [SerializeField] BossEnemy boss;

    // Start is called before the first frame update
    void Start()
    {
        SpawnEnemy(enemySpawnPoints);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.isEnemyWave) // 적 웨이브가 활성화 되면 평소 생성되는 적보다 많은 수가 생성됨, 다른 위치에서 생성 ( 포탈 주변에서 생성 )
        {
            Debug.Log("is enemy wave");
            //enemyCount *= 2;
           // SpawnEnemy(enemyWaveSpawnPoints, 50f);
            GameManager.Instance.isEnemyWave = false;

            SpawnBoss();
        }
    }

    void SpawnEnemy(Transform[] spawnPoints, float radius = 40f)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)]; // 랜덤 위치를 지정

            Vector3 randomSpawnPoint = randomPoint.position + Random.insideUnitSphere * radius; // 지정된 위치를 중심으로 반경 2를 갖는 구 안에서 생성

            randomSpawnPoint.y = randomPoint.position.y; // y 좌표는 고정

            Instantiate(enemys[Random.Range(0, enemys.Length)], randomSpawnPoint, randomPoint.rotation); // 랜덤한 enemy 생성
        }
        return;
    }

    void SpawnBoss()
    {
        Vector3 bossSpawnPoints = potal.position;
        bossSpawnPoints.y += 5;
        Instantiate(boss, bossSpawnPoints, potal.rotation);
    }
}
