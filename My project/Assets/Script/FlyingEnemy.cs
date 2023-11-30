using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FlyingEnemy : Enemy
{
    protected NavMeshAgent agent2;
    protected Transform agentTarget2;

    // Start is called before the first frame update
    void Start()
    {
        agent2 = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        //base.Update();
        Move();
    }
    protected override void Chase()
    {
        Debug.Log("자식 chase");
        if (agent2 == null) return;
        destination = this.agentTarget2.position;
        agent2.SetDestination(destination);
    }

    private void Move()
    {
        if (agentTarget2 == null) return;
        Debug.Log("자식 이동");
        destination = this.agentTarget2.position;
        agent2.SetDestination(destination);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("들어옴");
            agentTarget2 = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("나감");
        if (other.gameObject.tag == "Player")
        {
            agentTarget2 = null;
        }
    }
}
