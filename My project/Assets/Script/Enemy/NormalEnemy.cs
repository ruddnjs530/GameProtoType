using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalEnemy : Enemy
{
    private int normalPrice = 10;
    private float attackDamage = 3f;

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
            if (animatorStateInfo.IsName("Attack01") || animatorStateInfo.normalizedTime > 0.95f)
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
        GameManager.Instance.IncreaseMoney(normalPrice);
        base.Die();
    }
}
