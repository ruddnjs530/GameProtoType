using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DroneObject : MonoBehaviour
{
    NavMeshAgent agent;
    Transform agentTarget;
    Vector3 destination;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agentTarget = GameObject.FindWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        destination = agentTarget.position;
        agent.SetDestination(destination);
    }
}
