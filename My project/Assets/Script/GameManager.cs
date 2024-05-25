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

    public int money = 100;

    public bool isEnemyWave = false; // 포탈을 활성화 시켰을 때를 위한 웨이브 변수

    public bool isPlayerAlive = true;

    public bool isGameClear = false;

    public int bulletDamage = 10;

    [SerializeField] private TextMeshProUGUI moneyUI;

    [SerializeField] private ParticleSystem hitParticle;
    [SerializeField] private GameObject damageTextPrefab;

    private PlayerStatus playerStatus;
    public List<GameObject> turrets = new List<GameObject>();
    public List<GameObject> drones = new List<GameObject>();

    [SerializeField] private GameObject boss;

    [SerializeField] private Canvas canvas;

    private float time = 0f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            if (canvas != null) DontDestroyOnLoad(canvas);
        }
        else
            Destroy(this.gameObject);
    }

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
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
        if (boss != null)
        {
            GameObject currentBoss = Instantiate(boss, Vector3.zero, Quaternion.identity);
            BossEnemy bossComponent = currentBoss.GetComponent<BossEnemy>();
            if (bossComponent != null)
            {
                Slider bossHealthBar = canvas.GetComponentInChildren<Slider>();
                if (bossHealthBar != null)
                {
                    bossComponent.SetHealthBar(bossHealthBar);
                }
            }
        }
    }

    public void SavePlayerStatus(Player player, Inventory inventory, List<GameObject> turrets, List<GameObject> drones)
    {
        List<InventoryItem> inventoryItems = inventory.GetItems();
        playerStatus = new PlayerStatus(player.GetHP(), player.GetMaxHP(), money, inventoryItems, turrets, drones);
    }

    public void LoadPlayerStatus(Player player, Inventory inventory, List<GameObject> turrets, List<GameObject> drones)
    {
        if (playerStatus != null)
        {
            player.SetHP(playerStatus.currentHP);
            player.SetMaxHP(playerStatus.maxHP);
            money = playerStatus.money;
            inventory.SetItems(playerStatus.inventoryItems);
            turrets = playerStatus.turrets;
            drones = playerStatus.drones;

            foreach (var turret in playerStatus.turrets)
            {
                Instantiate(turret.gameObject, turret.gameObject.transform.position, turret.gameObject.transform.rotation);
            }

            foreach (var drone in playerStatus.drones)
            {
                Instantiate(drone.gameObject, drone.gameObject.transform.position, drone.gameObject.transform.rotation);
            }
        }
    }

    public void AddTurret(GameObject turret)
    {
        turrets.Add(turret);
    }

    public List<GameObject> GetTurrets()
    {
        return turrets;
    }

    public void AddDrone(GameObject drone)
    {
        drones.Add(drone);
    }

    public List<GameObject> GetDrones()
    {
        return drones;
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
