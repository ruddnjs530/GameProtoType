using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NomalEnemy : Enemy
{
    //private float attackDistance = 5f;
    //private float attackDamage = 3f;

    //public Transform targetTransform;

    protected override void Update()
    {
        base.Update();
    }


    protected override void Chase()
    {
        Debug.Log("자식 chase");
        if (agent == null) return;
        destination = agentTarget.position;
        agent.SetDestination(destination);
        agent.speed = chaseSpeed;
    }
    protected override void Attack()
    {
        Debug.Log("자식 attack");
        attackDelay -= Time.deltaTime;
        if (attackDelay < 0) attackDelay = 0;
        if (attackDelay == 0)
        {
            agentTarget.gameObject.GetComponent<Player>().TakeDamage(attackDamage);
            attackDelay = 2f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            canAttack = true;
            agentTarget = other.transform;
            Debug.Log("자식 enter");
            //target = other.transform.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            canAttack = false;
            agentTarget = null;
            Debug.Log("자식 exit");
            //target = null;
        }
    }
}
