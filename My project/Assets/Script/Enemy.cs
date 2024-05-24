using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum EnemyState { Idle, SimpleMove, Chase, Attack, Die }

public struct EnemyData
{
    public Transform Transform { get; set; }
    public int EnemyDataID { get; set; }

    public EnemyData(Transform transform, int enemyID)
    {
        this.Transform = transform;
        this.EnemyDataID = enemyID;
    }

    public float GetDistance(Vector3 targetPosition)
    {
        return Vector3.Distance(targetPosition, Transform.position);
    }
}

public class Enemy : MonoBehaviour
{ 
    protected NavMeshAgent agent;
    protected Transform agentTarget;
    protected Vector3 destination;
    protected float walkSpeed = 3f;
    protected float chaseSpeed = 6f;
    public float MaxHP { get; set; } = 100;
    protected float currentHP = 100;

    protected EnemyState enemyState;
    float currentTime = 0f;

    public bool canAttack = true;
    protected float attackDelay = 2f;

    [SerializeField] private GameObject textObject;

    protected bool isSeePlayer = false;

    private Animator anim;

    public delegate void EnemyDied(Enemy enemy);
    public event EnemyDied OnEnemyDied;

    [SerializeField] protected Slider hpBar;
    private HealthBar healthBar;

    private Coroutine currentCoroutine;

    public int EnemyID { get; set; }

    [SerializeField] protected Transform enemyBody;
    public Transform EnemyBody { get { return enemyBody; } }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        enemyState = EnemyState.Idle;

        healthBar = new HealthBar(hpBar, MaxHP, true);
        healthBar.Hide();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        switch (enemyState)
        {
            case EnemyState.Idle:
                if (currentHP <= 0)
                {
                    enemyState = EnemyState.Die;
                    break;
                }
                else if (isSeePlayer)
                {
                    enemyState = EnemyState.Chase;
                    break;
                }
                else if (currentTime >= 1f)
                {
                    enemyState = EnemyState.SimpleMove;
                    break;
                }
                currentTime += Time.deltaTime;

                break;

            case EnemyState.SimpleMove:
                if (currentHP <= 0)
                {
                    enemyState = EnemyState.Die;
                    break;
                }
                else if (isSeePlayer)
                {
                    enemyState = EnemyState.Chase;
                    break;
                }
                else if (currentTime == 0)
                {
                    enemyState = EnemyState.Idle;
                    break;
                }

                ReSetDestination();
                SimpleMove();
                currentTime = 0;

                break;

            case EnemyState.Chase:
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
                Chase();
                break;

            case EnemyState.Attack:
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
                Attack();
                break;

            case EnemyState.Die:
                Die();
                break;
        }

        if (hpBar != null)
        {
            hpBar.value = currentHP / MaxHP;
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHP <= 0)
        {
            enemyState = EnemyState.Die;
            return;
        }

        currentHP -= damage;

        anim.SetTrigger("GetHit");
        if (hpBar != null)
        {
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(healthBar.ShwoAndHide());
        }
    }

    private void ReSetDestination() // 목표 재설정.
    {
        float randomX = Random.Range(-180.0f, 180.0f);
        float randomZ = Random.Range(-1.0f, 180.0f);
        destination = new Vector3(randomX, 0f, randomZ);
    }

    private void SimpleMove() // destination으로 이동.
    {
        agent.SetDestination(destination);
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

    public void LookAtDirection(Vector3 direction)
    {
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = lookRotation;
        enemyState = EnemyState.Chase;
    }
}

