using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurretState { Idle, Attack, Die }

public class TurretObject : MonoBehaviour
{
    //GameObjectPriorityQueue surroundingsObj;
    TurretState turretState;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePos;
    float rateOfFire = 1f;
    float currentRateOfFire = 1f;

    float hp = 100;

    Quaternion originalRotation;

    GameObject attackTarget;

    // Start is called before the first frame update
    void Start()
    {
        turretState = TurretState.Idle;
        originalRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        switch (turretState)
        {
            case TurretState.Idle:
                transform.Rotate(new Vector3(0, 45, 0) * Time.deltaTime);
                if (hp <= 0)
                {
                    turretState = TurretState.Die;
                    break;
                }
                //else if (priorityQueue.Count != 0)
                    //turretState = TurretState.Attack;
                break;

            case TurretState.Attack:
                if (attackTarget == null || attackTarget.GetComponent<Enemy>().hp < 0)
                {
                    //priorityQueueDequeue(); // 피가 0이하이면 surroundingObj에서 target을 삭제
                    //if (priorityQueue.Count == 0)
                    //{
                    //    transform.rotation = originalRotation;
                    //    turretState = TurretState.Idle;
                    //    break;
                    //}
                    //target = priorityQueueMinObject();
                }

                Attack(attackTarget);
                break;
            case TurretState.Die:
                Die();
                break;
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (turretState == TurretState.Attack && attackTarget == null)
        {
            if (other.gameObject.tag == "Enemy") attackTarget = NearestObj(other.gameObject);
        }    
    }

    private void OnTriggerExit(Collider other)
    {
        //if (other.gameObject.tag == "Enemy") 
    }

    GameObject NearestObj(GameObject obj)
    {
        List<GameObject> objs = new List<GameObject>();
        objs.Add(obj);

        GameObject nearestObj = objs[0];
        foreach (GameObject objInList in objs)
        {
            if (DistanceWithEnemy(nearestObj) > DistanceWithEnemy(objInList))
            {
                nearestObj = objInList;
            }
        }
       
        return nearestObj;
    }

    void Attack(GameObject target)
    {
        if (target == null) return;
        Vector3 direction = target.transform.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        currentRateOfFire += Time.deltaTime;

        if (currentRateOfFire >= rateOfFire)
        {
            GameObject currentBullet = Instantiate(bulletPrefab, firePos.position, firePos.rotation);
            Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
            rb.AddForce(firePos.forward * 2f, ForceMode.Impulse);
            currentRateOfFire = 0;
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    float DistanceWithEnemy(GameObject enemy)
    {
        float distance = Vector3.Distance(enemy.transform.position, this.transform.position);
        return distance;
    }
}