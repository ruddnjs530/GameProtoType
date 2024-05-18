using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : Enemy
{
    int flyingPrice = 15;

    [SerializeField] Transform shotPos;
    [SerializeField] GameObject bulletPrefab; // 총알 프리팹
    float bulletSpeed = 10f; // 총알 발사 속도

    float fireRate = 1.0f;
    float timer = 0.0f;

    [SerializeField] Transform enemyBody;
    Vector3 targetPosition;
    private float timeBetweenMoves = 1f;
    private float timeSinceLastMove = 0f;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        if (enemyState == EnemyState.Idle || enemyState == EnemyState.SimpleMove)
        {
            timeSinceLastMove += Time.deltaTime;
            if (timeSinceLastMove >= timeBetweenMoves)
            {
                SetRandomTargetPosition();
                timeSinceLastMove = 0f;
            }
            enemyBody.position = Vector3.Lerp(enemyBody.position, targetPosition, Time.deltaTime * 0.3f);
        }
        else enemyBody.position = Vector3.Lerp(enemyBody.position, transform.position + new Vector3(0f, 4.25f, 0f), Time.deltaTime * 0.3f);
    }

    protected override void Attack()
    {
        base.Attack();

        if (agentTarget)
            LookAt(agentTarget);

        timer += Time.deltaTime;
        if (timer > fireRate)
        {
            Quaternion bulletRotation = Quaternion.Euler(0f, 0f, -45f);
            GameObject currentBullet = Instantiate(bulletPrefab, shotPos.position, bulletRotation);
            Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
            rb.AddForce(shotPos.forward * bulletSpeed, ForceMode.Impulse);
            timer = 0.0f;
        }
    }

    void SetRandomTargetPosition()
    {
        // 랜덤한 방향으로 이동할 목표 위치 설정
        float randomAngle = Random.Range(0f, 360f);
        targetPosition = enemyBody.position + Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward * 2f;
    }

    void LookAt(Transform target)
    {
        Vector3 direction = target.position - enemyBody.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction.normalized);
        enemyBody.rotation = Quaternion.Slerp(enemyBody.rotation, lookRotation, Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            agentTarget = other.transform;
            isSeePlayer = true;
            canAttack = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            canAttack = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isSeePlayer = false;
            agentTarget = null;
            canAttack = false;
        }
    }

    protected override void Die()
    {
        GameManager.Instance.IncreaseMoney(flyingPrice);
        base.Die();
    }
}
