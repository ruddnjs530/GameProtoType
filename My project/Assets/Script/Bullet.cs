using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //float destroyTime =10f;
    //float timer;

    int bulletDamage = 10;

    // Update is called once per frame
    void Update()
    {
        //timer += Time.deltaTime;
        //if (timer >= destroyTime) Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag =="Damage")
        {
            Debug.Log("hi");
            //other.gameObject.GetComponent<Enemy>().TakeDamageAndInstantiateText(bulletDamage);
            //other.gameObject.GetComponent<Enemy1>().TakeDamageAndInstantiateText(bulletDamage);
            other.gameObject.GetComponent<BossEnemy>().TakeDamageAndInstantiateText(bulletDamage);
        }
        Destroy(this.gameObject);
    }
}
