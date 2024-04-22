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

    List<GameObject> priorityQueue = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        turretState = TurretState.Idle;
        originalRotation = transform.rotation;
        //surroundingsObj = new GameObjectPriorityQueue();
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
                break;

            case TurretState.Attack:
                Attack(attackTarget);
                break;
            case TurretState.Die:
                Die();
                break;
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.tag == "Enemy")
        {
            if (turretState == TurretState.Idle)
            {
                turretState = TurretState.Attack;
            }
            else if (turretState == TurretState.Attack && attackTarget == null)
            {
                attackTarget = NearestObj(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Enemy" && other.gameObject == attackTarget)
        {
            attackTarget = null;
            turretState = TurretState.Idle;
            return;
        }
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
    float DistanceWithEnemy(GameObject enemy)
    {
        float distance = Vector3.Distance(enemy.transform.position, this.transform.position);
        return distance;
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

    void priorityQueueEnqueue(GameObject obj)
    {
        if (!priorityQueue.Contains(obj))
        {
            priorityQueue.Add(obj);
        }

        int currentIndex = priorityQueue.Count - 1;

        while (currentIndex > 0)
        {
            int parentNode = (currentIndex - 1) / 2;

            while (priorityQueue[parentNode] == null)
            {
                if (currentIndex == priorityQueue.Count - 1 && currentIndex % 2 != 0)     
                {
                    Swap(priorityQueue[parentNode], priorityQueue[currentIndex]);
                    currentIndex = parentNode;
                    priorityQueue.RemoveAt(priorityQueue.Count - 1);
                }

                int leftChild = parentNode * 2 + 1;
                int rightChild = parentNode * 2 + 2;

                if (leftChild != currentIndex)
                {
                    if (DistanceWithEnemy(priorityQueue[leftChild]) < DistanceWithEnemy(priorityQueue[currentIndex]))
                    {
                        Swap(priorityQueue[parentNode], priorityQueue[leftChild]);
                        RemoveNull(leftChild);
                        Heapify(leftChild);
                    }
                    else
                    {
                        Swap(priorityQueue[parentNode], priorityQueue[currentIndex]);
                        RemoveNull(currentIndex);
                        Heapify(currentIndex);
                        currentIndex = parentNode;
                    }
                }
                else
                {
                    if (DistanceWithEnemy(priorityQueue[rightChild]) < DistanceWithEnemy(priorityQueue[currentIndex]))
                    {
                        Swap(priorityQueue[parentNode], priorityQueue[rightChild]);
                        RemoveNull(rightChild);
                        Heapify(rightChild);
                    }
                    else
                    {
                        Swap(priorityQueue[parentNode], priorityQueue[currentIndex]);
                        RemoveNull(currentIndex);
                        Heapify(currentIndex);
                        currentIndex = parentNode;
                    }
                }

                parentNode = (currentIndex - 1) / 2;
            }

            if (DistanceWithEnemy(priorityQueue[parentNode]) < DistanceWithEnemy(priorityQueue[currentIndex])) break;

            Swap(priorityQueue[parentNode], priorityQueue[currentIndex]);

            currentIndex = parentNode;
        }
    }


    void priorityQueueDequeue()
    {
        if (priorityQueue.Count - 1 < 0) return;

        priorityQueue[0] = priorityQueue[priorityQueue.Count - 1];
        priorityQueue.RemoveAt(priorityQueue.Count - 1);

        int currentIndex = 0;
        Heapify(currentIndex);

    }

    void RemoveNull(int index)
    {
        Swap(priorityQueue[index], priorityQueue[priorityQueue.Count - 1]);
        priorityQueue.RemoveAt(priorityQueue.Count - 1);
    }

    //void RemoveIntermediateElement(int index)
    //{
    //    Swap()
    //}

    void Heapify(int currentIndex)
    {
        int lastIndex = priorityQueue.Count - 1;
        while (true)
        {
            int leftIndex = 2 * currentIndex + 1;
            int rightIndex = 2 * currentIndex + 2;

            if (priorityQueue[leftIndex] == null)
            {
                RemoveNull(leftIndex);
            }
            if (priorityQueue[rightIndex] == null)
            {
                RemoveNull(rightIndex);
            }

            if (leftIndex <= lastIndex && DistanceWithEnemy(priorityQueue[leftIndex]) < DistanceWithEnemy(priorityQueue[currentIndex]))
            {
                currentIndex = leftIndex;
                Swap(priorityQueue[leftIndex], priorityQueue[currentIndex]);
            }
            else if (rightIndex <= lastIndex && DistanceWithEnemy(priorityQueue[rightIndex]) < DistanceWithEnemy(priorityQueue[currentIndex]))
            {
                currentIndex = rightIndex;
                Swap(priorityQueue[rightIndex], priorityQueue[currentIndex]);
            }
            else break;
        }
    }

    void Swap(GameObject obj1, GameObject obj2)
    {
        GameObject temp = obj1;
        obj1 = obj2;
        obj2 = temp;
    }

    public GameObject priorityQueueMinObject()
    {
        if (priorityQueue.Count == 0)
        {
            return null;
        }
        return priorityQueue[0];
    }
}