using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//public enum EnemyState { Move, Attack, Die }
public class Enemy1 : MonoBehaviour
{
    NavMeshAgent agent;

    [SerializeField]
    Transform target;
    EnemyState enemyState;
    private bool canAttack;
    private float hp;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        target = GameObject.Find("Player").transform;
        //enemyState = EnemyState.Move;
    }

    // Update is called once per frame
    void Update()
    {
        //switch (enemyState)
        //{
        //    case EnemyState.Move:
        //        Move();
        //        break;
        //    case EnemyState.Attack:
        //        Attack();
        //        break;
        //    case EnemyState.Die:
        //        Die();
        //        break;
        //}
    }

    private void Move()
    {
        agent.SetDestination(target.position);
        //if (canAttack) enemyState = EnemyState.Attack;
        //if (hp <= 0) enemyState = EnemyState.Die;
    }

    private void Attack()
    {
        // 적의 공격 함수
        //if (!canAttack) enemyState = EnemyState.Move;
        //if (hp <= 0) enemyState = EnemyState.Die;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Player") canAttack = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Player") canAttack = false;
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
