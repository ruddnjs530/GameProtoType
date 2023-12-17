using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NomalEnemy : Enemy
{
    protected override void Update()
    {
        base.Update();
    }

    protected override void Chase()
    {
        Debug.Log("�ڽ� chase");
        if (agentTarget == null) return;
        destination = agentTarget.position;
        agent.SetDestination(destination);
        agent.speed = chaseSpeed;
    }

    protected override void Attack()
    {
        Debug.Log("�ڽ� attack");
        attackDelay -= Time.deltaTime;
        if (attackDelay < 0) attackDelay = 0;
        if (attackDelay == 0)
        {
            agentTarget.gameObject.GetComponent<Player>().TakeDamage(attackDamage);
            attackDelay = 2f;
        }
    }

    private void OnTriggerEnter(Collider other) // ���� �ݶ��̴��� ������Ʈ ������ �̵� ���Ѽ� �ڿ��� ���� ���� ���� ���ϰ� ��.
    {
        if (other.gameObject.tag == "Player") // ������ ���⼭ canAttack�� true, false�� ����µ� �ݶ��̴��� ������ enemyCanAttack ��ũ��Ʈ���� �����ϰ� ��.
        {
            agentTarget = other.transform;
            isSeePlayer = true;
            Debug.Log("�ڽ� enter");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isSeePlayer = false;
            agentTarget = null;
            Debug.Log("�ڽ� exit");
        }
    }
}
