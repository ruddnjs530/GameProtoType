using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : Enemy
{
    [Header("Flying Enemy Settings")]
    [SerializeField] private int rewardMoney = 15; // 처치 시 보상금
    [SerializeField] private float bulletSpeed = 10f; // 총알 속도
    [SerializeField] private float rotationThreshold = 0.1f; // 회전 보정 임계값
    [SerializeField] private float rotationSmoothTime = 0.01f; // 회전 부드러움 정도

    [Header("References")]
    [SerializeField] private Transform shotPos; // 발사 위치
    [SerializeField] private GameObject bulletPrefab; // 총알 프리팹

    private Quaternion originalRotation; // 초기 회전값

    protected override void Start()
    {
        base.Start();

        originalRotation = enemyBody.rotation;
    }

    protected override void Update()
    {
        base.Update();

        // 원래 방향으로 복귀 로직
        if (enemyState != EnemyState.Attack && Mathf.Abs(Mathf.DeltaAngle(enemyBody.rotation.eulerAngles.y, originalRotation.y)) > rotationThreshold)
        {
            Vector3 currentEulerAngles = enemyBody.rotation.eulerAngles;
            Vector3 originalEulerAngles = originalRotation.eulerAngles;

            float newY = Mathf.LerpAngle(currentEulerAngles.y, originalEulerAngles.y, rotationThreshold);

            Quaternion newRotation = Quaternion.Euler(currentEulerAngles.x, newY, currentEulerAngles.z);
            enemyBody.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * rotationSmoothTime);
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
        Quaternion bulletRotation = Quaternion.Euler(0f, 0f, -45f); // 탄환 회전값 매직넘버 (-45도) 유지 (추후 수정 필요시 변수화)
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
        GameManager.Instance.IncreaseMoney(rewardMoney);
        base.Die();
    }
}
