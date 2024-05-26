using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public static event System.Action<Vector3> OnBulletHit;
   private  float destroyTime = 5f;
    private float timer;

    //[SerializeField] ParticleSystem hitParticle;

    // Update is called once per frame
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= destroyTime) Destroy(this.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            ContactPoint cp = collision.contacts[0];
            Vector3 bulletDirection = GetComponent<Rigidbody>().velocity.normalized;
            collision.gameObject.GetComponent<Enemy>().TakeDamage(GameManager.Instance.bulletDamage);
            collision.gameObject.GetComponent<Enemy>().LookAtDirection(-bulletDirection);

            if (OnBulletHit == null) Debug.Log("null");

            OnBulletHit?.Invoke(cp.point);
        }

        if (collision.gameObject.tag == "BossEnemy")
        {
            ContactPoint cp = collision.contacts[0];
            collision.gameObject.GetComponent<BossEnemy>().TakeDamage(GameManager.Instance.bulletDamage);

            if (OnBulletHit == null) Debug.Log("null");
            OnBulletHit?.Invoke(cp.point);
        }



        Destroy(this.gameObject);
    }
}
