using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurretState { Idle, Attack, Die }

public class TurretObject : MonoBehaviour
{
    public TurretState turretState;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePos;
    private float rateOfFire = 1f;
    private float currentRateOfFire = 1f;

    private float hp = 100;

    private Quaternion originalRotation;

    private GameObject attackTarget;

    private int queueMaxSize = 50;
    private EnemyData[] priorityQueue;
    private int currentQueueCount = 0;

    public delegate void AttackStateHandler();
    public event AttackStateHandler OnIsAttack;

    // Start is called before the first frame update
    private void Start()
    {
        priorityQueue = new EnemyData[queueMaxSize];

        turretState = TurretState.Idle;
        originalRotation = transform.rotation;

        attackTarget = null;
        var drone = GetComponentInParent<DroneObject>();
        if (drone != null) drone.OnPlayerTooFar += HandleDroneTooFar;
    }

    // Update is called once per frame
    private void Update()
    {
        switch (turretState)
        {
            case TurretState.Idle:
                if (hp <= 0)
                {
                    turretState = TurretState.Die;
                    break;
                }
                else if (currentQueueCount > 0)
                {
                    turretState = TurretState.Attack;
                    break;
                }

                if (Mathf.Abs(Mathf.DeltaAngle(transform.rotation.x, originalRotation.x)) > 0.1f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, Time.deltaTime);
                }

                transform.Rotate(new Vector3(0, 45, 0) * Time.deltaTime);
                break;

            case TurretState.Attack:
                if (attackTarget == null) attackTarget = priorityQueueDequeue();
                else
                {
                    Attack(attackTarget);
                    OnIsAttack?.Invoke();
                }
                //Attack(attackTarget);
                //OnIsAttack?.Invoke();
                break;

            case TurretState.Die:
                Die();
                break;
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Enemy"))
        {
            if (turretState == TurretState.Idle)
            {
                priorityQueueEnqueue(new EnemyData(other.transform, other.GetComponent<Enemy>().EnemyID));
            }

            //if (turretState == TurretState.Idle)
            //{
            //    turretState = TurretState.Attack;
            //}
            //else if (turretState == TurretState.Attack && attackTarget == null)
            //{
            //    attackTarget = NearestObj(other.gameObject);
            //}
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (other.gameObject == attackTarget)
            {
                attackTarget = null;
                turretState = TurretState.Idle;
                return;
            }

            RemovepriorityQueueElements(other.gameObject.GetComponent<Enemy>());
        }
        //if (other.gameObject.tag == "Enemy" && other.gameObject == attackTarget)
        //{
        //    attackTarget = null;
        //    turretState = TurretState.Idle;
        //    Debug.Log("enemy out");
        //    return;
        //}
    }

    private GameObject NearestObj(GameObject obj)
    {
        List<GameObject> objs = new List<GameObject>();
        objs.Add(obj);

        GameObject nearestObj = objs[0];
        foreach (GameObject objInList in objs)
        {
            if (DistanceWithEnemy2(nearestObj) > DistanceWithEnemy2(objInList))
            {
                nearestObj = objInList;
            }
        }

        return nearestObj;
    }

    private float DistanceWithEnemy2(GameObject enemy)
    {
        float distance = Vector3.Distance(enemy.transform.position, this.transform.position);
        return distance;
    }

    private void Attack(GameObject target)
    {
        if (target == null) return;
        Enemy enemy = target.GetComponent<Enemy>();
        Vector3 direction = enemy.EnemyBody.transform.position - transform.position;
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

    private void Die()
    {
        Destroy(gameObject);
    }

    private void priorityQueueEnqueue(EnemyData enemyData)
    {
        for (int i = 0; i < currentQueueCount; i++)
        {
            if (priorityQueue[i].EnemyDataID == enemyData.EnemyDataID) return;
        }

        if (currentQueueCount >= queueMaxSize)
        {
            queueMaxSize *= 2;
            EnemyData[] newQueue = new EnemyData[queueMaxSize];
            System.Array.Copy(priorityQueue, newQueue, currentQueueCount);
            priorityQueue = newQueue;
        }

        priorityQueue[currentQueueCount] = enemyData;
        currentQueueCount++;

        int currentIndex = currentQueueCount - 1;

        while (currentIndex > 0)
        {
            int parentIndex = (currentIndex - 1) / 2;
            if (priorityQueue[parentIndex].GetDistance(this.transform.position) > priorityQueue[currentIndex].GetDistance(this.transform.position))
            {
                Swap(currentIndex, parentIndex);
                currentIndex = parentIndex;
            }
            else
            {
                break;
            }
        }
    }

    private GameObject priorityQueueDequeue()
    {
        if (currentQueueCount == 0) return null;

        GameObject target = priorityQueue[0].Transform.gameObject;

        priorityQueue[0] = priorityQueue[currentQueueCount - 1];
        currentQueueCount--;

        if (currentQueueCount > 0) Heapify(0);

        if (target == null && currentQueueCount > 0)
        {
            return priorityQueueDequeue();
        }

        return target;

    }

    private void Heapify(int currentIndex)
    {
        int lastIndex = currentQueueCount - 1;
        while (true)
        {
            int leftIndex = 2 * currentIndex + 1;
            int rightIndex = 2 * currentIndex + 2;

            if (leftIndex <= lastIndex && priorityQueue[leftIndex].GetDistance(this.transform.position) < priorityQueue[currentIndex].GetDistance(this.transform.position))
            {
                currentIndex = leftIndex;
                Swap(currentIndex, leftIndex);
            }
            else if (rightIndex <= lastIndex && priorityQueue[rightIndex].GetDistance(this.transform.position) < priorityQueue[currentIndex].GetDistance(this.transform.position))
            {
                currentIndex = rightIndex;
                Swap(currentIndex, rightIndex);
            }
            else break;
        }
    }

    private void Swap(int index1, int index2)
    {
        EnemyData temp = priorityQueue[index1];
        priorityQueue[index1] = priorityQueue[index2];
        priorityQueue[index2] = temp;
    }

    private void RemovepriorityQueueElements(Enemy targetEnemy)
    {
        for (int i = 0; i < currentQueueCount -1; i++)
        {
            if (priorityQueue[i].EnemyDataID == targetEnemy.EnemyID)
            {
                priorityQueue[i] = priorityQueue[currentQueueCount - 1];
                currentQueueCount--;
                RebuildHeap();
            }
        }

        if (attackTarget != null && attackTarget == targetEnemy.gameObject)
        {
            attackTarget = null;
            turretState = TurretState.Idle;
        }
    }

    private void RebuildHeap()
    {
        int lastParentIndex = (currentQueueCount - 2) / 2;
        for (int i = lastParentIndex; i >= 0; i--)
        {
            Heapify(i);
        }
    }

    private void HandleDroneTooFar()
    {
        attackTarget = null;
        turretState = TurretState.Idle;
    }
}