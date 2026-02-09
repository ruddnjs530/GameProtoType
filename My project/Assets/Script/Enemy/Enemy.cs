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
    [Header("Movement Settings")]
    [SerializeField] protected float walkSpeed = 1f; // 걷기 속도
    [SerializeField] protected float chaseSpeed = 6f; // 추격 속도
    [SerializeField] protected float roamRange = 180.0f; // 배회 범위
    [SerializeField] protected float moveInterval = 1.0f; // 이동 간격
    [SerializeField] protected float rotationSpeed = 3f; // 회전 속도

    protected NavMeshAgent agent;
    protected Transform agentTarget;
    protected Vector3 destination;

    [Header("Stats")]
    public float MaxHP { get; set; } = 100; // 최대 체력
    protected float currentHP = 100; // 현재 체력
    public int EnemyID { get; set; } // 적 ID

    [Header("Combat Settings")]
    public bool canAttack = true; // 공격 가능 여부
    public float attackTimer = 0.0f;
    protected float attackRate = 2.0f; // 공격 주기

    protected EnemyState enemyState; // 적 상태
    float currentTime = 0f;

    protected AnimatorStateInfo animatorStateInfo;
    protected Coroutine attackCoroutine = null;

    [Header("References")]
    [SerializeField] private GameObject textObject; // 텍스트 오브젝트 (용도 확인 필요)
    [SerializeField] protected Slider hpBar; // 체력바 UI
    [SerializeField] protected Transform enemyBody; // 적 모델
    public Transform EnemyBody { get { return enemyBody; } }

    protected bool isSeePlayer = false; // 플레이어 발견 여부
    protected Animator anim; // 애니메이터

    public delegate void EnemyDied(Enemy enemy);
    public event EnemyDied OnEnemyDied; // 사망 이벤트

    private HealthBar healthBar; // 체력바 스크립트
    private Coroutine currentCoroutine;

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
                else if (currentTime >= moveInterval)
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
                    agent.isStopped = true;
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
                    enemyState = EnemyState.Die;
                    break;
                }
                else if (!canAttack)
                {
                    agent.isStopped = false;
                    anim.ResetTrigger("Attack");
                    enemyState = EnemyState.Idle;
                    break;
                }
                Attack();
                //if (attackCoroutine == null)
                //{
                //    anim.SetTrigger("Attack");
                //    LookAt(agentTarget);

                //    attackCoroutine = StartCoroutine(Attack());
                //}

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
            currentCoroutine = StartCoroutine(healthBar.ShowAndHide()); // 오타 수정: ShwoAndHide -> ShowAndHide
        }
    }

    private void ReSetDestination()
    {
        float randomX = Random.Range(-roamRange, roamRange);
        float randomZ = Random.Range(-roamRange, roamRange);
        destination = new Vector3(randomX, 0f, randomZ);
    }

    private void SimpleMove()
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

    //protected abstract IEnumerator Attack();

    protected virtual void Attack()
    {
        anim.SetTrigger("Attack");
    }

    protected void LookAt(Transform target)
    {
        Vector3 direction = target.position - enemyBody.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction.normalized);
        enemyBody.rotation = Quaternion.Slerp(enemyBody.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    protected virtual void Die()
    {
        anim.SetBool("Die", true);

        OnEnemyDied?.Invoke(this);
        Destroy(gameObject);
    }

    public void LookAtDirection(Vector3 direction)
    {
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = lookRotation;
        enemyState = EnemyState.Chase;
    }
}

