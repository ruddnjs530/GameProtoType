using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float destroyTime;
    float timer;

    int bulletDamage = 10;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= destroyTime) Destroy(this.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag =="Enemy")
        {
            collision.gameObject.GetComponent<Enemy>().TakeDamage(bulletDamage);
        }
        Destroy(this.gameObject);
    }

    void TakeDamage(int damage)
    {

    }
}
