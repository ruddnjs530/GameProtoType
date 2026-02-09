using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnManager : MonoBehaviour
{
    private static SpawnManager instance;
    public static SpawnManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SpawnManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("SpawnManager");
                    instance = go.AddComponent<SpawnManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    [Header("Drone Settings")]
    [SerializeField] private GameObject dronePrefab; // 드론 프리팹
    public List<GameObject> drones = new List<GameObject>();
    private int droneCount = 0;

    [Header("Boss Settings")]
    [SerializeField] private GameObject bossPrefab; // 보스 프리팹
    
    // EnemySpawner에서 사용하는 변수들도 여기서 관리할 수 있으나, 일단 GameManager에 있던 것만 이동.

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void InitializeBoss()
    {
        // UIManager에서 Boss HP Bar 활성화
        UIManager.Instance.ShowBossHPBar(true);

        if (bossPrefab != null)
        {
            Vector3 pos = new Vector3(0, 0, -40); // 보스 스폰 위치
            GameObject currentBoss = Instantiate(bossPrefab, pos, bossPrefab.transform.rotation);
            BossEnemy bossComponent = currentBoss.GetComponent<BossEnemy>();
            if (bossComponent != null)
            {
                // UIManager를 통해 HP Bar 슬라이더 가져오기
                bossComponent.SetHealthBar(UIManager.Instance.GetBossHPSlider());
            }
        }
    }

    public void AddDrone()
    {
        droneCount += 1;
    }

    public int GetDroneCount() { return droneCount; }

    public void SetDrone(int _droneCount, Player player) 
    { 
        droneCount = _droneCount; 
        for (int i = 0; i < _droneCount; i++)
        {
            if (dronePrefab != null)
                Instantiate(dronePrefab, player.transform.position, player.transform.rotation);
        }
    }
}
