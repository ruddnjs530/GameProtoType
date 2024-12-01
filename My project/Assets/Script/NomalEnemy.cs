using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NomalEnemy : Enemy
{
    private int nomalPrice = 10;
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
        attackDelay -= Time.deltaTime;
        if (attackDelay < 0) attackDelay = 0;
        if (attackDelay == 0)
        {
            Debug.Log("enemy attack");
            agentTarget.gameObject.GetComponent<Player>().TakeDamage(attackDamage);
            attackDelay = 1f;
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            agentTarget = other.transform;
            isSeePlayer = true;
            LookAtTarget(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canAttack = false;
            isSeePlayer = false;
            agentTarget = null;
        }
    }

    protected override void Die()
    {
        GameManager.Instance.IncreaseMoney(nomalPrice);
        base.Die();
    }
}
