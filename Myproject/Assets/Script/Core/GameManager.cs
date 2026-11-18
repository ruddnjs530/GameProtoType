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

    [Header("Effects")]
    [SerializeField] private ParticleSystem hitParticle; // 피격 효과

    public PlayerStatus playerStatus;

    private float time = 0f;
    [SerializeField] private float gameOverDelay = 6.0f; // 게임 오버 후 씬 전환 지연 시간

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (instance != this) // 인스턴스 중복 확인 로직 강화
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
        UIManager.Instance.UpdateMoneyText(money);

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

    public void SavePlayerStatus(Player player, Inventory inventory)
    {
        List<InventoryItem> inventoryItems = inventory.GetItems();
        playerStatus = new PlayerStatus(player.GetHP(), player.GetMaxHP(), money, inventoryItems);
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

        UIManager.Instance.ShowDamageText(hitPosition, bulletDamage);
    }

    // MakeRandomPosition 함수는 UIManager로 이동됨 -> 제거
    // private Vector3 MakeRandomPosition(Vector3 textPosition) ...

    private void BossDeath()
    {
        SceneManager.LoadScene("ClearScene");
        UIManager.Instance.ShowClearPanel();
    }

    private void PlayerDeath()
    {
        SceneManager.LoadScene("GameOverScene");
        UIManager.Instance.ShowGameOverPanel();
    }

    public void IncreaseMoney(int price)
    {
        money += price;
        UIManager.Instance.UpdateMoneyText(money);
    }

    public void DecreaseMoney(int price)
    {
        money -= price;
        UIManager.Instance.UpdateMoneyText(money);
    }

    public void UpdateMoney()
    {
        UIManager.Instance.UpdateMoneyText(money);
    }  
}
