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

    //TurretObject turret;

    public delegate void DistanceEventHandler();
    public event DistanceEventHandler OnPlayerTooFar;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agentTarget = GameObject.FindWithTag("Player").transform;

        //turret = GetComponentInChildren<TurretObject>();

        droneState = DroneState.MoveToPlayer;

        var turret = GetComponentInChildren<TurretObject>();
        turret.OnIsAttack += ChangeStateToAttack;
    }

    // Update is called once per frame
    void Update()
    {
        switch(droneState)
        {
            case DroneState.MoveToPlayer:
                //if (turret.turretState == TurretState.Attack)
                //{
                //    agent.isStopped = true;
                //    droneState = DroneState.Attack;
                //    break;
                //}            
                destination = agentTarget.position;
                agent.SetDestination(destination);

                break;

            case DroneState.Attack:
                //if (turret.turretState == TurretState.Idle || DistanceWithPlayer(agentTarget) > 10f)
                //{
                //    agent.isStopped = false;
                //    turret.turretState = TurretState.Idle; // 커플링이 너무 심한가? 너무 주먹구구식인가?
                //    droneState = DroneState.MoveToPlayer;
                //    break;
                //}

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
