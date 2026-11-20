using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using UnityEngine.UI;


public enum BossState { Die, Attack, MoveToPlayer}
public class BossEnemy : MonoBehaviour
{
    private BossState bossState;
    private Transform target;
    private Animator anim;
    private NavMeshAgent agent;


    [SerializeField] private Transform shotPos;
    [SerializeField] private LineRenderer warningLine;
    private LineRenderer laser;

    public GameObject textObject;
    private float currentHP = 2000;
    private float maxHP = 2000;

    [SerializeField] private AnimationCurve heightCurve;

    [SerializeField] private LayerMask playerLayer;

    [SerializeField] private Projector projector;

    private float attackDamage = 10.0f;

    private float jumpSpeed = 2f;
    private int jumpDamage = 5;

    private float levitationAttackRange = 20f;
    private float attackAngle = 90f;

    private bool isAttacking = false;

    private float walkSpeed = 2f;

    private float rotationSpeed = 3f;

    private HealthBar healthBar;

    private bool canLaserAttack;

    public static event System.Action OnBossDeath;

    private List<IBossSkill> bossSkills;
    private IBossSkill currentBossSkill;

    public Transform Target => target;
    public Animator Anim => anim;
    public NavMeshAgent Agent => agent;
    public Transform ShotPos => shotPos;
    public LineRenderer Laser => laser;
    public LineRenderer WarningLine => warningLine;
    public AnimationCurve HeightCurve => heightCurve;
    public LayerMask PlayerLayer => playerLayer;
    public Projector Projector => projector;
    public bool CanLaserAttack => canLaserAttack;
    public float CurrentHP => currentHP;
    public float MaxHP => maxHP;
    public float AttackAngle => attackAngle;
    public float LevitationAttackRange => levitationAttackRange;
    public float AttackDamage => attackDamage;

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;

        canLaserAttack = false;

        laser = GetComponent<LineRenderer>();
        laser.enabled = false;

        warningLine.enabled = false;

        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        projector.enabled = false;
        // 바닥 경고 표시를 실제 판정 범위(반경 levitationAttackRange, 각도 attackAngle)와 같게 맞춘다.
        // Projector는 보스의 자식이라 한 번 맞추면 보스를 따라 회전한다.
        LevitationArea.AlignProjector(projector, transform.position, levitationAttackRange);

        isAttacking = false;  

        bossSkills = new List<IBossSkill>
        {
            new LaserAttack(),
            new LevitationAttack(),
        };

