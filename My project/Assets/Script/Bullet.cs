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
    //    if (other.gameObject.tag == "Enemy") // 둘다 enemy 태그로 했을 때 enemy인지 bossEnemy인지 찾아서 적용시키면 더 좋겠지만 생각나는게 없기도 하고 일단 이걸로 함. 12.16
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
