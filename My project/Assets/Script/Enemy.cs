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
    protected float hp = 5;

    protected EnemyState enemyState;
    protected float currentTime = 0f;

    protected bool canAttack = false;

    protected bool seePlayer = false;
    [SerializeField] protected GameObject textObject;

    protected float viewAngle = 130f; 
    protected float viewDistance = 20f; 
    [SerializeField] protected LayerMask targetMask;  // Ÿ�� ����ũ(�÷��̾�)
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        seePlayer = false;
        canAttack = false;
        //target = GameObject.Find("Player").transform;
        enemyState = EnemyState.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        switch (enemyState)
        {
            case EnemyState.Idle:
                LookAround();
                currentTime += Time.deltaTime;

                if (currentTime >= 3f && !seePlayer && !canAttack)
                    enemyState = EnemyState.SimpleMove;
                else if (seePlayer && !canAttack)
                    enemyState = EnemyState.Chase;
                else if (canAttack) enemyState = EnemyState.Attack;
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
        View();
        //LookAround();

    }

    public void TakeDamage(int damage)
    {
        GameObject text = Instantiate(textObject);
        text.GetComponent<DamageText>().damage = damage;
    }

    protected void LookAround()
    {
        Debug.Log("look around");
        int randomAngle = Random.Range(-1, 1);
        transform.Rotate(Vector3.up * Time.deltaTime * 5f * randomAngle);
    }

    protected void ReSetDestination() // ��ǥ �缳��.
    {
        agent.ResetPath();
        destination.Set(Random.Range(-0.2f, 0.2f), 0f, Random.Range(0.5f, 1f));
    }

    private void SimpleMove() // destination���� �̵�.
    {
        Debug.Log("simple move");
        agent.SetDestination(transform.position + destination * 5f);
        agent.speed = walkSpeed;
    }

    protected void Chase()
    {
        Debug.Log("chase");
        destination = agentTarget.position;
        agent.SetDestination(destination);
        agent.speed = chaseSpeed;
    }

    private void Attack()
    {
        // ���� ���� �Լ�
        //Debug.Log("����");
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

    private Vector3 BoundaryAngle(float angle) // ��谡 �Ǵ� ��Ÿ ��ǥ�� ����
    {
        angle += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0f, Mathf.Cos(angle * Mathf.Deg2Rad));
    }

    private void View()
    {
        Vector3 _leftBoundary = BoundaryAngle(-viewAngle * 0.5f);  // z �� �������� �þ� ������ ���� ������ŭ �������� ȸ���� ���� (�þ߰��� ���� ��輱)
        Vector3 _rightBoundary = BoundaryAngle(viewAngle * 0.5f);  // z �� �������� �þ� ������ ���� ������ŭ ���������� ȸ���� ���� (�þ߰��� ������ ��輱)

        Debug.DrawRay(transform.position + transform.up, _leftBoundary, Color.red); // ��輱�� �׸��� ����
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
                            Debug.Log("hi");
                            seePlayer = true;
                            agentTarget = hit.transform;
                            Debug.Log(agentTarget.name);
                            Debug.DrawRay(transform.position + transform.up, direction, Color.blue);

                            enemyState = EnemyState.Chase;
                        }
                    }
                }
                else seePlayer = false;
            }
        }
    }
}
