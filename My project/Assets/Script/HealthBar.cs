using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Slider healthBar;
    private float maxHealth;
    private float currentHealth;
    private Vector3 attachmentLocation = new Vector3(0, 1, 0);

    public HealthBar(Slider healthBar, float maxHealth, bool isAttachment)
    {
        this.healthBar = healthBar;
        this.maxHealth = maxHealth;
        this.currentHealth = maxHealth;

        if (isAttachment) SetHealthBarPosition();
        else
        {
            // 따라다니지 않다면 화면에 고정하고 싶은 위치를 받아서 고정하는 함수
        }
    }
    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        healthBar.value = currentHealth / maxHealth;
    }

    private void SetHealthBarPosition()
    {
        this.transform.position = transform.parent.position + attachmentLocation;
    }

    public void Show() // enemy의 경우 보이게 안보이게 하기 위함. 또한 보스의 경우에도 생성될 때만 체력바가 보이게 하기 위함
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
