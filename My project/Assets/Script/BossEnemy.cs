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
    private float walkSpeed = 2f;

    [SerializeField] private Transform shotPos;
    private LineRenderer laserLine;
    private float laserRange = 15f;
    private float laserDuration = 0.5f;

    private float attackDamage = 10.0f;

    public GameObject textObject;
    private float currentHP = 10000f;
    private float maxHP = 10000f;

    private Animator anim;

    private NavMeshAgent agent;

    [SerializeField] private AnimationCurve HeightCurve;
    private float jumpSpeed = 2f;
    private int jumpDamage = 5;

    private float levitationAttackRange = 20f;
    [SerializeField] private LayerMask playerLayer;
    private float levitationAttackAngle = 90f;
    [SerializeField] private Projector projector;

    private bool isAttacking = false;

    private List<BossSkill> skills = new List<BossSkill>();
    private BossSkill currentSkill;

    private HealthBar healthBar;

    private bool canLaserAttack = false;

    public static event System.Action OnBossDeath;

    // Start is called before the first frame update
    void Start()
    {
        bossState = BossState.MoveToPlayer;
        laserLine = GetComponent<LineRenderer>();

        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        laserLine.enabled = false;

        target = GameObject.FindGameObjectWithTag("Player").transform;
        projector.enabled = false;
        transform.LookAt(target);

        skills.Add(new BossSkill("laserAttack", 3f));
        skills.Add(new BossSkill("jumpAttack", 15f));
        skills.Add(new BossSkill("levitationAttack", 12f));

        foreach (var skill in skills)
        {
            skill.OnCooldownFinished += HandleSkillReady;
        }    
    }

    // Update is called once per frame
    void Update()
    {
        switch (bossState)
        {
            case BossState.MoveToPlayer:
                CheckDeath();
                Move();
                break;

            case BossState.Attack:
                CheckDeath();
                if (currentSkill == null)
                {
                    anim.SetBool("isWalking", true);
                    agent.isStopped = false;
                    bossState = BossState.MoveToPlayer;
                    break;
                }

                if (isAttacking == false)
                    Attack(currentSkill);
                break;

            case BossState.Die:
                OnBossDeath?.Invoke();
                Destroy(this.gameObject, 3f);
                break;
        }    
        healthBar.SetHealth(currentHP);
        UpdateSkill();
    }

    private void Move()
    {
        if (target == null) return;
        agent.SetDestination(target.position);
        agent.speed = walkSpeed;
        anim.SetBool("isWalking", true);
    }

    private void Attack(BossSkill skill)
    {
        if (skill == null || anim.GetCurrentAnimatorStateInfo(0).IsName("GetHit")) return;

        isAttacking = true;

        if (currentHP > 500)
        {
            if (canLaserAttack) StartCoroutine(LaserAttack(target.position));
            else isAttacking = false;
            skill.SetSkillTime();
            return;
        }

        switch (skill.GetSkillName())
        {
            case "laserAttack":
                if (canLaserAttack)
                {
                    StartCoroutine(LaserAttack(target.position));
                    skill.SetSkillTime();
                }
                else isAttacking = false;
                break;

            case "levitationAttack":
                Collider player = Physics.OverlapSphere(transform.position, levitationAttackRange, playerLayer).FirstOrDefault();
                if (player != null)
                {
                    StartCoroutine(levitationAttack(player));
                    skill.SetSkillTime();
                }
                else isAttacking = false;
                break;

            case "jumpAttack":
                StartCoroutine(JumpAttack(target.position));
                skill.SetSkillTime();
                break;
        }
    }

    private IEnumerator JumpAttack(Vector3 targetPosition)
    {
        anim.SetTrigger("jumpAttack");
        yield return new WaitForSeconds(0.5f);     
        Vector3 startingPos = transform.position;
        for (float time = 0; time < 1; time += Time.deltaTime * jumpSpeed)
        {
            transform.position = Vector3.Lerp(startingPos, targetPosition, time) + Vector3.up * HeightCurve.Evaluate(time);

            yield return null;
        }

        float distance = Vector3.Distance(this.transform.position, target.position);
        if (distance <= 10f) target.GetComponent<Player>().TakeDamage(jumpDamage);
        yield return new WaitForSeconds(1.0f);
        currentSkill = null;
        isAttacking = false;
    }

    private IEnumerator LaserAttack(Vector3 targetPosition)
    {
        Vector3 directionToPlayer = (target.position - transform.position).normalized;

        if (Vector3.Angle(transform.forward, directionToPlayer) < levitationAttackAngle / 2)
        {
            RaycastHit hit;
            Vector3 startPosition = shotPos.position;
            Vector3 direction = (targetPosition - startPosition).normalized;

            laserLine.enabled = true;
            laserLine.SetPosition(0, startPosition);

            if (Physics.Raycast(startPosition, direction, out hit, laserRange))
            {
                laserLine.SetPosition(1, hit.point);
                if (hit.transform.CompareTag("Player"))
                {
                    Player player = hit.transform.GetComponent<Player>();
                    if (player != null)
                    {
                        player.TakeDamage(attackDamage);
                    }
                }
            }

            yield return new WaitForSeconds(laserDuration);
            laserLine.enabled = false;
        }

        currentSkill = null;
        isAttacking = false;
    }

    private IEnumerator levitationAttack(Collider player) 
    {
        agent.updateRotation = false;

        Vector3 direction = (player.transform.position - transform.position).normalized;
        float angleBetween = Vector3.Angle(transform.forward, direction);

        if (angleBetween < levitationAttackAngle / 2)
        {
            CharacterController playerController = player.GetComponent<CharacterController>();
            if (playerController != null)
            {
                anim.SetTrigger("levitation");
                projector.enabled = true;
                yield return new WaitForSeconds(2);
                projector.enabled = false;

                direction = (player.transform.position - transform.position).normalized;
                angleBetween = Vector3.Angle(transform.forward, direction);
                if (angleBetween < levitationAttackAngle / 2)
                {
                    playerController.transform.gameObject.GetComponent<Player>().TakeDamage(attackDamage);
                    playerController.transform.GetComponent<Player>().anim.SetTrigger("levitation");
                    StartCoroutine(ApplyKnockback(playerController));
                }
            }
        }
        yield return new WaitForSeconds(2);
        agent.updateRotation = true;
        currentSkill = null;
        isAttacking = false;
    }

    private IEnumerator ApplyKnockback(CharacterController controller)
    {
        Vector3 knockbackForce = new Vector3(0, 10, 0); 
        float timer = 0;
        while (timer < 0.5f)
        {
            controller.Move(knockbackForce * Time.deltaTime); 
            timer += Time.deltaTime;
            yield return null;
        }
    }


    private void HandleSkillReady(BossSkill skill)
    {
        if (isAttacking) return;

        currentSkill = skill;

        anim.SetBool("isWalking", false);
        agent.isStopped = true;
        bossState = BossState.Attack;
    }

    private void UpdateSkill()
    {
        foreach (var skill in skills)
        {
            skill.UpdateCoolTime();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            anim.SetBool("isWalking", false);
            agent.isStopped = true;
            canLaserAttack = true;
            bossState = BossState.Attack;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            anim.SetBool("isWalking", false);
            agent.isStopped = true;
            bossState = BossState.Attack;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canLaserAttack = false;
            target = other.transform;
            agent.isStopped = false;
            anim.SetBool("isWalking", true);
            bossState = BossState.MoveToPlayer;
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHP <= 0)
        {
            bossState = BossState.Die;
            return;
        }

        currentHP -= damage;

        anim.SetTrigger("isHit");
    }

    private void CheckDeath()
    {
        if (currentHP <= 0)
        {
            anim.SetBool("isDie", true);
            bossState = BossState.Die;
        }
    }

    public void SetHealthBar(Slider hpBar)
    {
        healthBar = new HealthBar(hpBar, maxHP, false, new Vector2(0.5f, 0.9f));
    }
}