using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    float destroyTime = 5f;
    float timer;

    int bulletDamage = 11;

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= destroyTime) Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<Player>().TakeDamage(bulletDamage);
        }
    }
}
