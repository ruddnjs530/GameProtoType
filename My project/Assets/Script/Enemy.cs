using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum EnemyState { Idle, SimpleMove, Chase, Attack, Die }

public struct EnemyData
{
    public Vector3 position; // public으로 사용하려면 대문자로
    public int enemyID;
    public float GetDistance(Vector3 turretPos)
    {
        return Vector3.Distance(turretPos, position);
    }

}

public class Enemy : MonoBehaviour
{ 
    protected NavMeshAgent agent;
    protected Transform agentTarget;
    protected Vector3 destination;
    protected float walkSpeed = 3f;
    protected float chaseSpeed = 6f;
    public float maxHP = 100; // 프로퍼티
    protected float currentHP = 100;

    protected EnemyState enemyState;
    float currentTime = 0f;

    public bool canAttack = true;
    protected float attackDelay = 2f;

    [SerializeField] GameObject textObject;

    protected bool isSeePlayer = false;

    Animator anim;

    public delegate void EnemyDied(Enemy enemy);
    public event EnemyDied OnEnemyDied;

    [SerializeField] protected Slider hpBar;
    HealthBar healthBar;

    private Coroutine currentCoroutine;

    public int enemyID; // public으로 사용하려면 대문자로. 근데 프로퍼티로 만들기

    // Start is called before the first frame update
    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        enemyState = EnemyState.Idle;

        healthBar = new HealthBar(hpBar, maxHP, true);
        healthBar.Hide();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        switch (enemyState)
        {
            case EnemyState.Idle:
                LookAround();
                currentTime += Time.deltaTime;
                if (currentHP <= 0) enemyState = EnemyState.Die;
                else if(isSeePlayer) enemyState = EnemyState.Chase;
                else if (currentTime >= 1f) enemyState = EnemyState.SimpleMove;
                break;

            case EnemyState.SimpleMove:
                ReSetDestination();
                SimpleMove();
                currentTime = 0;
                if (currentHP <= 0) enemyState = EnemyState.Die;
                else if (isSeePlayer) enemyState = EnemyState.Chase;
                else enemyState = EnemyState.Idle;

                break;

            case EnemyState.Chase:
                Chase();
                if (currentHP <= 0) enemyState = EnemyState.Die;
                else if (canAttack)
                {
                    anim.SetBool("Chase", false);
                    enemyState = EnemyState.Attack;
                    break;
                }
                else if (!isSeePlayer)
                {
                    anim.SetBool("Chase", false);
                    enemyState = EnemyState.Idle;
                    break;
                }
                currentTime = 0;
                break;

            case EnemyState.Attack:
                Attack();
                if (currentHP <= 0)
                {
                    anim.SetBool("Attack", false);
                    enemyState = EnemyState.Die;
                    break;
                }
                else if (!canAttack)
                {
                    anim.SetBool("Attack", false);
                    enemyState = EnemyState.Idle;
                    break;
                }
                break;

            case EnemyState.Die:
                Die();
                break;
        }

        if (hpBar != null)
        {
            hpBar.value = currentHP / maxHP;
        }
    }

    public void TakeDamageAndInstantiateText(int damage, float yPos = 1)
    {
        if (currentHP <= 0)
        {
            enemyState = EnemyState.Die;
            return;
        }

        currentHP -= damage;
        GameObject text = Instantiate(textObject, MakeRandomPosition(yPos), Quaternion.identity);
        text.GetComponent<DamageText>().damage = damage;
        anim.SetTrigger("GetHit");
        if (hpBar != null)
        {
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(healthBar.ShwoAndHide());
        }
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
        destination.Set(Random.Range(-1.0f, 1.0f), 0f, Random.Range(-0.5f, 1f));
    }

    private void SimpleMove() // destination으로 이동.
    {
        agent.SetDestination(transform.position + destination * 4f);
        agent.speed = walkSpeed;
    }

    protected virtual void Chase()
    {
        if (agentTarget == null) return;
        destination = agentTarget.position;
        agent.SetDestination(destination);
        agent.speed = chaseSpeed;
        anim.SetBool("Chase", true);
    }

    protected virtual void Attack()
    {
        anim.SetBool("Attack", true);
    }

    protected virtual void Die()
    {
        anim.SetBool("Die", true);

        OnEnemyDied?.Invoke(this);
        Destroy(gameObject);
    }

    public void LookAtTarget(Collider col)
    {
        Vector3 direction = col.transform.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
}

