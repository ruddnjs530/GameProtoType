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
    bool canAttack = false;

    [SerializeField] Transform shotPos;
    LineRenderer laserLine;
    float laserRange = 10f;
    float laserDuration = 0.5f;
    float fireRate = 1.0f;

    float attackDamage = 10.0f;

    float timer = 0.0f;

    public GameObject textObject;
    float hp = 20;

    float walkSpeed = 2f;

    Animator anim;

    NavMeshAgent agent;

    float jumpAttackCoolTime = 6f; 
    float eightWayAttackCoolTime = 4f;

    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform eightWayShotPos;

    [SerializeField] AnimationCurve HeightCurve;
    float jumpSpeed = 2f;
    int jumpDamage = 5;

    float levitationAttackRange = 20f;
    [SerializeField] LayerMask playerLayer;
    float levitationAttackAngle = 90f;

    List<BossSkills> skills = new List<BossSkills>
    {
        new BossSkills("laserAttack", 1f, 1),
        new BossSkills("jumpAttack", 5f, 2),
        new BossSkills("levitationAttack", 8f, 3)
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
                foreach (var skill in skills)
                {
                    if (skill.isReady())
                    {
                        Attack(skill);
                        skill.setUseSkillTime();
                    }
                }
                break;

            case BossState.Die:
                anim.SetBool("isDie", true);
                Destroy(this.gameObject, 3f);
                break;
        }
    }

    private void Move()
    {
        agent.SetDestination(target.position);
        agent.speed = walkSpeed;
        anim.SetBool("isWalking", true);
    }

    private void Attack(BossSkills skill) // 가독성이 좋아보이지는 않음 BossSkills skills
    {
        //if (hp > 30)
        //{
        //    timer += Time.deltaTime;
        //    if (timer > fireRate)
        //    {
        //        StartCoroutine(LaserAttack());
        //        timer = 0.0f;
        //    }
        //}
        //else
        //{
        //    if (CanExecuteJumpAttack())
        //    {
        //        StartCoroutine(JumpAttack(target.position));
        //        jumpAttackCoolTime = Time.time + jumpAttackCoolTime;
        //    }
        //    else if (CanExecuteEightWayAttack())
        //    {
        //        StartCoroutine(JumpAttack(target.position));
        //        jumpAttackCoolTime = Time.time + jumpAttackCoolTime;
        //    }
        //    else
        //    {
        //        timer += Time.deltaTime;
        //        if (timer > fireRate)
        //        {
        //            StartCoroutine(LaserAttack());
        //            timer = 0.0f;
        //        }
        //    }
        //}
        if (hp > 30)
        {
            timer += Time.deltaTime;
            if (timer > fireRate)
            {
                StartCoroutine(LaserAttack()); // 코루틴이 매번 시작만 되면 직전에 돌아가던 코루틴함수가 끝나지 않은 상태로 새로운 코루틴 함수가 시작될 수 있기 때문에 오류가 날 수 있다고 함. 코루틴이 끝나고 실행되게 하던지, 코루틴은 완전히 끝난 후에 다시 시작한다는 것을 입증하던지 해야할듯
                timer = 0.0f;
            }
        }
        else
        {
            switch (skill.skillname)
            {
                case "laserAttack":
                    timer += Time.deltaTime;
                    if (timer > fireRate)
                    {
                        StartCoroutine(LaserAttack());
                        timer = 0.0f;
                    }
                    break;
                case "levitiationAttack":
                    levitationAttack();
                    eightWayAttackCoolTime = Time.time + eightWayAttackCoolTime;
                    break;
                case "jumpAttack":
                    StartCoroutine(JumpAttack(target.position));
                    jumpAttackCoolTime = Time.time + jumpAttackCoolTime;
                    break;
            }
        }
    }

    IEnumerator JumpAttack(Vector3 targetPosition)
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
    }

    IEnumerator LaserAttack()
    {
        laserLine.enabled = true;
        laserLine.SetPosition(0, shotPos.position);
        RaycastHit hit;
        Vector3 startPosition = shotPos.position;

        if (Physics.Raycast(startPosition, transform.forward + (transform.up), out hit, laserRange)) 
        {
            laserLine.SetPosition(1, hit.point);
            if (hit.transform.tag == "Player")
                hit.transform.gameObject.GetComponent<Player>().TakeDamage(attackDamage);
        }    
        else
            laserLine.SetPosition(1, startPosition + (transform.forward + (transform.up * -1)) * laserRange); 

        yield return new WaitForSeconds(laserDuration);
        laserLine.enabled = false;
        canAttack = false;
    }

    void levitationAttack()
    {
        Collider player = Physics.OverlapSphere(transform.position, levitationAttackRange, playerLayer).FirstOrDefault();   
        if (player != null)
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;
            float angleBetween = Vector3.Angle(transform.forward, direction);
            if (angleBetween < levitationAttackAngle / 2)
            {
                CharacterController playerController = player.GetComponent<CharacterController>();
                if (playerController != null)
                {
                    //StartCoroutine(ApplyKnockback(playerController));
                    playerController.Move((playerController.transform.position + new Vector3(0,5,0)) * 0.1f);
                }
            }
        }
    }

    IEnumerator ApplyKnockback(CharacterController controller)
    {
        Vector3 knockbackForce = new Vector3(0, 5, 0); 
        float timer = 0;
        while (timer < 0.5f)
        {
            controller.Move(knockbackForce * Time.deltaTime); 
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            transform.LookAt(other.transform);
            bossState = BossState.Attack;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
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
            bossState = BossState.Die;
        }
    }
}

public class BossSkills
{
    public string skillname;
    float coolTime;
    float LastUsedTime;
    int prioty;

    public BossSkills (string skillname, float coolTime, int  prioty)
    {
        this.skillname = skillname;
        this.coolTime = coolTime;
        this.LastUsedTime = -coolTime;
        this.prioty = prioty;
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