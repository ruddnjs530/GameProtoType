using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FlyingEnemy : Enemy
{
    protected override void Update()
    {
        base.Update();
    }

    //protected override void Chase()
    //{
    //    if (agentTarget == null) return;
    //    destination = agentTarget.position;
    //    agent.SetDestination(destination);
    //    agent.speed = chaseSpeed;
    //}

    protected override void Attack()
    {
        //Debug.Log("자식 attack");
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
            agentTarget = other.transform;
            isSeePlayer = true;
            Debug.Log("자식 in");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isSeePlayer = false;
            agentTarget = null;
            Debug.Log("자식 out");
        }
    }
}
