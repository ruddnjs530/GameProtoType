using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Idle, SimpleMove, Chase, Attack, Die }

public class Enemy : MonoBehaviour
{ 
    NavMeshAgent agent;
    Transform agentTarget;
    Vector3 destination;
    float walkSpeed = 3f;
    float chaseSpeed = 6f;
    float hp = 100;

    EnemyState enemyState;
    float currentTime = 0f;

    bool canAttack = false;
    float attackDelay = 2f;

    [SerializeField] GameObject textObject;

    float viewAngle = 130f; 
    float viewDistance = 20f; 
    [SerializeField] LayerMask targetMask;

    float attackDamage = 3f;

    int enemyPrice = 20;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        canAttack = false;
        enemyState = EnemyState.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        switch (enemyState)
        {
            case EnemyState.Idle:
                if (hp <= 0) enemyState = EnemyState.Die;
                else if(IsPlayerInTheView()) enemyState = EnemyState.Chase;
                else if (currentTime >= 3f) enemyState = EnemyState.SimpleMove;
                
                LookAround();
                currentTime += Time.deltaTime;
                //Debug.Log("idle");
                break;

            case EnemyState.SimpleMove:
                if (hp <= 0) enemyState = EnemyState.Die;
                else if (IsPlayerInTheView()) enemyState = EnemyState.Chase;
                else enemyState = EnemyState.Idle;

                ReSetDestination();
                SimpleMove();
                currentTime = 0;

                //Debug.Log("simple");
                break;

            case EnemyState.Chase:
                Chase();

                if (hp <= 0) enemyState = EnemyState.Die;
                else if (canAttack) enemyState = EnemyState.Attack;
                else if (!IsPlayerInTheView()) enemyState = EnemyState.Idle;
                //Debug.Log("chase");
                break;

            case EnemyState.Attack:
                Attack();

                if (hp <= 0) enemyState = EnemyState.Die;
                else if (!canAttack) enemyState = EnemyState.Idle;
                //Debug.Log("attack");
                break;

            case EnemyState.Die:
                Die();
                break;
        }
    }

    public void TakeDamageAndInstantiateText(int damage)
    {
        hp -= damage;
        GameObject text = Instantiate(textObject, MakeRandomPosition(), Quaternion.identity);
        text.GetComponent<DamageText>().damage = damage;
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

    private void Chase()
    {
        destination = agentTarget.position;
        agent.SetDestination(destination);
        agent.speed = chaseSpeed;
    }

    private void Attack()
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
        Destroy(gameObject);
    }

    private Vector3 BoundaryAngle(float angle) // 경계가 되는 델타 좌표를 구함
    {
        angle += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0f, Mathf.Cos(angle * Mathf.Deg2Rad));
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
        //Vector3 _leftBoundary = BoundaryAngle(-viewAngle * 0.5f);  // z 축 기준으로 시야 각도의 절반 각도만큼 왼쪽으로 회전한 방향 (시야각의 왼쪽 경계선)
        //Vector3 _rightBoundary = BoundaryAngle(viewAngle * 0.5f);  // z 축 기준으로 시야 각도의 절반 각도만큼 오른쪽으로 회전한 방향 (시야각의 오른쪽 경계선)

        Collider[] target = Physics.OverlapSphere(transform.position, viewDistance, targetMask);

        for (int i = 0; i < target.Length; i++)
        {
            if (target[i].gameObject.tag != "Player") return false;

            Transform targetTf = target[i].transform;
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
        return false;
    }
}

