using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    public bool canPlayerMove = true;
    public bool isOpenInventory = false;

    public bool isMenuOpen = false;

    public int money = 0;

    public bool isEnemyWave = false; 

    public bool isPlayerAlive = true;

    public bool isGameClear = false;

    public int bulletDamage = 10;

    [SerializeField] private TextMeshProUGUI moneyUI;

    [SerializeField] private ParticleSystem hitParticle;
    [SerializeField] private GameObject damageTextPrefab;

    public PlayerStatus playerStatus;
    public List<GameObject> drones = new List<GameObject>();
    [SerializeField] GameObject dronePrefabs;
    private int droneCount = 0;

    [SerializeField] private GameObject boss;

    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject bossHPBar;

    [SerializeField] private GameObject clear;
    [SerializeField] private GameObject gameOver;

    private float time = 0f;

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
            if (time >= 6.0f)
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
            Instantiate(dronePrefabs, player.transform.position, player.transform.rotation);
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
