using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar
{
    [SerializeField] Slider healthBar;
    private float maxHealth;
    private float currentHealth;
    private Vector3 attachmentLocation = new Vector3(0, 1, 0);

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

    public IEnumerator ShwoAndHide()
    {
        Show();
        yield return new WaitForSeconds(2f);
        Hide();
    }

    private void SetHealthBarToScreen(Vector2 position)
    {
        RectTransform rectTransform = healthBar.GetComponent<RectTransform>();
        rectTransform.anchorMin = position;
        rectTransform.anchorMax = position + new Vector2(healthBar.GetComponent<RectTransform>().sizeDelta.x / Screen.width, healthBar.GetComponent<RectTransform>().sizeDelta.y / Screen.height);
        rectTransform.pivot = position;
    }
}
