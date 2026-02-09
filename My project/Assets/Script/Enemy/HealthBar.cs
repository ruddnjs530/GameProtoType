using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar
{
    [SerializeField] Slider healthBar;
    private float maxHealth;
    private float currentHealth;
    private Vector3 attachmentLocation = new Vector3(0, 1, 0); // 체력바 위치 보정값

    // 생성자: 체력바 초기화 및 위치 설정
    public HealthBar(Slider healthBar, float maxHealth, bool isAttachment, Vector2 position = default)
    {
        this.healthBar = healthBar;
        this.maxHealth = maxHealth;
        this.currentHealth = maxHealth;

        if (isAttachment) SetHealthBarOnObject();
        else
        {
            SetHealthBarToScreen(position);
        }
    }

    // 체력 설정 및 UI 업데이트
    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        healthBar.value = currentHealth / maxHealth;
    }

    private void SetHealthBarOnObject()
    {
        healthBar.transform.position = healthBar.transform.parent.position + attachmentLocation;
    }

    public void Show()
    {
        healthBar.gameObject.SetActive(true);
    }

    public void Hide()
    {
        healthBar.gameObject.SetActive(false);
    }

    // 일정 시간동안 체력바 표시 후 숨기기
    public IEnumerator ShowAndHide() // 오타 수정: ShwoAndHide -> ShowAndHide
    {
        Show();
        yield return new WaitForSeconds(2f);
        Hide();
    }

    // 화면 좌표에 체력바 설정
    private void SetHealthBarToScreen(Vector2 position)
    {
        RectTransform rectTransform = healthBar.GetComponent<RectTransform>();
        rectTransform.anchorMin = position;
        // 화면 비율에 맞춰 앵커 설정
        rectTransform.anchorMax = position + new Vector2(healthBar.GetComponent<RectTransform>().sizeDelta.x / Screen.width, healthBar.GetComponent<RectTransform>().sizeDelta.y / Screen.height);
        rectTransform.pivot = position;
    }
}
