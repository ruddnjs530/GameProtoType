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

    [SerializeField] private float gameOverDelay = 3.0f; // 사망 애니메이션을 보여 준 뒤 게임오버 씬으로 넘어가는 시간
    [SerializeField] private float clearDelay = 3.0f; // 보스 사망 연출을 보여 준 뒤 클리어 씬으로 넘어가는 시간

    // 게임오버·클리어 씬 전환을 한 번만 요청하기 위한 가드. 사망 이벤트가 여러 번 와도 씬은 한 번만 로드한다.
    private bool isRunEnding = false;

    // 한 판(런) 동안 씬을 넘어 유지되는 오브젝트 목록.
    // 재시작할 때 이 목록만 정리하면 되도록 DDOL 지정을 이곳 한 곳에서 관리한다.
    private static readonly List<GameObject> runObjects = new List<GameObject>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            KeepForRun(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (instance != this) // 인스턴스 중복 확인 로직 강화
            Destroy(this.gameObject);
    }

    private void OnDestroy()
    {
        if (instance != this) return;

        // 재시작 후 새 GameManager가 생기므로, 파괴된 인스턴스가 정적 이벤트를 계속 받지 않게 해제한다.
        SceneManager.sceneLoaded -= OnSceneLoaded;
        BossEnemy.OnBossDeath -= BossDeath;
        Player.OnPlayerDeath -= PlayerDeath;
    }

    /// <summary>
    /// 씬이 바뀌어도 이번 판이 끝날 때까지 유지할 오브젝트를 등록한다.
    /// DDOL은 루트 오브젝트에만 적용되므로 루트를 등록한다.
    /// </summary>
    public static void KeepForRun(GameObject go)
    {
        GameObject root = go.transform.root.gameObject;
        if (runObjects.Contains(root)) return;

        DontDestroyOnLoad(root);
        runObjects.Add(root);
    }

    /// <summary>
    /// 이번 판에서 유지하던 오브젝트를 모두 파괴한다. 게임오버·클리어 씬은 HUD·플레이어 없이 독립적으로 동작하고,
    /// 타이틀에서 다시 시작하면 SampleScene의 오브젝트가 새로 만들어진다.
    /// </summary>
    public static void EndRun()
    {
        foreach (GameObject go in runObjects)
        {
            if (go != null) Destroy(go);
        }
        runObjects.Clear();

        // 메뉴(timeScale 0)나 인벤토리를 연 채로 끝났을 수 있으므로 전역 상태를 기본값으로 되돌린다.
        Time.timeScale = 1f;
        Inventory.inventoryActivated = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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
            KeepForRun(inventory.gameObject);
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
        EndRunAfterDelay("ClearScene", clearDelay);
    }

    private void PlayerDeath()
    {
        isPlayerAlive = false;
        EndRunAfterDelay("GameOverScene", gameOverDelay);
    }

    // 씬 전환 경로를 이곳 하나로 모은다. 먼저 들어온 결과(사망/클리어)만 처리한다.
    private void EndRunAfterDelay(string sceneName, float delay)
    {
        if (isRunEnding) return;
        isRunEnding = true;
        StartCoroutine(EndRunRoutine(sceneName, delay));
    }

    private IEnumerator EndRunRoutine(string sceneName, float delay)
    {
        // 메뉴가 열려 timeScale이 0이어도 진행되도록 실시간으로 기다린다.
        yield return new WaitForSecondsRealtime(delay);

        // 파괴는 프레임 끝에 일어나므로 같은 프레임에 씬 로드를 요청해도 안전하다.
        EndRun();
        SceneManager.LoadScene(sceneName);
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
