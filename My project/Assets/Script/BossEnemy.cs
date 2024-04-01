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
    float laserRange = 50f;
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
                anim.SetBool("isIdle", true);
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
                if (canAttack)
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
                 Move();

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
                    StartCoroutine(LaserAttack());
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

        if (Physics.Raycast(startPosition, transform.forward + (transform.up * -1), out hit, laserRange)) 
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

    IEnumerator EightWayAttack()
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


//void EightWayAttack()
//{
//    anim.SetTrigger("eightWayAttack");

//    Vector3[] directions = {
//            Vector3.forward,
//            Vector3.back,
//            Vector3.left,
//            Vector3.right,
//            (Vector3.forward + Vector3.right).normalized,
//            (Vector3.forward + Vector3.left).normalized,
//            (Vector3.back + Vector3.right).normalized,
//            (Vector3.back + Vector3.left).normalized
//        };

//    foreach (Vector3 direction in directions)
//    {
//        GameObject sphere = Instantiate(bulletPrefab, transform.position, Quaternion.identity); // 여기 transform.position말고 bulletShotPos로 해서 생성해야할듯
//        Rigidbody rb = sphere.GetComponent<Rigidbody>();
//        if (rb != null)
//        {
//            rb.AddForce(direction * 10f, ForceMode.VelocityChange);
//        }
//    }
//}


//IEnumerator EeightWayAttack()
//{
//    Vector3[] directions = { // 이걸 굳이 매번 받아야할까? 보스가 회전하니까 매번 받아야하나?
//        Vector3.forward,
//        Vector3.back,
//        Vector3.left,
//        Vector3.right,
//        (Vector3.forward + Vector3.right).normalized,
//        (Vector3.forward + Vector3.left).normalized,
//        (Vector3.back + Vector3.right).normalized,
//        (Vector3.back + Vector3.left).normalized
//    };

//    foreach (Vector3 direction in directions)
//    {
//        GameObject sphere = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
//        Rigidbody rb = sphere.GetComponent<Rigidbody>();
//        if (rb != null)
//        {
//            rb.AddForce(direction * 10f, ForceMode.VelocityChange);
//        }
//    }
//    yield return new WaitForSeconds(3f);
//    canAttack = false;
//}


//void JumpAttack2() // 위에꺼 처럼 하고 네비메쉬로 이동하는걸로 수정해봐야할듯
//{
//    //float lerpRatio = timer / lerpTime;
//    //Vector3 positionOffset = HeightCurve.Evaluate(lerpRatio) * lerpOffset;

//    //transform.position = Vector3.Lerp(transform.position, target.position, lerpRatio) + positionOffset;

//    for (float time = 0; time < 1; time += Time.deltaTime * jumpSpeed)
//    {
//        this.transform.position = Vector3.Lerp(this.transform.position, target.position, time)
//            + Vector3.up * HeightCurve.Evaluate(time);
//    }
//    useTime = Time.time;
//}

//IEnumerator JumpAttack(Vector3 targetPostion)
//{
//    for (float time = 0; time < 1; time += Time.deltaTime * jumpSpeed)
//    {
//        this.transform.position = Vector3.Lerp(this.transform.position, targetPostion, time)
//            + Vector3.up * HeightCurve.Evaluate(time);
//        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position),
//        //    time);

//        //transform.position =new Vector3(target.position.x, HeightCurve.Evaluate(time), target.position.z);

//        //this.transform.position = new Vector3(Vector3.Lerp(this.transform.position, target.position, time).x,
//        //Vector3.Lerp(this.transform.position, target.position, time).y, Vector3.Lerp(this.transform.position, target.position, time).z) 
//        //    +Vector3.up * HeightCurve.Evaluate(time);

//        yield return null;
//    }

//    useTime = Time.time;

//    float distance = Vector3.Distance(this.transform.position, target.position);
//    //if (distance <= 2f) target.GetComponent<Player>().TakeDamage(jumpDamage);

//    //if (NavMesh.SamplePosition(target.position, out NavMeshHit hit, 1f, this.agent.areaMask))
//    //{
//    //    agent.Warp(hit.position);
//    //}
//}