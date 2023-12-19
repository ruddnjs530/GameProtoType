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

    public bool canAttack = false;
    protected float attackDelay = 2f;

    [SerializeField] GameObject textObject;

    float viewAngle = 130f;
    float viewDistance = 20f;
    [SerializeField] protected LayerMask targetMask;

    protected float attackDamage = 3f;

    int enemyPrice = 20;

    public bool isDie = false;

    protected bool isSeePlayer = false;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        canAttack = false;
        enemyState = EnemyState.Idle;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        switch (enemyState)
        {
            case EnemyState.Idle:
                if (hp <= 0) enemyState = EnemyState.Die;
                else if(isSeePlayer) enemyState = EnemyState.Chase;
                else if (currentTime >= 3f) enemyState = EnemyState.SimpleMove;
                //.Log("idle");
                LookAround();
                currentTime += Time.deltaTime;
                break;

            case EnemyState.SimpleMove:
                ReSetDestination();
                SimpleMove();
                currentTime = 0;
                if (hp <= 0) enemyState = EnemyState.Die;
                else if (isSeePlayer) enemyState = EnemyState.Chase;
                else enemyState = EnemyState.Idle;
                //Debug.Log("simple");

                break;

            case EnemyState.Chase:
                Chase();
                //Debug.Log("chase");
                if (hp <= 0) enemyState = EnemyState.Die;
                else if (canAttack) enemyState = EnemyState.Attack;
                else if (!isSeePlayer) enemyState = EnemyState.Idle;
                currentTime = 0;
                break;

            case EnemyState.Attack:
                Attack();
                //Debug.Log("attack");
                if (hp <= 0) enemyState = EnemyState.Die;
                else if (!canAttack) enemyState = EnemyState.Idle;
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

    protected virtual void Chase()
    {
        //if (agent == null) return;
        //destination = agentTarget.position;
        //agent.SetDestination(destination);
        //agent.speed = chaseSpeed;
    }

    protected virtual void Attack()
    {
        //attackDelay -= Time.deltaTime;
        //if (attackDelay < 0) attackDelay = 0;
        //if (attackDelay == 0)
        //{
        //    agentTarget.gameObject.GetComponent<Player>().TakeDamage(attackDamage);
        //    attackDelay = 2f;
        //}
    }

    private void Die()
    {
        GameManager.Instance.IncreaseMoney(enemyPrice);
        isDie = true;
        Destroy(gameObject);
    }

    public void SetAgentTarget(Collider col)
    {
        agentTarget = col.transform;
    }

    public bool IsAgentTargetExist()
    {
        if (agentTarget != null) return true;
        return false;
    }

    public void LookAtTarget(Collider col)
    {
        transform.LookAt(col.transform);
    }


    //protected bool IsPlayerInTheView()
    //{
    //    Collider[] target = Physics.OverlapSphere(transform.position, viewDistance, targetMask);

    //    for (int i = 0; i < target.Length; i++)
    //    {
    //        if (target[i].gameObject.tag != "Player") return false;

    //        Transform targetTf = target[i].transform;
    //        Vector3 direction = (targetTf.position - transform.position).normalized; // �÷��̾��� ����
    //        float betweenEnemyAndPlayerAngle = Vector3.Angle(direction, transform.forward); // �÷��̾�� enemy.forward�� ���հ��� ����

    //        if (betweenEnemyAndPlayerAngle < viewAngle * 0.5f) // ���հ��� �þ� * 0.5���� �۴ٸ� �þ� �ȿ� �ִ� ��
    //        {
    //            RaycastHit hit;
    //            if (Physics.Raycast(transform.position + transform.up, direction, out hit, viewDistance))   // ���̿� ��ֹ��� �ִ��� Ȯ��
    //            {
    //                if (hit.transform.tag == "Player")
    //                {
    //                    agentTarget = hit.transform;
    //                    return true;
    //                }
    //                else return false;
    //            }
    //        }
    //    }
    //    return false;
    //}
}

