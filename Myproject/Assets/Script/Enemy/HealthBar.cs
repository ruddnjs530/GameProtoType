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

    // 최대 체력이 바뀌면(체력 버프, 로드) 비율 계산의 기준도 함께 바꾼다.
    // 바꾸지 않으면 최대 체력이 100을 넘었을 때 HP바가 가득 찬 채 줄지 않다가 100 아래에서야 줄어든다.
    public void SetMaxHealth(float max)
    {
        maxHealth = Mathf.Max(1f, max);
        SetHealth(currentHealth);
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

    // 화면 비율 좌표(position)에 체력바를 배치한다. 크기는 RectTransform의 sizeDelta(캔버스 단위)를 그대로 쓴다.
    // 예전에는 앵커만 바꾸고 기존 anchoredPosition을 남겨 보스 HP바(y=386)가 화면 위로 밀려났고,
    // 앵커 폭을 Screen.width로 계산해 해상도에 따라 크기와 위치가 달라졌다.
    private void SetHealthBarToScreen(Vector2 position)
    {
        RectTransform rectTransform = healthBar.GetComponent<RectTransform>();
        rectTransform.anchorMin = position;
        rectTransform.anchorMax = position;
        rectTransform.pivot = position;
        rectTransform.anchoredPosition = Vector2.zero;
    }
}
