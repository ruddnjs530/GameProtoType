using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossState { Move, Die, Attack}
public class BossEnemy : MonoBehaviour
{
    BossState bossState;
    Transform target;
    bool canAttack = false;

    [SerializeField] Transform shotPos;
    LineRenderer laserLine;
    float laserRange = 50f;
    float laserDuration = 0.5f;
    float fireRate = 1.0f;

    float attackDamage = 10.0f;

    float timer;

    // Start is called before the first frame update
    void Start()
    {
        bossState = BossState.Move;
        laserLine = GetComponent<LineRenderer>();

        timer = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        switch (bossState)
        {
            case BossState.Move:
                if (canAttack)
                    bossState = BossState.Attack;
                Move();
                break;

            case BossState.Die:
                Destroy(this.gameObject);
                break;

            case BossState.Attack:
                timer += Time.deltaTime;
                if (timer > fireRate)
                {
                    timer = 0;
                    StartCoroutine(Attack());
                }
                break;
        }
    }


    IEnumerator Attack()
    {
        timer += Time.deltaTime;
        laserLine.enabled = true;
        laserLine.SetPosition(0, shotPos.position);
        RaycastHit hit;
        Vector3 startPosition = shotPos.position;

        if (Physics.Raycast(startPosition, transform.forward, out hit, laserRange))
        {
            laserLine.SetPosition(1, hit.point);
            Debug.Log("hit something");
            if (hit.transform.tag == "Player")
                hit.transform.gameObject.GetComponent<Player>().TakeDamage(attackDamage);
        }    
        else
            laserLine.SetPosition(1, startPosition + (transform.forward * laserRange));

        yield return new WaitForSeconds(laserDuration);
        laserLine.enabled = false;
        canAttack = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player" && !canAttack)
        {
            target = other.transform;
            transform.LookAt(target);
            canAttack = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            canAttack = false;
            target = null;
        }
    }

    private void Move()
    {
        
    }
}
