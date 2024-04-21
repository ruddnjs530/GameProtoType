using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LaserAttack : IBossSkill
{
    public bool IsActive { get; set; }
    private float cooldownTime = 10f;
    private float lastUsedTime;
    public event Action OnSkillFinished;

    public bool CanActivate()
    {
        return !IsActive && Time.time >= lastUsedTime + cooldownTime;
    }

    public void Activate()
    {
        if (CanActivate())
        {
            IsActive = true;
            lastUsedTime = Time.time;
            Debug.Log("Fireball activated!");
            // 스킬 실행 코드 (애니메이션 등)
            // 스킬 종료를 가정한 예:
            //StartCoroutine(EndSkillAfterSeconds(5)); // 5초 후 스킬 종료
        }
    }

    private IEnumerator EndSkillAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        IsActive = false;
        OnSkillFinished?.Invoke();  // 스킬 종료 이벤트 발생
    }
}
