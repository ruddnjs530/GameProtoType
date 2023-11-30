using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState1 { Idle, SimpleMove, Chase, Attack, Die }

public class Enemy1 : MonoBehaviour
{
    protected NavMeshAgent agent;
    protected Transform agentTarget;
    protected Vector3 destination;
    protected float walkSpeed = 3f;
    protected float chaseSpeed = 6f;
    protected float hp = 100;

    EnemyState1 enemyState;
    float currentTime = 0f;

    protected bool canAttack = false;
    protected float attackDelay = 2f;

    [SerializeField] GameObject textObject;

    float viewAngle = 130f;
    float viewDistance = 20f;
    [SerializeField] protected LayerMask targetMask;

    protected float attackDamage = 3f;

    int enemyPrice = 20;
    public bool isDie = false;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        canAttack = false;
        enemyState = EnemyState1.Idle;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        switch (enemyState)
        {
            case EnemyState1.Idle:
                //Debug.Log("idle");
                if (hp <= 0) enemyState = EnemyState1.Die;
                else if (IsPlayerInTheView()) enemyState = EnemyState1.Chase;
                else if (currentTime >= 3f) enemyState = EnemyState1.SimpleMove;

                LookAround();
                currentTime += Time.deltaTime;
                break;

            case EnemyState1.SimpleMove:
                //Debug.Log("simple");
                if (hp <= 0) enemyState = EnemyState1.Die;
                else if (IsPlayerInTheView()) enemyState = EnemyState1.Chase;
                else enemyState = EnemyState1.Idle;

                ReSetDestination();
                SimpleMove();
                currentTime = 0;
                break;

            case EnemyState1.Chase:
                Chase();
                //Debug.Log("chase");
                if (hp <= 0) enemyState = EnemyState1.Die;
                else if (canAttack) enemyState = EnemyState1.Attack;
                else if (!IsPlayerInTheView()) enemyState = EnemyState1.Idle;
                currentTime = 0;
                break;

            case EnemyState1.Attack:
                Attack();
                //Debug.Log("attack");
                if (hp <= 0) enemyState = EnemyState1.Die;
                else if (!canAttack) enemyState = EnemyState1.Idle;
                break;

            case EnemyState1.Die:
                Die();
                break;
        }
        //if (agentTarget != null) Debug.Log("타겟있음");
    }

    public void TakeDamageAndInstantiateText(int damage)
    {
        hp -= damage;
        GameObject text = Instantiate(textObject, MakeRandomPosition(), Quaternion.identity);
        text.GetComponent<DamageText>().damage = damage;
        Debug.Log("damage text");
    }

    Vector3 MakeRandomPosition()
    {
        Vector3 textPosition;
        float rand = Random.Range(-0.5f, 0.5f);
        textPosition.x = transform.position.x + rand;
        textPosition.y = transform.position.y + 1;
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
        agent.ResetPath();
        destination.Set(Random.Range(-0.2f, 0.2f), 0f, Random.Range(0.5f, 1f));
    }

    private void SimpleMove() // destination으로 이동.
    {
        agent.SetDestination(transform.position + destination * 5f);
        agent.speed = walkSpeed;
    }

    protected virtual void Chase()
    {
        if (agent == null) return;
        destination = agentTarget.position;
        agent.SetDestination(destination);
        agent.speed = chaseSpeed;
    }

    protected virtual void Attack()
    {
        attackDelay -= Time.deltaTime;
        if (attackDelay < 0) attackDelay = 0;
        if (attackDelay == 0)
        {
            agentTarget.gameObject.GetComponent<Player>().TakeDamage(attackDamage);
            attackDelay = 2f;
        }
    }

    private void Die()
    {
        GameManager.Instance.IncreaseMoney(enemyPrice);
        isDie = true;
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            canAttack = true;
            agentTarget = other.transform;
            transform.LookAt(agentTarget);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            canAttack = false;
            agentTarget = null;
        }
    }

    private bool IsPlayerInTheView()
    {
        Collider[] target = Physics.OverlapSphere(transform.position, viewDistance, targetMask);

        for (int i = 0; i < target.Length; i++)
        {
            //if (target[i].gameObject.tag != "Player") return false;
            Transform targetTf = target[i].transform;
            if (targetTf.name == "Player00")
            {
                Vector3 direction = (targetTf.position - transform.position).normalized; // 플레이어의 방향
                float betweenEnemyAndPlayerAngle = Vector3.Angle(direction, transform.forward); // 플레이어와 enemy.forward로 사잇값을 구함

                if (betweenEnemyAndPlayerAngle < viewAngle * 0.5f) // 사잇값이 시야 * 0.5보다 작다면 시야 안에 있는 것임
                {
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position + transform.up, direction, out hit, viewDistance))   // 사이에 장애물이 있는지 확인
                    {
                        if (hit.transform.tag == "Player")
                        {
                            agentTarget = hit.transform;
                            return true;
                        }
                        else return false;
                    }
                }
            }
        }
        return false;
    }
}

