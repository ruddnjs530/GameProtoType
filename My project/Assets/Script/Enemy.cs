using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Idle, SimpleMove, Chase, Attack, Die }

public class Enemy : MonoBehaviour
{ 
    protected NavMeshAgent agent;
    protected Transform agentTarget;
    protected Vector3 destination;
    protected float walkSpeed = 3f;
    protected float chaseSpeed = 6f;
    protected float hp = 100;

    EnemyState enemyState;
    float currentTime = 0f;

    public bool canAttack = true;
    protected float attackDelay = 2f;

    [SerializeField] GameObject textObject;

    public bool isDie = false;

    protected bool isSeePlayer = false;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        enemyState = EnemyState.Idle;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        switch (enemyState)
        {
            case EnemyState.Idle:
                LookAround();
                currentTime += Time.deltaTime;
                if (hp <= 0) enemyState = EnemyState.Die;
                else if(isSeePlayer) enemyState = EnemyState.Chase;
                else if (currentTime >= 1f) enemyState = EnemyState.SimpleMove;
                break;

            case EnemyState.SimpleMove:
                ReSetDestination();
                SimpleMove();
                currentTime = 0;
                if (hp <= 0) enemyState = EnemyState.Die;
                else if (isSeePlayer) enemyState = EnemyState.Chase;
                else enemyState = EnemyState.Idle;

                break;

            case EnemyState.Chase:
                Chase();
                if (hp <= 0) enemyState = EnemyState.Die;
                else if (canAttack) enemyState = EnemyState.Attack;
                else if (!isSeePlayer) enemyState = EnemyState.Idle;
                currentTime = 0;
                break;

            case EnemyState.Attack:
                Attack();
                if (hp <= 0) enemyState = EnemyState.Die;
                else if (!canAttack) enemyState = EnemyState.Idle;
                break;

            case EnemyState.Die:
                Die();
                break;
        }
    }

    public void TakeDamageAndInstantiateText(int damage, float yPos = 1)
    {
        hp -= damage;
        GameObject text = Instantiate(textObject, MakeRandomPosition(yPos), Quaternion.identity);
        text.GetComponent<DamageText>().damage = damage;
    }

    Vector3 MakeRandomPosition(float yPos)
    {
        Vector3 textPosition;
        float rand = Random.Range(-0.5f, 0.5f);
        textPosition.x = transform.position.x + rand;
        textPosition.y = transform.position.y + yPos;
        textPosition.z = transform.position.z + rand;
        return textPosition;
    }

    private void LookAround()
    {
        int randomAngle = Random.Range(-1, 1);
        transform.Rotate(Vector3.up * Time.deltaTime * 5f * randomAngle);
    }

    private void ReSetDestination() // 목표 재설정.
    {
        //agent.ResetPath();
        destination.Set(Random.Range(-0.5f, 0.5f), 0f, Random.Range(0.5f, 1f));
    }

    private void SimpleMove() // destination으로 이동.
    {
        agent.SetDestination(transform.position + destination * 7f);
        agent.speed = walkSpeed;
    }

    protected virtual void Chase()
    {
        if (agentTarget == null) return;
        destination = agentTarget.position;
        agent.SetDestination(destination);
        agent.speed = chaseSpeed;
    }

    protected virtual void Attack()
    {
    }

    protected virtual void Die()
    {
        isDie = true;
        Destroy(gameObject);
    }

    public void LookAtTarget(Collider col)
    {
        Vector3 direction = col.transform.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
}

