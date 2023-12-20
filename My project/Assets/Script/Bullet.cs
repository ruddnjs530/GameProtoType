using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    float destroyTime = 10f;
    float timer;

    int bulletDamage = 10;

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= destroyTime) Destroy(this.gameObject);
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.gameObject.tag == "Enemy") // �Ѵ� enemy �±׷� ���� �� enemy���� bossEnemy���� ã�Ƽ� �����Ű�� �� �������� �������°� ���⵵ �ϰ� �ϴ� �̰ɷ� ��. 12.16
    //    {
    //        other.gameObject.GetComponent<Enemy>().TakeDamageAndInstantiateText(bulletDamage);
    //        Debug.Log("hi");
    //    }
    //    if (other.gameObject.tag == "BossEnemy")
    //    {
    //        other.gameObject.GetComponent<BossEnemy>().TakeDamageAndInstantiateText(bulletDamage);
    //    }
    //    Destroy(this.gameObject);
    //}

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(this.gameObject);
        if (collision.gameObject.tag == "Enemy")
        {
            ContactPoint cp = collision.contacts[0];
            collision.gameObject.GetComponent<Enemy>().TakeDamageAndInstantiateText(bulletDamage, cp.point.y + 2);
            Debug.Log("hi");
        }
    }
}
