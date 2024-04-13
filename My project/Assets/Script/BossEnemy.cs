using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum BossState { Idle, Die, Attack1, Attack2, MoveToPlayer}
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

    List<BossSkills> skills = new List<BossSkills>
    {
        new BossSkills("laserAttack", 1f, 1),
        new BossSkills("frontAttack", 5f, 2),
        new BossSkills("jumpAttack", 8f, 3)
    };

    // Start is called before the first frame update
    void Start()
    {
        bossState = BossState.Idle;
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
            case BossState.Idle:
                anim.SetBool("isIdle", true); // 이거는 idle로 상태가 바뀔 때 같이 바꿔주기. 여기에 지정해주면 idle일 동안 계속 돌아가서 비효율적이라고 함
                if (hp <= 0)
                {
                    bossState = BossState.Die;
                    break;
                }
                if (hp <= 30) // 굳이 페이즈1, 2일 필요X 공격 상태이고 피가 적을 때 스킬이 추가되는 형태가 되는게 좋을듯. 그리고 여기서도 쿨타임인지 확인하고, attack2에서도 쿨타임 확인할 필요 없을듯.
                {
                    if (CanExecuteJumpAttack() || CanExecuteEightWayAttack())
                    {
                        bossState = BossState.Attack2;
                        break;
                    }
                }
                if (canAttack) // 이거는 굳이 변수 사용안하고 그냥 충돌되면 공격되게 하는게 나을거 같다고 함.
                {
                    bossState = BossState.Attack1;
                    break;
                }
                else
                {
                    bossState = BossState.MoveToPlayer;
                    break;
                }

            case BossState.MoveToPlayer:
                Move(); // 이동 상태일 때도 다른 상태로 넘어갈 수 있는지 부터 체크하고 다른 조건이 안되면 Move함수가 실행되게 하는게 나을거 같다고 함.

                if (hp <= 0)
                {
                    bossState = BossState.Die;
                    break;
                }
                if (canAttack)
                {
                    bossState = BossState.Attack1;
                    break;
                }
                if (hp <= 30)
                {
                    if (CanExecuteJumpAttack() || CanExecuteEightWayAttack())
                    {
                        bossState = BossState.Attack2;
                        break;
                    }
                }
                break;

            case BossState.Attack1:
                timer += Time.deltaTime;
                if (timer > fireRate)
                {
                    StartCoroutine(LaserAttack()); // 코루틴이 매번 시작만 되면 직전에 돌아가던 코루틴함수가 끝나지 않은 상태로 새로운 코루틴 함수가 시작될 수 있기 때문에 오류가 날 수 있다고 함. 코루틴이 끝나고 실행되게 하던지, 코루틴은 완전히 끝난 후에 다시 시작한다는 것을 입증하던지 해야할듯
                    timer = 0.0f;
                }

                if (hp <= 0)
                {
                    bossState = BossState.Die;
                    break;
                }
                if (hp <= 30)
                {
                    if (CanExecuteJumpAttack() || CanExecuteEightWayAttack())
                    {
                        bossState = BossState.Attack2;
                        break;
                    }
                }
                if (!canAttack)
                {
                    bossState = BossState.MoveToPlayer;
                    break;
                }
                break;

            case BossState.Attack2:
                if (CanExecuteJumpAttack())
                {
                    StartCoroutine(JumpAttack(target.position));
                    jumpAttackCoolTime = Time.time + jumpAttackCoolTime;
                    break;
                }
                else if (CanExecuteEightWayAttack())
                {
                    StartCoroutine(EightWayAttack());
                    eightWayAttackCoolTime = Time.time + eightWayAttackCoolTime;
                    break;
                }
                if (hp <= 0)
                {
                    bossState = BossState.Die;
                    break;
                }
                if (!canAttack)
                {
                    bossState = BossState.MoveToPlayer;
                    break;
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

    IEnumerator EightWayAttack() // 8 방향 공격말고 다른 공격을 생각해야할듯
    {
        anim.SetTrigger("eightWayAttack");
        yield return new WaitForSeconds(1f);
        Vector3[] directions = {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right,
            (Vector3.forward + Vector3.right).normalized,
            (Vector3.forward + Vector3.left).normalized,
            (Vector3.back + Vector3.right).normalized,
            (Vector3.back + Vector3.left).normalized
        };

        foreach (Vector3 direction in directions)
        {
            GameObject sphere = Instantiate(bulletPrefab, eightWayShotPos.position, Quaternion.identity);
            Rigidbody rb = sphere.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(direction * 10f, ForceMode.VelocityChange);
            }
        }
    }

    bool CanExecuteJumpAttack()
    {
        return Time.time >= jumpAttackCoolTime;
    }

    bool CanExecuteEightWayAttack()
    {
        return Time.time >= eightWayAttackCoolTime;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            //shotPos.LookAt(other.transform); // 이걸 빼면 레이저가 제대로 안나감
            transform.LookAt(other.transform);
            canAttack = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            canAttack = false;
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
}

public class BossSkills
{
    string skillname;
    float coolTime;
    float currentCoolTime;
    int prioty;

    public BossSkills (string skillname, float coolTime, int  prioty)
    {
        this.skillname = skillname;
        this.coolTime = coolTime;
        this.currentCoolTime = coolTime;
        this.prioty = prioty;
    }

    public bool isCoolTimeEnd(float deltaTime)
    {
        currentCoolTime += deltaTime;
        if (currentCoolTime > coolTime)
        {
            currentCoolTime = 0;
            return true;
        }
        return false;
    }
}