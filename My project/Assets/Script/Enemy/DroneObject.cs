using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DroneObject : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform agentTarget;
    private Vector3 destination;

    private enum DroneState { MoveToPlayer, Attack }
    private DroneState droneState;

    public delegate void DistanceEventHandler();
    public event DistanceEventHandler OnPlayerTooFar;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agentTarget = GameObject.FindWithTag("Player").transform;


        droneState = DroneState.MoveToPlayer;

        var turret = GetComponentInChildren<TurretObject>();
        turret.OnIsAttack += ChangeStateToAttack;
    }

    // Update is called once per frame
    void Update()
    {
        switch (droneState)
        {
            case DroneState.MoveToPlayer:

                destination = agentTarget.position;
                agent.SetDestination(destination);

                break;

            case DroneState.Attack:

                if (DistanceWithPlayer(agentTarget) > 10f)
                {
                    agent.isStopped = false;
                    droneState = DroneState.MoveToPlayer;
                    OnPlayerTooFar?.Invoke();
                }

                break;

        }
    }

    private float DistanceWithPlayer(Transform player)
    {
        float distance = Vector3.Distance(player.transform.position, this.transform.position);
        return distance;
    }

    private void ChangeStateToAttack()
    {
        agent.isStopped = true;
        droneState = DroneState.Attack;
    }
}
