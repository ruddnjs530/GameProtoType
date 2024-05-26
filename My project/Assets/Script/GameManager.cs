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

    public bool isEnemyWave = false; 

    public bool isPlayerAlive = true;

    public bool isGameClear = false;

    public int bulletDamage = 10;

    [SerializeField] private TextMeshProUGUI moneyUI;

    [SerializeField] private ParticleSystem hitParticle;
    [SerializeField] private GameObject damageTextPrefab;

    public PlayerStatus playerStatus;
    public List<GameObject> turrets = new List<GameObject>();
    public List<GameObject> drones = new List<GameObject>();

    [SerializeField] private GameObject boss;

    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject bossHPBar;

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
                Debug.LogError("GameManager instance is null");
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
        bossHPBar.SetActive(true);
        if (boss != null)
        {
            Vector3 pos = new Vector3(0, 0, 0);
            GameObject currentBoss = Instantiate(boss, pos, boss.transform.rotation);
            Debug.Log(currentBoss);
            BossEnemy bossComponent = currentBoss.GetComponent<BossEnemy>();
            if (bossComponent != null)
            {
                bossComponent.SetHealthBar(bossHPBar.GetComponent<Slider>());
            }
        }
    }

    public void SavePlayerStatus(Player player, Inventory inventory, List<GameObject> turrets, List<GameObject> drones)
    {
        List<InventoryItem> inventoryItems = inventory.GetItems();
        playerStatus = new PlayerStatus(player.GetHP(), player.GetMaxHP(), money, inventoryItems, turrets, drones);
        Debug.Log("Player status saved: HP " + player.GetHP() + ", Money " + money);
    }

    public void LoadPlayerStatus(Player player, Inventory inventory, PlayerStatus playerStatus)
    {
        if (playerStatus != null)
        {
            player.SetHP(playerStatus.currentHP);
            player.SetMaxHP(playerStatus.maxHP);
            money = playerStatus.money;
            inventory.SetItems(playerStatus.inventoryItems);

            foreach (var turret in turrets)
            {
                Destroy(turret);
            }
            turrets.Clear();

            foreach (var drone in drones)
            {
                Destroy(drone.gameObject);
            }
            drones.Clear();

            foreach (var drone in playerStatus.drones)
            {
                if (drone != null)
                {
                    Debug.Log("Instantiating drone: " + drone.name);
                    var newDrone = Instantiate(drone, drone.transform.position, drone.transform.rotation);
                    drones.Add(newDrone);
                }
                else
                {
                    Debug.LogError("Drone is null in playerState");
                }
            }

            foreach (var turret in playerStatus.turrets)
            {
                if (turret != null)
                {
                    Debug.Log("Instantiating turret: " + turret.name);
                    var newTurret = Instantiate(turret, turret.transform.position, turret.transform.rotation);
                    turrets.Add(newTurret);
                }
                else
                {
                    Debug.LogError("Turret is null in playerState");
                }
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
