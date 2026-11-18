using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBossSkill
{
    string Name { get; }
    float CoolTime { get; }
    float LastUsedTime { get; set; }
    bool CanExecute(BossEnemy boss);
    bool IsCoolDownComplete();
    IEnumerator Execute(BossEnemy boss);
    void SetSkillUsedTime();
}
