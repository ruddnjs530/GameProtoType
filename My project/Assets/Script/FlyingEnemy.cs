using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : Enemy
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        Move();
    }
    protected override void Chase()
    {
        Debug.Log("�ڽ� chase");
        if (agent == null) return;
        destination = agentTarget.position;
        agent.SetDestination(destination);
    }

    private void Move()
    {
        if (agentTarget == null) return;
        Debug.Log("�ڽ� �̵�");
        destination = agentTarget.position;
        agent.SetDestination(destination);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("����");
            agentTarget = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("����");
        if (other.gameObject.tag == "Player")
        {
            agentTarget = null;
        }
    }
}
