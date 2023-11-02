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

    protected EnemyState enemyState;
    protected float currentTime = 0f;

    protected bool canAttack = false;
    private float attackDelay = 2f;

    protected bool seePlayer = false;
    [SerializeField] protected GameObject textObject;

    protected float viewAngle = 130f; 
    protected float viewDistance = 20f; 
    [SerializeField] protected LayerMask targetMask;

    private float attackDistance = 5f;
    private float attackDamage = 3f;

    // Start is called before the first frame update
    protected void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        seePlayer = false;
        canAttack = false;
        enemyState = EnemyState.Idle;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        View();
        CanAttack();

        switch (enemyState)
        {
            case EnemyState.Idle:
                LookAround();
                currentTime += Time.deltaTime;

                if (currentTime >= 3f && !seePlayer)
                    enemyState = EnemyState.SimpleMove;
                else if (seePlayer)
                    enemyState = EnemyState.Chase;
                else if (hp <= 0) enemyState = EnemyState.Die;
                break;

            case EnemyState.SimpleMove:
                ReSetDestination();
                SimpleMove();
                currentTime = 0;

                if (hp <= 0) enemyState = EnemyState.Die;
                else enemyState = EnemyState.Idle;
                break;

            case EnemyState.Chase:
                Chase();

                if (hp <= 0) enemyState = EnemyState.Die;
                else if (canAttack) enemyState = EnemyState.Attack;
                else if (!seePlayer) enemyState = EnemyState.Idle;
                break;

            case EnemyState.Attack:
                Attack();

                if (!canAttack) enemyState = EnemyState.Idle;
                break;

            case EnemyState.Die:
                Die();
                break;
        }
    }

    public virtual void TakeDamage(int damage)
    {
        hp -= damage;
        GameObject text = Instantiate(textObject, transform);
        text.GetComponent<DamageText>().damage = damage;
    }

    protected void LookAround()
    {
        int randomAngle = Random.Range(-1, 1);
        transform.Rotate(Vector3.up * Time.deltaTime * 5f * randomAngle);
    }

    protected void ReSetDestination() // 목표 재설정.
    {
        agent.ResetPath();
        destination.Set(Random.Range(-0.2f, 0.2f), 0f, Random.Range(0.5f, 1f));
    }

    protected void SimpleMove() // destination으로 이동.
    {
        agent.SetDestination(transform.position + destination * 5f);
        agent.speed = walkSpeed;
    }

    protected virtual void Chase()
    {
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

    private void CanAttack()
    {
        Collider[] target = Physics.OverlapSphere(transform.position, attackDistance, targetMask);
        for (int i = 0; i < target.Length; i++)
        {
            Transform targetTf = target[i].transform;
            if (targetTf.tag == "Player")
            {
                canAttack = true;
                transform.LookAt(targetTf);
                agentTarget = targetTf;
            }
            else
            {
                canAttack = false;
            }
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    protected Vector3 BoundaryAngle(float angle) // 경계가 되는 델타 좌표를 구함
    {
        angle += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0f, Mathf.Cos(angle * Mathf.Deg2Rad));
    }

    protected void View()
    {
        Vector3 _leftBoundary = BoundaryAngle(-viewAngle * 0.5f);  // z 축 기준으로 시야 각도의 절반 각도만큼 왼쪽으로 회전한 방향 (시야각의 왼쪽 경계선)
        Vector3 _rightBoundary = BoundaryAngle(viewAngle * 0.5f);  // z 축 기준으로 시야 각도의 절반 각도만큼 오른쪽으로 회전한 방향 (시야각의 오른쪽 경계선)

        Debug.DrawRay(transform.position + transform.up, _leftBoundary, Color.red); // 경계선을 그리기 위함
        Debug.DrawRay(transform.position + transform.up, _rightBoundary, Color.red);

        Collider[] target = Physics.OverlapSphere(transform.position, viewDistance, targetMask);

        for (int i = 0; i < target.Length; i++)
        {
            Transform targetTf = target[i].transform;
            if (targetTf.tag == "Player")
            {
                Vector3 direction = (targetTf.position - transform.position).normalized;
                float angle = Vector3.Angle(direction, transform.forward);

                if (angle < viewAngle * 0.5f)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position + transform.up, direction, out hit, viewDistance))
                    {
                        if (hit.transform.tag == "Player")
                        {
                            seePlayer = true;
                            agentTarget = hit.transform;
                            Debug.DrawRay(transform.position + transform.up, direction, Color.blue);

                            enemyState = EnemyState.Chase;
                        }
                        else seePlayer = false;
                    }
                }
            }
        }
    }
}
