using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;


public enum BossState { Die, Attack, MoveToPlayer}
public class BossEnemy : MonoBehaviour
{
    BossState bossState;
    Transform target;
    float walkSpeed = 2f;

    [SerializeField] Transform shotPos;
    LineRenderer laserLine;
    float laserRange = 10f;
    float laserDuration = 0.5f;

    float attackDamage = 10.0f;

    public GameObject textObject;
    float hp = 20;

    Animator anim;

    NavMeshAgent agent;

    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform eightWayShotPos;

    [SerializeField] AnimationCurve HeightCurve;
    float jumpSpeed = 2f;
    int jumpDamage = 5;

    float levitationAttackRange = 20f;
    [SerializeField] LayerMask playerLayer;
    float levitationAttackAngle = 90f;
    [SerializeField] Projector projector;

    bool isAttacking = false;

    List<BossSkills> skills = new List<BossSkills>
    {
        new BossSkills("laserAttack", 2f, 3),
        new BossSkills("jumpAttack", 8f, 2),
        new BossSkills("levitationAttack", 12f, 1)
    };

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
    }

    // Update is called once per frame
    void Update()
    {
        switch (bossState)
        {
            case BossState.MoveToPlayer:
                CheckDeath();
                CheckAttackCoolTime();
                Move();
                break;

            case BossState.Attack:
                CheckDeath();

                bool hasAttacked = false;
                foreach (var skill in skills.OrderBy(s => s.GetPriority()))
                {
                    if (!isAttacking && skill.isReady())
                    {
                        Attack(skill);
                        hasAttacked = true;
                    }
                }

                if (!hasAttacked)
                {
                    anim.SetBool("isWalking", true);
                    agent.isStopped = false;
                    bossState = BossState.MoveToPlayer;
                    break;
                }
                break;

            case BossState.Die:
                Destroy(this.gameObject, 3f);
                break;
        }
    }

    private void Move()
    {
        if (target == null) return;
        agent.SetDestination(target.position);
        agent.speed = walkSpeed;
        anim.SetBool("isWalking", true);
    }

    private void Attack(BossSkills skill)
    {
        if (isAttacking) return;
        if (hp > 30)
        {
            StartCoroutine(LaserAttack());
            skill.setUseSkillTime();
        }
        else
        {
            switch (skill.GetSkillName())
            {
                case "laserAttack":
                    StartCoroutine(LaserAttack());
                    skill.setUseSkillTime();
                    break;
                case "levitationAttack":
                    Collider player = Physics.OverlapSphere(transform.position, levitationAttackRange, playerLayer).FirstOrDefault();
                    if (player != null)
                    {
                        StartCoroutine(levitationAttack(player));
                        skill.setUseSkillTime();
                    }
                    break;
                case "jumpAttack":
                    StartCoroutine(JumpAttack(target.position));
                    skill.setUseSkillTime();
                    break;
            }      
        }
    }

    IEnumerator JumpAttack(Vector3 targetPosition)
    {
        isAttacking = true;
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
        isAttacking = false;
    }

    IEnumerator LaserAttack()
    {
        isAttacking = true;
        laserLine.enabled = true;
        laserLine.SetPosition(0, shotPos.position);
        RaycastHit hit;
        Vector3 startPosition = shotPos.position;

        if (Physics.Raycast(startPosition, transform.forward + (transform.up), out hit, laserRange)) 
        {
            laserLine.SetPosition(1, hit.point);
            if (hit.transform.tag == "Player")
            {
                transform.LookAt(hit.transform);
                hit.transform.gameObject.GetComponent<Player>().TakeDamage(attackDamage);
            }
        }    
        else
            laserLine.SetPosition(1, startPosition + (transform.forward + (transform.up * -1)) * laserRange); 

        yield return new WaitForSeconds(laserDuration);

        laserLine.enabled = false;
        isAttacking = false;
    }

    IEnumerator levitationAttack(Collider player)
    {
        isAttacking = true;
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
    }

    IEnumerator ApplyKnockback(CharacterController controller)
    {
        Vector3 knockbackForce = new Vector3(0, 10, 0); 
        float timer = 0;
        while (timer < 0.5f)
        {
            controller.Move(knockbackForce * Time.deltaTime); 
            timer += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(2);
        isAttacking = false;
    }

    void CheckAttackCoolTime()
    {
        foreach (var skill in skills)
        {
            if (skill.isReady() && skill.GetSkillName() != "laserAttack")
            {
                anim.SetBool("isWalking", false);
                agent.isStopped = true;
                bossState = BossState.Attack;
                break; 
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            anim.SetBool("isWalking", false);
            agent.isStopped = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            bossState = BossState.Attack;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            target = other.transform;
            agent.isStopped = false;
            bossState = BossState.MoveToPlayer;
        }
    }

    public void TakeDamageAndInstantiateText(int damage)
    {
        hp -= damage;
        GameObject text = Instantiate(textObject, MakeRandomPosition(), Quaternion.identity);
        text.GetComponent<DamageText>().damage = damage;
        anim.SetTrigger("isHit");
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

    void CheckDeath()
    {
        if (hp <= 0)
        {
            anim.SetBool("isDie", true);
            bossState = BossState.Die;
        }
    }
}

public class BossSkills
{
    string skillname;
    float coolTime;
    float LastUsedTime;
    int priority;

    public string GetSkillName() { return skillname; }
    public int GetPriority() { return priority; }

    public BossSkills (string skillname, float coolTime, int priority)
    {
        this.skillname = skillname;
        this.coolTime = coolTime;
        this.LastUsedTime = -coolTime;
        this.priority = priority;
    }

    public bool isReady()
    {
        return (Time.time - LastUsedTime) >= coolTime;
    }

    public void setUseSkillTime()
    {
        LastUsedTime = Time.time;
    }
}