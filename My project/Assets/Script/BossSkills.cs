using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BossSkills
{
    string skillName;
    float coolTime;
    public float lastUsedTime;

    public event Action<BossSkills> OnCooldownFinished;

    public string GetSkillName() { return skillName; }

    public BossSkills(string skillName, float coolTime)
    {
        this.skillName = skillName;
        this.coolTime = coolTime;
        this.lastUsedTime = -coolTime;
    }

    public bool isReady()
    {
        if ((Time.time - lastUsedTime) >= coolTime)
        {
            CooldownFinished();
            return true;
        }
        return false;
    }

    public void setUseSkillTime()
    {
        lastUsedTime = Time.time;
    }

    private void CooldownFinished()
    {
        Debug.Log("cool down finished");
        OnCooldownFinished?.Invoke(this);
    }
}
