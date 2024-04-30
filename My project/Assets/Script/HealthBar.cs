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
            // ����ٴ��� �ʴٸ� ȭ�鿡 �����ϰ� ���� ��ġ�� �޾Ƽ� �����ϴ� �Լ�
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

    public void Show() // enemy�� ��� ���̰� �Ⱥ��̰� �ϱ� ����. ���� ������ ��쿡�� ������ ���� ü�¹ٰ� ���̰� �ϱ� ����
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
