using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalEnemy : Enemy
{
    [Header("Normal Enemy Settings")]
    [SerializeField] private int rewardMoney = 10; // 처치 시 보상금
    [SerializeField] private float attackDamage = 3f; // 공격 데미지
    [SerializeField] private float attackTiming = 0.95f; // 공격 판정 시점 (애니메이션 진행도)

    protected override void Start()
    {
        base.Start();
    }
    protected override void Update()
    {
        base.Update();
    }

    protected override void Attack()
    {
        base.Attack();
        if (agentTarget == null) return;
        LookAt(agentTarget);

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate)
        {
            animatorStateInfo = anim.GetCurrentAnimatorStateInfo(0);
            if (animatorStateInfo.IsName("Attack01") || animatorStateInfo.normalizedTime > attackTiming)
            {
                agentTarget.gameObject.GetComponent<Player>().TakeDamage(attackDamage);
                attackTimer = 0f;
            }
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            agentTarget = other.transform;
            isSeePlayer = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isSeePlayer = false;
            agentTarget = null;
        }
    }

    protected override void Die()
    {
        GameManager.Instance.IncreaseMoney(rewardMoney);
        base.Die();
    }
}
