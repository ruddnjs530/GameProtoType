using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState1 { Idle, SimpleMove, Chase, Attack, Die }

public class Enemy1 : MonoBehaviour
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
                else if (IsPlayerInTheView()) enemyState = EnemyState.Chase;
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

    public void TakeDamage(int damage)
    {
        hp -= damage;
        //int rand = Random.Range(-1, 2);
        GameObject text = Instantiate(textObject, transform);
        text.GetComponent<DamageText>().damage = damage;
    }

    private void LookAround()
    {
        int randomAngle = Random.Range(-1, 1);
        transform.Rotate(Vector3.up * Time.deltaTime * 5f * randomAngle);
    }

    private void ReSetDestination() // ��ǥ �缳��.
    {
        agent.ResetPath();
        destination.Set(Random.Range(-0.2f, 0.2f), 0f, Random.Range(0.5f, 1f));
    }

    private void SimpleMove() // destination���� �̵�.
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

    private Vector3 BoundaryAngle(float angle) // ��谡 �Ǵ� ��Ÿ ��ǥ�� ����
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
        //Vector3 _leftBoundary = BoundaryAngle(-viewAngle * 0.5f);  // z �� �������� �þ� ������ ���� ������ŭ �������� ȸ���� ���� (�þ߰��� ���� ��輱)
        //Vector3 _rightBoundary = BoundaryAngle(viewAngle * 0.5f);  // z �� �������� �þ� ������ ���� ������ŭ ���������� ȸ���� ���� (�þ߰��� ������ ��輱)

        Collider[] target = Physics.OverlapSphere(transform.position, viewDistance, targetMask);

        for (int i = 0; i < target.Length; i++)
        {
            if (target[i].gameObject.tag != "Player") return false;

            Transform targetTf = target[i].transform;
            Vector3 direction = (targetTf.position - transform.position).normalized;
            float angle = Vector3.Angle(direction, transform.forward);

            if (angle < viewAngle * 0.5f)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + transform.up, direction, out hit, viewDistance))   // ���̿� ��ֹ��� �ִ��� Ȯ��
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

