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
    private Enemy[] priorityQueue = new Enemy[50];
    private int currentQueueCount = 0;
    private readonly Dictionary<Enemy, HashSet<Collider>> detectedColliders =
        new Dictionary<Enemy, HashSet<Collider>>();
    private DroneObject drone;

    public delegate void AttackStateHandler();
    public event AttackStateHandler OnIsAttack;

    // Start is called before the first frame update
    private void Start()
    {
        turretState = TurretState.Idle;
        originalRotation = transform.rotation;

        attackTarget = null;
        drone = GetComponentInParent<DroneObject>();
        if (drone != null) drone.OnPlayerTooFar += HandleDroneTooFar;

        originalRotation = transform.rotation;
    }

    // Update is called once per frame
    private void Update()
    {
        // Moving enemies (and the drone) change priorities. Refresh before selecting.
        RefreshPriorityQueue();
        attackTarget = currentQueueCount > 0 ? priorityQueue[0].gameObject : null;
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
                if (attackTarget == null)
                {
                    Debug.Log("Queue count 0");
                    turretState = TurretState.Idle;
                    break;
                }


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
        RegisterCandidate(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // Recover overlaps after this component or a collider is re-enabled.
        RegisterCandidate(other);
    }

    private void RegisterCandidate(Collider other)
    {
        if (!isActiveAndEnabled) return;
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null || !enemy.CompareTag("Enemy") || enemy.IsDead ||
            !enemy.isActiveAndEnabled) return;

        if (!detectedColliders.TryGetValue(enemy, out HashSet<Collider> colliders))
        {
            colliders = new HashSet<Collider>();
            detectedColliders.Add(enemy, colliders);
            enemy.OnEnemyDied += RemovePriorityQueueElements;
            PriorityQueueEnqueue(enemy);
        }
        colliders.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null || !detectedColliders.TryGetValue(enemy, out HashSet<Collider> colliders)) return;
        colliders.Remove(other);
        if (colliders.Count == 0) RemovePriorityQueueElements(enemy);
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

    private void PriorityQueueEnqueue(Enemy enemy)
    {
        for (int i = 0; i < currentQueueCount; i++)
        {
            if (priorityQueue[i] == enemy) return;
        }

        if (currentQueueCount >= queueMaxSize)
        {
            queueMaxSize *= 2;
            Enemy[] newQueue = new Enemy[queueMaxSize];
            System.Array.Copy(priorityQueue, newQueue, currentQueueCount);
            priorityQueue = newQueue;
        }

        priorityQueue[currentQueueCount] = enemy;
        currentQueueCount++;
        int currentIndex = currentQueueCount - 1;

        while (currentIndex > 0)
        {
            int parentIndex = (currentIndex - 1) / 2;
            if (GetSquaredDistance(priorityQueue[parentIndex]) > GetSquaredDistance(priorityQueue[currentIndex]))
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

    private float GetSquaredDistance(Enemy enemy)
    {
        return enemy == null ? float.PositiveInfinity :
            (enemy.transform.position - transform.position).sqrMagnitude;
    }

    private void Heapify(int currentIndex)
    {
        int lastIndex = currentQueueCount - 1;
        while (true)
        {
            int leftIndex = 2 * currentIndex + 1;
            int rightIndex = 2 * currentIndex + 2;
            int smallestIndex = currentIndex;
            if (leftIndex <= lastIndex && GetSquaredDistance(priorityQueue[leftIndex]) < GetSquaredDistance(priorityQueue[smallestIndex]))
                smallestIndex = leftIndex;
            if (rightIndex <= lastIndex && GetSquaredDistance(priorityQueue[rightIndex]) < GetSquaredDistance(priorityQueue[smallestIndex]))
                smallestIndex = rightIndex;
            if (smallestIndex == currentIndex) break;
            Swap(currentIndex, smallestIndex);
            currentIndex = smallestIndex;
        }
    }

    private void Swap(int index1, int index2)
    {
        Enemy temp = priorityQueue[index1];
        priorityQueue[index1] = priorityQueue[index2];
        priorityQueue[index2] = temp;
    }

    private void RemovePriorityQueueElements(Enemy targetEnemy)
    {
        for (int i = 0; i < currentQueueCount; i++)
        {
            if (ReferenceEquals(priorityQueue[i], targetEnemy))
            {
                RemoveCandidateAt(i);
                RefreshPriorityQueue();
                break;
            }
        }
    }

    private void RemoveCandidateAt(int index)
    {
        Enemy enemy = priorityQueue[index];
        if (enemy != null)
        {
            enemy.OnEnemyDied -= RemovePriorityQueueElements;
        }
        detectedColliders.Remove(enemy);
        if (enemy == null || attackTarget == enemy.gameObject)
            attackTarget = null;
        currentQueueCount--;
        priorityQueue[index] = priorityQueue[currentQueueCount];
        priorityQueue[currentQueueCount] = null;
    }

    private void RefreshPriorityQueue()
    {
        for (int i = 0; i < currentQueueCount;)
        {
            Enemy enemy = priorityQueue[i];
            if (enemy == null || enemy.IsDead || !enemy.isActiveAndEnabled)
            {
                RemoveCandidateAt(i);
                continue;
            }
            HashSet<Collider> colliders = detectedColliders[enemy];
            colliders.RemoveWhere(collider => collider == null || !collider.enabled ||
                !collider.gameObject.activeInHierarchy);
            if (colliders.Count == 0)
            {
                RemoveCandidateAt(i);
                continue;
            }
            i++;
        }
        RebuildHeap();
    }

    private void RebuildHeap()
    {
        int lastParentIndex = currentQueueCount / 2 - 1;
        for (int i = lastParentIndex; i >= 0; i--)
        {
            Heapify(i);
        }
    }

    private void OnDisable()
    {
        while (currentQueueCount > 0) RemoveCandidateAt(currentQueueCount - 1);
        attackTarget = null;
        turretState = TurretState.Idle;
    }

    private void OnDestroy()
    {
        if (drone != null) drone.OnPlayerTooFar -= HandleDroneTooFar;
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