        ChangeToMoveToPlayerState();
    }

    // Update is called once per frame
    void Update()
    {
        switch (bossState)
        {
            case BossState.MoveToPlayer:
                CheckDeath();
                UpdateSkill();
                Move();
                break;

            case BossState.Attack:
                CheckDeath();

                // 스킬을 고른 프레임과 실행하는 프레임 사이에 조건이 바뀌면(예: 플레이어가 레이저 트리거를 벗어남)
                // 스킬이 시작되지 않는다. 이때 Attack 상태에 남으면 빠져나갈 경로가 없으므로 이동 상태로 되돌린다.
                if (!isAttacking)
                {
                    IBossSkill skill = currentBossSkill;
                    currentBossSkill = null;
                    if (!BossAttack(skill)) ChangeToMoveToPlayerState();
                }

                break;

            case BossState.Die:
                // 사망 처리는 EnterDie()에서 한 번만 한다. 씬 전환은 GameManager가 연출 시간 뒤에 처리한다.
                break;
        }

        healthBar.SetHealth(currentHP);
    }

    private void Move()
    {
        if (target == null) return;
        RotateTowardPlayer();
    }

    void RotateTowardPlayer()
    {
        Vector3 direction = (target.position - this.transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    // 스킬을 시작했으면 true, 조건이 맞지 않아 시작하지 못했으면 false를 돌려준다.
    private bool BossAttack(IBossSkill skill)
    {
        if (skill == null || !skill.CanExecute(this) || isAttacking)
            return false;

        isAttacking = true;
        StartCoroutine(skill.Execute(this));
        return true;
    }

    public bool CanLevitationAttack()
    {
        return Physics.CheckSphere(transform.position, levitationAttackRange, playerLayer);
    }

    //private IEnumerator JumpAttack(Vector3 targetPosition)
    //{
    //    anim.SetTrigger("jumpAttack");
    //    yield return new WaitForSeconds(0.5f);
    //    Vector3 startingPos = transform.position;
    //    for (float time = 0; time < 1; time += Time.deltaTime * jumpSpeed)
    //    {
    //        transform.position = Vector3.Lerp(startingPos, targetPosition, time) + Vector3.up * heightCurve.Evaluate(time);

    //        yield return null;
    //    }

    //    float distance = Vector3.Distance(this.transform.position, target.position);
    //    if (distance <= 10f) target.GetComponent<Player>().TakeDamage(jumpDamage);
    //    yield return new WaitForSeconds(2.0f);
    //    currentBossSkill = null;
    //    isAttacking = false;
    //}

    //private IEnumerator levitationAttack(Collider player)
    //{
    //    agent.updateRotation = false;

    //    Vector3 direction = (player.transform.position - transform.position).normalized;
    //    float angleBetween = Vector3.Angle(transform.forward, direction);

    //    if (angleBetween < attackAngle / 2)
    //    {
    //        CharacterController playerController = player.GetComponent<CharacterController>();
    //        if (playerController != null)
    //        {
    //            anim.SetTrigger("levitation");
    //            projector.enabled = true;
    //            yield return new WaitForSeconds(2);
    //            projector.enabled = false;

    //            direction = (player.transform.position - transform.position).normalized;
    //            angleBetween = Vector3.Angle(transform.forward, direction);
    //            if (angleBetween < attackAngle / 2)
    //            {
    //                GameManager.Instance.canPlayerMove = false;
    //                playerController.transform.gameObject.GetComponent<Player>().TakeDamage(attackDamage);
    //                playerController.transform.GetComponent<Player>().anim.SetTrigger("levitation");
    //                StartCoroutine(ApplyKnockback(playerController));
    //            }
    //        }
    //    }
    //    yield return new WaitForSeconds(3);

    //    currentBossSkill = null;
    //    isAttacking = false;
    //    GameManager.Instance.canPlayerMove = true;
    //}

    //private IEnumerator ApplyKnockback(CharacterController controller)
    //{
    //    Vector3 knockbackForce = new Vector3(0, 10, 0);
    //    float timer = 0;
    //    while (timer < 0.5f)
    //    {
    //        controller.Move(knockbackForce * Time.deltaTime);
    //        timer += Time.deltaTime;
    //        yield return null;
    //    }
    //}

    public void ChangeToMoveToPlayerState()
    {
        //currentBossSkill = null;
        isAttacking = false;
        //agent.SetDestination(target.position);
       // agent.speed = walkSpeed;
        agent.isStopped = false;
        //anim.SetBool("isWalking", true);
        bossState = BossState.MoveToPlayer;

        // 공격 애니메이션의 끝을 스킬 코루틴의 끝과 맞춘다. 예전에는 애니메이터의 Exit Time(레이저 약 3.8초)으로만 끝나
        // 스킬(4.3초)과 어긋났고, 이미 이동 상태로 돌아가 회전하는 동안 레이저 자세가 남았다.
        if (anim != null)
        {
            anim.ResetTrigger("LaserAttack");
            anim.ResetTrigger("LevitationAttack");
            anim.CrossFadeInFixedTime("Walk", AttackAnimFade);
        }
    }

    private const float AttackAnimFade = 0.15f; // 공격 애니메이션 진입·복귀 전환 시간(초)

    private void UpdateSkill()
    {
        if (bossState == BossState.Attack || currentBossSkill != null || isAttacking)
        {
            return;
        }

        foreach (var skill in bossSkills)
        {
            if (skill.CanExecute(this))
            {
                currentBossSkill = skill;
                bossState = BossState.Attack;
                break;
            }
        }
    }

    public void PrepareForAttack(string animationName)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(animationName))
            return;

        //anim.SetBool("isWalking", false);
        agent.isStopped = true;
        // 트리거 대신 상태로 바로 전환한다. Walk → 공격 전이가 Exit Time(루프 경계) 기준이라 트리거를 넣어도
        // 최대 1.3초(Walk 클립 한 바퀴) 늦게 시작됐고, 소비되지 않은 트리거가 다음 공격 애니메이션과 겹쳤다.
        anim.ResetTrigger("LaserAttack");
        anim.ResetTrigger("LevitationAttack");
        anim.CrossFadeInFixedTime(animationName, AttackAnimFade);
        Debug.Log("PrepareForAttack " + anim.GetCurrentAnimatorStateInfo(0).IsName(animationName));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canLaserAttack = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canLaserAttack = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canLaserAttack = false;
        }
    }

    public void TakeDamage(int damage)
    {
        if (bossState == BossState.Die) return;

        currentHP -= damage;
        CheckDeath();

        //anim.SetTrigger("isHit");
    }

    private void CheckDeath()
    {
        if (bossState != BossState.Die && currentHP <= 0) EnterDie();
    }

    // Die 상태에 진입할 때 한 번만 실행한다. 진행 중인 스킬 코루틴을 멈춰 사망 후 공격·경직이 남지 않게 한다.
    private void EnterDie()
    {
        bossState = BossState.Die;
        currentHP = 0;

        StopAllCoroutines();
        isAttacking = false;
        currentBossSkill = null;
        if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
        if (laser != null) laser.enabled = false;
        if (warningLine != null) warningLine.enabled = false;
        if (projector != null) projector.enabled = false;

        anim.SetBool("isDie", true);
        OnBossDeath?.Invoke();
    }

    public void SetHealthBar(Slider hpBar)
    {
        healthBar = new HealthBar(hpBar, maxHP, false, new Vector2(0.5f, 0.9f));
    }
}