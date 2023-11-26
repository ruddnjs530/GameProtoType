using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    float destroyTime =10f;
    float timer;

    int bulletDamage = 10;

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= destroyTime) Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag =="Enemy")
        {
            other.gameObject.GetComponent<Enemy>().TakeDamageAndInstantiateText(bulletDamage);
        }
        Destroy(this.gameObject);
    }
}
