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
        this.lastUsedTime = Time.time - coolTime;
    }

    public void UpdateCoolTime()
    {
        if ((Time.time - lastUsedTime) >= coolTime)
        {
            lastUsedTime = Time.time;
            OnCooldownFinished?.Invoke(this);
        }
    }

    public void setUseSkillTime()
    {
        lastUsedTime = Time.time;
    }
}
