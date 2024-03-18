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

    //protected override void Chase()
    //{
    //    if (agentTarget == null) return;
    //    //Debug.Log(agentTarget);
    //    destination = agentTarget.position;
    //    agent.SetDestination(destination);
    //    agent.speed = chaseSpeed;
    //    //Debug.Log("�ڽ� chase");
    //    //Debug.Log(chaseSpeed);
    //}

    protected override void Attack()
    {
        base.Attack();
        if (agentTarget == null) return;
        attackDelay -= Time.deltaTime;
        if (attackDelay < 0) attackDelay = 0;
        if (attackDelay == 0)
        {
            agentTarget.gameObject.GetComponent<Player>().TakeDamage(attackDamage);
            attackDelay = 2f;
        }
        Debug.Log("hello");
    }

    private void OnTriggerEnter(Collider other) // ���� �ݶ��̴��� ������Ʈ ������ �̵� ���Ѽ� �ڿ��� ���� ���� ���� ���ϰ� ��.
    {
        if (other.gameObject.tag == "Player") // ������ ���⼭ canAttack�� true, false�� ����µ� �ݶ��̴��� ������ enemyCanAttack ��ũ��Ʈ���� �����ϰ� ��.
        {
            Debug.Log("player in");
            //canAttack = true;
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
        Destroy(gameObject);
    }
}
