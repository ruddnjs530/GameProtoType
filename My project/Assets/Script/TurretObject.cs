using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurretState { Idle, Attack, Die }

public class TurretObject : MonoBehaviour
{
    //GameObjectPriorityQueue surroundingsObj;
    public TurretState turretState;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePos;
    float rateOfFire = 1f;
    float currentRateOfFire = 1f;

    float hp = 100;

    Quaternion correctRotation;

    GameObject attackTarget;

    List<Enemy> priorityQueue = new List<Enemy>();

    public delegate void AttackStateHandler();
    public event AttackStateHandler OnIsAttack;

    // Start is called before the first frame update
    void Start()
    {
        turretState = TurretState.Idle;
        correctRotation = new Quaternion(transform.rotation.x, 0, transform.rotation.z, transform.rotation.w);

        attackTarget = null;
        var drone = GetComponentInParent<DroneObject>();
        if (drone != null) drone.OnPlayerTooFar += HandleDroneTooFar;

        Invoke("FindEnemys", 0.5f);
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
                else if (priorityQueue.Count > 0)
                {
                    turretState = TurretState.Attack;
                    break;
                }
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

    private void OnTriggerStay(Collider other) 
    {
        if (other.gameObject.tag == "Enemy")
        {
            if (turretState == TurretState.Idle)
            {
                priorityQueueEnqueue(other.gameObject.GetComponent<Enemy>());
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
        if (other.gameObject.tag == "Enemy")
        {
            if (other.gameObject == attackTarget)
            {
                attackTarget = null;
                transform.rotation = Quaternion.Slerp(transform.rotation, correctRotation, 0.1f);
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

    GameObject NearestObj(GameObject obj)
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

    float DistanceWithEnemy2(GameObject enemy)
    {
        float distance = Vector3.Distance(enemy.transform.position, this.transform.position);
        return distance;
    }

    private float DistanceWithEnemy(Enemy enemy)
    {
        float distance = Vector3.Distance(enemy.transform.position, this.transform.position);
        return distance;
    }

    private void Attack(GameObject target)
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

    private void Die()
    {
        Destroy(gameObject);
    }

    private void priorityQueueEnqueue(Enemy obj)
    {
        if (!priorityQueue.Contains(obj))
        {
            priorityQueue.Add(obj);
        }

        int currentIndex = priorityQueue.Count - 1;

        while (currentIndex > 0)
        {
            int parentIndex = (currentIndex - 1) / 2;
            if (DistanceWithEnemy(priorityQueue[parentIndex]) > DistanceWithEnemy(priorityQueue[currentIndex]))
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
        if (priorityQueue.Count == 0) return null;

        GameObject target = priorityQueue[0].gameObject;
        priorityQueue[0] = priorityQueue[priorityQueue.Count - 1];
        priorityQueue.RemoveAt(priorityQueue.Count - 1);

        if (priorityQueue.Count > 0) Heapify(0);
        return target;

    }

    private void Heapify(int currentIndex)
    {
        int lastIndex = priorityQueue.Count - 1;
        while (true)
        {
            int leftIndex = 2 * currentIndex + 1;
            int rightIndex = 2 * currentIndex + 2;

            if (leftIndex <= lastIndex && DistanceWithEnemy(priorityQueue[leftIndex]) < DistanceWithEnemy(priorityQueue[currentIndex]))
            {
                currentIndex = leftIndex;
                Swap(currentIndex, leftIndex);
            }
            else if (rightIndex <= lastIndex && DistanceWithEnemy(priorityQueue[rightIndex]) < DistanceWithEnemy(priorityQueue[currentIndex]))
            {
                currentIndex = rightIndex;
                Swap(currentIndex, rightIndex);
            }
            else break;
        }
    }

    private void Swap(int index1, int index2)
    {
        Enemy temp = priorityQueue[index1];
        priorityQueue[index1] = priorityQueue[index2];
        priorityQueue[index2] = temp;
    }

    private Enemy priorityQueueMinObject()
    {
        if (priorityQueue.Count == 0)
        {
            return null;
        }
        return priorityQueue[0];
    }

    private void RemovepriorityQueueElements(Enemy targetEnemy)
    {
        for (int i = 0; i < priorityQueue.Count; i++)
        {
            if (priorityQueue[i] == targetEnemy)
            {
                priorityQueue.RemoveAt(i);
                RebuildHeap();
                break;
            }
        }
    }

    private void RebuildHeap()
    {
        int lastParentIndex = (priorityQueue.Count - 2) / 2;
        for (int i = lastParentIndex; i >= 0; i--)
        {
            Heapify(i);
        }
    }

    private void HandleDroneTooFar()
    {
        attackTarget = null;
        transform.rotation = Quaternion.Slerp(transform.rotation, correctRotation, 0.1f);
        turretState = TurretState.Idle;
    }

    void FindEnemys()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            enemy.OnEnemyDied += RemovepriorityQueueElements;
            Debug.Log("hi2");
        }
    }
}