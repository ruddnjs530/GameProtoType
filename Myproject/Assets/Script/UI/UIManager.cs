using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro 사용
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UIManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("UIManager");
                    instance = go.AddComponent<UIManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI moneyUI; // 돈 표시 UI
    [SerializeField] private GameObject bossHPBar; // 보스 체력바
    [SerializeField] private GameObject clearPanel; // 클리어 화면
    [SerializeField] private GameObject gameOverPanel; // 게임 오버 화면
    [SerializeField] private Canvas mainCanvas; // 메인 캔버스
    [SerializeField] private GameObject damageTextPrefab; // 데미지 텍스트 프리팹

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            if (mainCanvas != null) DontDestroyOnLoad(mainCanvas);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void UpdateMoneyText(int money)
    {
        if (moneyUI != null)
            moneyUI.text = "$     " + money.ToString();
    }

    public void ShowBossHPBar(bool show)
    {
        if (bossHPBar != null)
            bossHPBar.SetActive(show);
    }
    
    public Slider GetBossHPSlider()
    {
        if (bossHPBar != null)
            return bossHPBar.GetComponent<Slider>();
        return null;
    }

    public void ShowClearPanel()
    {
        if (clearPanel != null)
            clearPanel.SetActive(true);
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void ShowDamageText(Vector3 position, int damage)
    {
        if (damageTextPrefab != null)
        {
            Vector3 randomPos = MakeRandomPosition(position);
            GameObject textInstance = Instantiate(damageTextPrefab, randomPos, Quaternion.identity);
            DamageText damageText = textInstance.GetComponent<DamageText>();
            if (damageText != null)
            {
                damageText.damage = damage;
            }
        }
    }

    private Vector3 MakeRandomPosition(Vector3 position)
    {
        float rand = Random.Range(-0.5f, 0.5f);
        position.x += rand;
        position.y += 4; // 높이 보정
        // 주의: 기존 로직은 GameManager의 y + 4 였음. 여기서는 맞은 위치 + 4로 변경하거나 조정 필요.
        // 기존: textPosition.y = transform.position.y + 4; (GameManager 위치 기준? GameManager는 보통 (0,0,0)에 있나? DontDestroyOnLoad 객체라 위치가 모호함)
        // 보통 데미지 텍스트는 피격 위치 위에 떠야 함. 인자로 받은 position 사용이 맞음.
        position.z += rand;
        return position;
    }
}
