using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DefaultBossSkill : IBossSkill
{
    public string Name { get; protected set; }
    public float CoolTime { get; protected set; }
    public float LastUsedTime { get; set; }

    public DefaultBossSkill(string name, float coolTime)
    {
        Name = name;
        CoolTime = coolTime;
        LastUsedTime = Time.time;
    }

    public bool IsCoolDownComplete()
    {
        return (Time.time - LastUsedTime) >= CoolTime;
    }

    public void SetSkillUsedTime()
    {
        LastUsedTime = Time.time;
    }

    public abstract bool CanExecute(BossEnemy boss);

    public abstract IEnumerator Execute(BossEnemy boss);
}
