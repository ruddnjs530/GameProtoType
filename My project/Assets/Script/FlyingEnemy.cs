using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : Enemy
{
    private int flyingPrice = 15;

    [SerializeField] private Transform shotPos;
    [SerializeField] private GameObject bulletPrefab; // ÃÑ¾Ë ÇÁ¸®ÆÕ
    private float bulletSpeed = 10f; // ÃÑ¾Ë ¹ß»ç ¼Óµµ

    private float fireRate = 1.0f;
    private float timer = 0.0f;

    private Quaternion originalRotation;

    protected override void Start()
    {
        base.Start();

        originalRotation = enemyBody.rotation;
    }

    protected override void Update()
    {
        base.Update();

        if (enemyState != EnemyState.Attack && Mathf.Abs(Mathf.DeltaAngle(enemyBody.rotation.x, originalRotation.x)) > 0.1f)
        {
            Vector3 currentEulerAngles = enemyBody.rotation.eulerAngles;
            Vector3 originalEulerAngles = originalRotation.eulerAngles;

            float newX = Mathf.LerpAngle(currentEulerAngles.x, originalEulerAngles.x, 0.1f);

            Quaternion newRotation = Quaternion.Euler(newX, currentEulerAngles.y, currentEulerAngles.z);
            enemyBody.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * 0.3f);
        }
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

    void LookAt(Transform target)
    {
        Vector3 direction = target.position - enemyBody.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction.normalized);
        enemyBody.rotation = Quaternion.Slerp(enemyBody.rotation, lookRotation, Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Plyaer"))
        {
            agentTarget = other.transform;
            isSeePlayer = true;
            canAttack = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Plyaer"))
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
