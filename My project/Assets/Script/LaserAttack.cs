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
            // ��ų ���� �ڵ� (�ִϸ��̼� ��)
            // ��ų ���Ḧ ������ ��:
            //StartCoroutine(EndSkillAfterSeconds(5)); // 5�� �� ��ų ����
        }
    }

    private IEnumerator EndSkillAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        IsActive = false;
        OnSkillFinished?.Invoke();  // ��ų ���� �̺�Ʈ �߻�
    }
}
