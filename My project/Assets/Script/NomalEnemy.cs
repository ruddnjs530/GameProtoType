using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NomalEnemy : Enemy
{
    int nomalPrice = 20;
    float attackDamage = 3f;

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
            agentTarget.gameObject.GetComponent<Player>().TakeDamage(attackDamage);
            attackDelay = 1f;
        }
    }

    private void OnTriggerEnter(Collider other) // 지금 콜라이더를 오브젝트 앞으로 이동 시켜서 뒤에서 왔을 때는 보지 못하게 함.
    {
        if (other.gameObject.tag == "Player") // 원래는 여기서 canAttack도 true, false로 해줬는데 콜라이더를 나눠서 enemyCanAttack 스크립트에서 관리하게 함.
        {
            agentTarget = other.transform;
            isSeePlayer = true;
            LookAtTarget(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
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
