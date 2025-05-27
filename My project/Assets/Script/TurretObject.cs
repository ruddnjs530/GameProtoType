using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurretState { Idle, Attack}

public class TurretObject : MonoBehaviour
{
    public TurretState turretState;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePos;
    private float rateOfFire = 1f;
    private float currentRateOfFire = 1f;

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

        originalRotation = transform.rotation;
    }

    // Update is called once per frame
    private void Update()
    {
        switch (turretState)
        {
            case TurretState.Idle:
                if (currentQueueCount > 0)
                {
                    turretState = TurretState.Attack;
                    break;
                }
                SmoothRotateToZeroXZ();
                RotateAroundYAxis();
                //float newYRotation = Mathf.LerpAngle(transform.rotation.eulerAngles.y, originalRotation.eulerAngles.y, 100 * Time.deltaTime);
                //transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, newYRotation, transform.rotation.eulerAngles.z);

                //if (Quaternion.Angle(transform.rotation, Quaternion.Euler(0, newYRotation, 0)) < 0.1f)
                //{
                //    transform.Rotate(0, 50f * Time.deltaTime, 0);
                //    Debug.Log("조정중");    
                //}

                break;

            case TurretState.Attack:
                if (currentQueueCount == 0 && attackTarget == null)
                {
                    Debug.Log("Queue count 0");
                    turretState = TurretState.Idle;
                    break;
                }


                if (attackTarget == null) attackTarget = PriorityQueueDequeue();

                if (attackTarget != null)
                {
                    Attack(attackTarget);
                    OnIsAttack?.Invoke();
                }
                //Attack(attackTarget);
                //OnIsAttack?.Invoke();
                break;
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Enemy"))
        {
            if (turretState == TurretState.Idle)
            {
                Debug.Log("Enemy in area");
                PriorityQueueEnqueue(new EnemyData(other.transform, other.GetComponent<Enemy>().EnemyID));
            }

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
            Debug.Log("out");
            RemovePriorityQueueElements(other.gameObject.GetComponent<Enemy>());
        }
    }

    private void Attack(GameObject target)
    {
        if (target == null) return;
        if (target.CompareTag("BossEnemy"))
        {
            Vector3 direction = target.transform.position - transform.position;
            Quaternion lookRotation2 = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation2, Time.deltaTime * 5f);
            currentRateOfFire += Time.deltaTime;

            if (currentRateOfFire >= rateOfFire)
            {
                GameObject currentBullet = Instantiate(bulletPrefab, firePos.position, firePos.rotation);
                Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
                rb.AddForce(firePos.forward * 2f, ForceMode.Impulse);
                currentRateOfFire = 0;
            }
        }

        if (target.CompareTag("Enemy"))
        {
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
    }

    private void PriorityQueueEnqueue(EnemyData enemyData)
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
        Debug.Log("currentQueueCount is " + currentQueueCount);
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

    private GameObject PriorityQueueDequeue()
    {
        if (currentQueueCount == 0 || priorityQueue[0].Transform == null)
        {
            Debug.Log("priorityQueue[0].Transform is null");
            currentQueueCount = 0;                     ////// ???????????????????????????????? 이렇게 하는게 맞나? 0번째 있는게 삭제가 됐는데 최신화가 안됐을경우임. 이 코드가.
            return null;
        }

        GameObject target = priorityQueue[0].Transform.gameObject;

        priorityQueue[0] = priorityQueue[currentQueueCount - 1];
        currentQueueCount--;

        if (currentQueueCount > 0) Heapify(0);

        if (target == null && currentQueueCount > 0)
        {
            return PriorityQueueDequeue();
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

    private void RemovePriorityQueueElements(Enemy targetEnemy)
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

    void SmoothRotateToZeroXZ()
    {
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, currentRotation.eulerAngles.y, 0);
        transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, Time.deltaTime * 300f);

        //float currentY = transform.rotation.eulerAngles.y;
        //float targetY = originalRotation.eulerAngles.y;

        //Quaternion current = transform.rotation;
        //Quaternion target = Quaternion.Euler(0f, targetY, 0f); // y축만 맞추기

        //transform.rotation = Quaternion.RotateTowards(current, target, Time.deltaTime * 100f);
    }

    void RotateAroundYAxis()
    {
        transform.Rotate(0, 50f * Time.deltaTime, 0, Space.World);
    }

    private void HandleDroneTooFar()
    {
        attackTarget = null;
        turretState = TurretState.Idle;
    }
}