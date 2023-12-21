using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    float destroyTime = 5f;
    float timer;

    int bulletDamage = 10;

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= destroyTime) Destroy(this.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(this.gameObject);
        if (collision.gameObject.tag == "Enemy")
        {
            ContactPoint cp = collision.contacts[0];
            collision.gameObject.GetComponent<Enemy>().TakeDamageAndInstantiateText(bulletDamage, cp.point.y + 2);
        }
        if (collision.gameObject.tag == "BossEnemy")
        {
            collision.gameObject.GetComponent<BossEnemy>().TakeDamageAndInstantiateText(bulletDamage);
        }
    }
}
