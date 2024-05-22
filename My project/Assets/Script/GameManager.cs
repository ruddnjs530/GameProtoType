using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    public bool canPlayerMove = true;
    public bool isOpenInventory = false;

    public bool isMenuOpen = false;

    public int money = 0;

    public bool isEnemyWave = false; // 포탈을 활성화 시켰을 때를 위한 웨이브 변수

    public bool isPlayerAlive = true;

    public bool isGameClear = false;

    public int bulletDamage = 10;

    [SerializeField] TextMeshProUGUI moneyUI;

    [SerializeField] ParticleSystem hitParticle;
    [SerializeField] GameObject damageTextPrefab;

    float time = 0f;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
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

    Vector3 MakeRandomPosition(Vector3 textPosition)
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
