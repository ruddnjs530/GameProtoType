using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] Enemy[] enemys; // ���� ��Ƶΰ� �������� �����ϱ� ���� ����Ʈ

    [SerializeField] Transform[] enemySpawnPoints;
    // ���� ������ ��ġ (��ġ�� ������ �ΰ� �ش� ��ġ�� �ݰ濡�� �������� ����)

    [SerializeField] Transform[] enemyWaveSpawnPoints;
    // �� ���̺� Ȱ��ȭ �� ������ ������ ��ġ

    int enemyCount = 100; // �� ���� ��, ���̺갡 Ȱ��ȭ �Ǹ� *4 ���� �� ����

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
        if (GameManager.Instance.isEnemyWave) // �� ���̺갡 Ȱ��ȭ �Ǹ� ��� �����Ǵ� ������ ���� ���� ������, �ٸ� ��ġ���� ���� ( ��Ż �ֺ����� ���� )
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
            Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)]; // ���� ��ġ�� ����

            Vector3 randomSpawnPoint = randomPoint.position + Random.insideUnitSphere * radius; // ������ ��ġ�� �߽����� �ݰ� 2�� ���� �� �ȿ��� ����

            randomSpawnPoint.y = randomPoint.position.y; // y ��ǥ�� ����

            Instantiate(enemys[Random.Range(0, enemys.Length)], randomSpawnPoint, randomPoint.rotation); // ������ enemy ����
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
