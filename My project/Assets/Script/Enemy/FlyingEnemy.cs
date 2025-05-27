using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : Enemy
{
    private int flyingPrice = 15;

    [SerializeField] private Transform shotPos;
    [SerializeField] private GameObject bulletPrefab;
    private float bulletSpeed = 10f;

    private Quaternion originalRotation;

    protected override void Start()
    {
        base.Start();

        originalRotation = enemyBody.rotation;
    }

    protected override void Update()
    {
        base.Update();

        if (enemyState != EnemyState.Attack && Mathf.Abs(Mathf.DeltaAngle(enemyBody.rotation.eulerAngles.y, originalRotation.y)) > 0.1f)
        {
            Vector3 currentEulerAngles = enemyBody.rotation.eulerAngles;
            Vector3 originalEulerAngles = originalRotation.eulerAngles;

            float newY = Mathf.LerpAngle(currentEulerAngles.y, originalEulerAngles.y, 0.1f);

            Quaternion newRotation = Quaternion.Euler(currentEulerAngles.x, newY, currentEulerAngles.z);
            enemyBody.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * 0.01f);
        }
    }

    protected override void Attack()
    {
        base.Attack();

        if (!agentTarget) return;

        LookAt(agentTarget);

        if (attackTimer > attackRate)
        {
            attackTimer = 0f;
            //animatorStateInfo = anim.GetCurrentAnimatorStateInfo(0);
            //if (animatorStateInfo.IsName("Attack01") || animatorStateInfo.normalizedTime > 0.95f)
            //{
            //    Quaternion bulletRotation = Quaternion.Euler(0f, 0f, -45f);
            //    GameObject currentBullet = Instantiate(bulletPrefab, shotPos.position, bulletRotation);
            //    Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
            //    rb.AddForce(shotPos.forward * bulletSpeed, ForceMode.Impulse);
            //    attackTimer = 0f;
            //}
        }
        attackTimer += Time.deltaTime;
    }

    private void Shoot() // animation event
    {
        if (!agentTarget && !base.canAttack) return;
        Quaternion bulletRotation = Quaternion.Euler(0f, 0f, -45f);
        GameObject currentBullet = Instantiate(bulletPrefab, shotPos.position, bulletRotation);
        Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
        rb.AddForce(shotPos.forward * bulletSpeed, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            agentTarget = other.transform;
            isSeePlayer = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isSeePlayer = false;
            agentTarget = null;
        }
    }

    protected override void Die()
    {
        GameManager.Instance.IncreaseMoney(flyingPrice);
        base.Die();
    }
}
