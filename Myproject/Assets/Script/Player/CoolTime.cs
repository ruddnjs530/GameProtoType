using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoolTime
{
    [SerializeField] private Image coolTimeImage;
    private float maxCoolTime;
    private float currentCoolTime;

    public CoolTime(Image coolTimeImage, float maxCoolTime)
    {
        this.coolTimeImage = coolTimeImage;
        this.maxCoolTime = maxCoolTime;
        this.currentCoolTime = maxCoolTime;
    }

    public IEnumerator ShowCoolTime()
    {
        while (currentCoolTime > 0.0f)
        {
            currentCoolTime -= Time.deltaTime;
            coolTimeImage.fillAmount = currentCoolTime / maxCoolTime;
            yield return new WaitForFixedUpdate();
        }
        currentCoolTime = maxCoolTime;
        coolTimeImage.fillAmount = currentCoolTime / maxCoolTime;
    }
}
