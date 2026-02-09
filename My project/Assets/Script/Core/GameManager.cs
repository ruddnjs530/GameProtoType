using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    [Header("Game Settings")]
    public bool canPlayerMove = true; // 플레이어 이동 가능 여부
    public bool isOpenInventory = false; // 인벤토리 열림 상태
    public bool isMenuOpen = false; // 메뉴 열림 상태
    public bool isEnemyWave = false; // 적 웨이브 진행 중 여부
    public bool isPlayerAlive = true; // 플레이어 생존 여부
    public bool isGameClear = false; // 게임 클리어 여부

    [Header("Player Stats")]
    public int money = 0; // 보유 자금
    public int bulletDamage = 10; // 총알 데미지

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI moneyUI; // 돈 표시 UI
    [SerializeField] private Canvas canvas; // 메인 캔버스
    [SerializeField] private GameObject clear; // 클리어 화면
    [SerializeField] private GameObject gameOver; // 게임 오버 화면

    [Header("Effects")]
    [SerializeField] private ParticleSystem hitParticle; // 피격 효과
    [SerializeField] private GameObject damageTextPrefab; // 데미지 텍스트 프리팹

    [Header("Boss & Enemies")]
    public PlayerStatus playerStatus;
    public List<GameObject> drones = new List<GameObject>();
    [SerializeField] GameObject dronePrefab; // 드론 프리팹
    private int droneCount = 0;

    [SerializeField] private GameObject boss; // 보스 프리팹
    [SerializeField] private GameObject bossHPBar; // 보스 체력바

    private float time = 0f;
    [SerializeField] private float gameOverDelay = 6.0f; // 게임 오버 후 씬 전환 지연 시간

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            if (canvas != null) DontDestroyOnLoad(canvas);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
            Destroy(this.gameObject);
    }

    public static GameManager Instance
    {
        get
        {
            return instance;
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Inventory inventory = FindObjectOfType<Inventory>();
        if (inventory != null)
        {
            DontDestroyOnLoad(inventory.gameObject);
        }
    }

    private void OnEnable()
    {
        Bullet.OnBulletHit += HandleBulletHit;
    }

    private void OnDisable()
    {
        Bullet.OnBulletHit -= HandleBulletHit;
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;;
        moneyUI.text = "$     " + money.ToString();

        BossEnemy.OnBossDeath += BossDeath;
        Player.OnPlayerDeath += PlayerDeath;
    }

    // Update is called once per frame
    void Update()
    {
        if (isOpenInventory || isMenuOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            canPlayerMove = false;
        }   
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            canPlayerMove = true;
        }

        if (!isPlayerAlive)
        {
            time += Time.deltaTime;
            if (time >= gameOverDelay) // 매직 넘버(6.0f) 제거
                SceneManager.LoadScene("GameOverScene");
        }    
    }

    public void InitializeBoss()
    {
        bossHPBar.SetActive(true);
        if (boss != null)
        {
            Vector3 pos = new Vector3(0, 0, -40);
            GameObject currentBoss = Instantiate(boss, pos, boss.transform.rotation);
            BossEnemy bossComponent = currentBoss.GetComponent<BossEnemy>();
            if (bossComponent != null)
            {
                bossComponent.SetHealthBar(bossHPBar.GetComponent<Slider>());
            }
        }
    }

    public void SavePlayerStatus(Player player, Inventory inventory, List<GameObject> drones)
    {
        List<InventoryItem> inventoryItems = inventory.GetItems();
        playerStatus = new PlayerStatus(player.GetHP(), player.GetMaxHP(), money, inventoryItems);
        //foreach (var drone in drones)
        //{
        //    DontDestroyOnLoad(drone);
        //}
    }

    public void LoadPlayerStatus(Player player, Inventory inventory, PlayerStatus playerStatus)
    {
        if (playerStatus != null)
        {
            player.SetHP(playerStatus.currentHP);
            player.SetMaxHP(playerStatus.maxHP);
            money = playerStatus.money;
            inventory.SetItems(playerStatus.inventoryItems);
        }
    }

    public void AddDrone() // itemBox.cs 에 있음. (GameObject drone)
    {
        // drones.Add(drone);
        droneCount += 1;
    }

    public int GetDroneCount() { return droneCount; }
    public void SetDrone(int _droneCount, Player player) 
    { 
        droneCount = _droneCount; 
        for (int i = 0; i < _droneCount; i++)
        {
            Instantiate(dronePrefab, player.transform.position, player.transform.rotation); // dronePrefabs -> dronePrefab 변경 적용
        }
    }

    private void HandleBulletHit(Vector3 hitPosition)
    {
        if (hitParticle != null)
        {
            ParticleSystem hitParticleInstance = Instantiate(hitParticle, hitPosition, Quaternion.identity);
            ParticleSystem particleSystem = hitParticleInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                particleSystem.Play();
                Destroy(hitParticleInstance, particleSystem.main.duration);
            }
        }

        if (damageTextPrefab != null)
        {
            GameObject textInstance = Instantiate(damageTextPrefab, MakeRandomPosition(hitPosition), Quaternion.identity);
            DamageText damageText = textInstance.GetComponent<DamageText>();
            if (damageText != null)
            {
                damageText.damage = bulletDamage;
            }
        }
    }

    private Vector3 MakeRandomPosition(Vector3 textPosition)
    {
        float rand = Random.Range(-0.5f, 0.5f);
        textPosition.x += rand;
        textPosition.y = transform.position.y + 4;
        textPosition.z += rand;
        return textPosition;
    }

    private void BossDeath()
    {
        SceneManager.LoadScene("ClearScene");
        clear.SetActive(true);
    }

    private void PlayerDeath()
    {
        SceneManager.LoadScene("GameOverScene");
        gameOver.SetActive(true);
    }

    public void IncreaseMoney(int price)
    {
        money += price;
        moneyUI.text = "$     " + money.ToString();
    }

    public void DecreaseMoney(int price)
    {
        money -= price;
        moneyUI.text = "$     " + money.ToString();
    }

    public void UpdateMoney()
    {
        moneyUI.text = "$     " + money.ToString();
    }  
}
