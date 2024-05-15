using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BossSkill
{
    string skillName;
    float coolTime;
    public float lastUsedTime;

    public event Action<BossSkill> OnCooldownFinished;

    public string GetSkillName() { return skillName; }

    public BossSkill(string skillName, float coolTime)
    {
        this.skillName = skillName;
        this.coolTime = coolTime;
        this.lastUsedTime = Time.time - coolTime;
    }

    public void UpdateCoolTime()
    {
        if ((Time.time - lastUsedTime) >= coolTime)
        {
            OnCooldownFinished?.Invoke(this);
        }
    }

    public void SetSkillTime()
    {
        lastUsedTime = Time.time;
    }
}
