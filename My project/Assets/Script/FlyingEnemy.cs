using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : Enemy
{
    int flyingPrice = 15;

    [SerializeField] Transform shotPos;
    [SerializeField] GameObject bulletPrefab; // ÃÑ¾Ë ÇÁ¸®ÆÕ
    float bulletSpeed = 10f; // ÃÑ¾Ë ¹ß»ç ¼Óµµ

    float fireRate = 1.0f;
    float timer = 0.0f;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void Attack()
    {
        base.Attack();
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            agentTarget = other.transform;
            isSeePlayer = true;
            canAttack = true;
            //Debug.Log("ÀÚ½Ä in");
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
            Debug.Log("ÀÚ½Ä out");
        }
    }

    protected override void Die()
    {
        GameManager.Instance.IncreaseMoney(flyingPrice);
        Destroy(gameObject);
    }
}
