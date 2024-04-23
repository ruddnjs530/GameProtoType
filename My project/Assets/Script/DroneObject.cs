using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DroneObject : MonoBehaviour
{
    NavMeshAgent agent;
    Transform agentTarget;
    Vector3 destination;

    private enum DroneState { MoveToPlayer, Attack }
    DroneState droneState;

    TurretObject turret;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agentTarget = GameObject.FindWithTag("Player").transform;

        turret = GetComponentInChildren<TurretObject>();

        droneState = DroneState.MoveToPlayer;
    }

    // Update is called once per frame
    void Update()
    {
        switch(droneState)
        {
            case DroneState.MoveToPlayer:
                if (turret.turretState == TurretState.Attack)
                {
                    agent.isStopped = true;
                    droneState = DroneState.Attack;
                    break;
                }            
                destination = agentTarget.position;
                agent.SetDestination(destination);

                break;

            case DroneState.Attack:
                if (turret.turretState == TurretState.Idle || DistanceWithPlayer(agentTarget) > 10f)
                {
                    agent.isStopped = false;
                    turret.turretState = TurretState.Idle; // Ŀ�ø��� �ʹ� ���Ѱ�? �ʹ� �ָԱ������ΰ�?
                    droneState = DroneState.MoveToPlayer;
                    break;
                }
                

                break;

        }
    }

    float DistanceWithPlayer(Transform player)
    {
        float distance = Vector3.Distance(player.transform.position, this.transform.position);
        return distance;
    }
}
