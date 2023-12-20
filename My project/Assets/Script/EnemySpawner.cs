using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] Enemy[] enemys; // ���� ��Ƶΰ� �������� �����ϱ� ���� ����Ʈ

    [SerializeField] Transform[] enemySpawnPoints;
    // ���� ������ ��ġ (��ġ�� ������ �ΰ� �ش� ��ġ�� �ݰ濡�� �������� ����)

    //[SerializeField] Transform[] enemyWaveSpawnPoints;
    // �� ���̺� Ȱ��ȭ �� ������ ������ ��ġ

    //public bool isEnemyWave = false; // ��Ż�� Ȱ��ȭ ������ ���� ���� ���̺� ����

    int enemyCount = 15; // �� ���� ��, ���̺갡 Ȱ��ȭ �Ǹ� *4 ���� �� ����

    // Start is called before the first frame update
    void Start()
    {
        SpawnEnemy(enemySpawnPoints);
    }

    // Update is called once per frame
    void Update()
    {
        //if (isEnemyWave) // �� ���̺갡 Ȱ��ȭ �Ǹ� ��� �����Ǵ� ������ ���� ���� ������, �ٸ� ��ġ���� ���� ( ��Ż �ֺ����� ���� )
        //{
        //    enemyCount *= 4;
        //    SpawnEnemy(enemyWaveSpawnPoints);
        //    isEnemyWave = false;
        //}
    }

    void SpawnEnemy(Transform[] spawnPoints)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)]; // ���� ��ġ�� ����

            Vector3 randomSpawnPoint = randomPoint.position + Random.insideUnitSphere * 4f; // ������ ��ġ�� �߽����� �ݰ� 2�� ���� �� �ȿ��� ����

            randomSpawnPoint.y = randomPoint.position.y; // y ��ǥ�� ����

            Instantiate(enemys[Random.Range(0, enemys.Length)], randomSpawnPoint, randomPoint.rotation); // ������ enemy ����
        }
        return;
    }
}
