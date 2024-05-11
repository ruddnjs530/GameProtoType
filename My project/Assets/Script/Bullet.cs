using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    float destroyTime = 5f;
    float timer;

    ParticleSystem hitParticle;
    Rigidbody bulletRigidbody;

    private void Start()
    {
        hitParticle = GetComponentInChildren<ParticleSystem>();
        bulletRigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        bulletRigidbody.velocity = transform.forward * 5f;
        timer += Time.deltaTime;
        if (timer >= destroyTime) Destroy(this.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            ContactPoint cp = collision.contacts[0];
            collision.gameObject.GetComponent<Enemy>().TakeDamageAndInstantiateText(GameManager.Instance.bulletDamage, cp.point.y + 2);
            //Debug.Log("hi");
            hitParticle.Play();
        }
        if (collision.gameObject.tag == "BossEnemy")
        {
            collision.gameObject.GetComponent<BossEnemy>().TakeDamageAndInstantiateText(GameManager.Instance.bulletDamage);
            hitParticle.Play();
        }

        Destroy(this.gameObject, 0.2f);
    }
}
