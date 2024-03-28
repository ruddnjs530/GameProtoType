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
                else if (priorityQueue.Count != 0)
                    Debug.Log("hi");
                    turretState = TurretState.Attack;
                break;
            case TurretState.Attack:
                GameObject target = priorityQueuePeek();
                if (target == null || target.GetComponent<Enemy>().hp < 0)
                {
                    priorityQueueDequeue(); // �ǰ� 0�����̸� surroundingObj���� target�� ����
                    if (priorityQueue.Count == 0)
                    {
                        transform.rotation = originalRotation;
                        turretState = TurretState.Idle;
                        break;
                    }
                    target = priorityQueuePeek();
                }

                Attack(target);
                break;
            case TurretState.Die:
                Die();
                break;
        }

        Debug.Log(priorityQueuePeek());
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Enemy") priorityQueueEnqueue(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Enemy") priorityQueueDequeue();
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
            Debug.Log("add enemy");
        }

        int nowIndex = priorityQueue.Count - 1;

        while (nowIndex > 0) // ���⼭ ���������� ������ ��� ���� while������ ����
        {
            int parentNode = (nowIndex - 1) / 2;
            if (DistanceWithEnemy(priorityQueue[parentNode]) < DistanceWithEnemy(priorityQueue[nowIndex])) break;
            // �θ� ���� �ڽĳ�带 ���ؼ� �θ� �� ������ while�� ��������. �Ÿ��� �� ����� ���� �켱����

            Swap(priorityQueue[parentNode], priorityQueue[nowIndex]);

            nowIndex = parentNode;
        }

    }

    void priorityQueueDequeue() // ����Ʈ���� ��� ������ �ڿ� �ִ� �͵��� �ڵ����� ��������� �켱����ť�� ����ϱ⿡ �ڵ����� ������°� ������
    {
        if (priorityQueue.Count - 1 < 0) return;

        int lastIndex = priorityQueue.Count - 1;

        priorityQueue[0] = priorityQueue[priorityQueue.Count - 1];
        priorityQueue.RemoveAt(priorityQueue.Count - 1);

        int nowIndex = 0; // ���� ���
        while (true)
        {
            int leftIndex = 2 * nowIndex + 1; // ���� �ڽ�
            int rightIndex = 2 * nowIndex + 2; // ������ �ڽ�

            if (leftIndex <= lastIndex && DistanceWithEnemy(priorityQueue[leftIndex]) > DistanceWithEnemy(priorityQueue[nowIndex])) // ������ �ε������� �۰� ( ���� ���� ���� ���� �ε��� )
                // �Ÿ��� ������ �� �� �ڽ��� �� ũ�� ��ġ �ٲ�.
            {
                nowIndex = leftIndex;
                Swap(priorityQueue[leftIndex], priorityQueue[nowIndex]);
            }
            else if (rightIndex <= lastIndex && DistanceWithEnemy(priorityQueue[rightIndex]) > DistanceWithEnemy(priorityQueue[nowIndex]))
            {
                nowIndex = rightIndex;
                Swap(priorityQueue[rightIndex], priorityQueue[nowIndex]);
            }
            else break; // �� ��� ��� �ƴ϶�� while�� ����
        }
    }

    // �Ÿ��� ���� �����ϴ� �Լ��� ���� ��������. �׷��� ��ġ�� �ٲ� �� ������ ��.
    // �׸��� �켱����ť �ȿ� �� �ִµ� �÷��̾ �׿��� ������ ��쿡�� ��� �˾�ä�� ��������?
    
    // 1. �׾����� ��� Ȯ���ұ�
    // 2. ���� ���� �켱����ť ������ ���° ���� ��� ����?

    void Swap (GameObject obj1, GameObject obj2)
    {
        GameObject temp = obj1;
        obj1 = obj2;
        obj2 = temp;
    }

    public GameObject priorityQueuePeek() // peek() == null �̸� ����ִ� �Լ�
    {
        if (priorityQueue.Count == 0)
        {
            return null;
        }
        return priorityQueue[0];
    }

    float DistanceWithEnemy(GameObject enemy)
    {
        float distance = Vector3.Distance(enemy.transform.position, this.transform.position);
        return distance;
    }
}

//void Update()
//{
//    switch (turretState)
//    {
//        case TurretState.Idle:
//            transform.Rotate(new Vector3(0, 45, 0) * Time.deltaTime);
//            if (hp <= 0)
//            {
//                turretState = TurretState.Die;
//                break;
//            }
//            else if (!surroundingsObj.IsEmpty())
//                turretState = TurretState.Attack;
//            break;
//        case TurretState.Attack:
//            GameObject target = surroundingsObj.Peek();
//            if (target == null || target.GetComponent<Enemy>().hp < 0)
//            {
//                surroundingsObj.Dequeue(target.transform); // �ǰ� 0�����̸� surroundingObj���� target�� ����
//                if (surroundingsObj.IsEmpty())
//                {
//                    transform.rotation = originalRotation;
//                    turretState = TurretState.Idle;
//                    break;
//                }
//                target = surroundingsObj.Peek();
//            }

//            Attack(target);
//            break;
//        case TurretState.Die:
//            Die();
//            break;
//    }

//    Debug.Log(surroundingsObj.Peek());
//}

//private void OnTriggerStay(Collider other)
//{
//    if (other.gameObject.tag == "Enemy") surroundingsObj.Enqueue(other.gameObject, transform);
//}

//private void OnTriggerExit(Collider other)
//{
//    if (other.gameObject.tag == "Enemy") surroundingsObj.Dequeue(other.gameObject.transform);
//}

//void Attack(GameObject target)
//{
//    if (target == null) return;
//    Vector3 direction = target.transform.position - transform.position;
//    Quaternion lookRotation = Quaternion.LookRotation(direction);
//    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
//    currentRateOfFire += Time.deltaTime;

//    if (currentRateOfFire >= rateOfFire)
//    {
//        GameObject currentBullet = Instantiate(bulletPrefab, firePos.position, firePos.rotation);
//        Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
//        rb.AddForce(firePos.forward * 2f, ForceMode.Impulse);
//        currentRateOfFire = 0;
//    }
//}
//public class GameObjectPriorityQueue : MonoBehaviour
//{
//    private List<GameObject> heap;

//    public int Count
//    {
//        get { return heap.Count; }
//    }

//    public GameObjectPriorityQueue()
//    {
//        heap = new List<GameObject>();
//    }

//    public void Enqueue(GameObject item, Transform referenceTransform)
//    {
//        heap.Add(item);
//        int currentIndex = heap.Count - 1;
//        while (currentIndex > 0)
//        {
//            int parentIndex = (currentIndex - 1) / 2;
//            if (DistanceToTransform(heap[currentIndex], referenceTransform) >= DistanceToTransform(heap[parentIndex], referenceTransform))
//            {
//                break;
//            }
//            GameObject temp = heap[currentIndex];
//            heap[currentIndex] = heap[parentIndex];
//            heap[parentIndex] = temp;
//            currentIndex = parentIndex;
//        }
//    }

//    public GameObject Dequeue(Transform referenceTransform)
//    {
//        if (heap.Count == 0)
//        {
//            return null;
//        }
//        GameObject frontItem = heap[0];
//        int lastIndex = heap.Count - 1;
//        heap[0] = heap[lastIndex];
//        heap.RemoveAt(lastIndex);

//        int currentIndex = 0;
//        while (true)
//        {
//            int leftChildIndex = currentIndex * 2 + 1;
//            int rightChildIndex = currentIndex * 2 + 2;
//            int smallestChildIndex = 0;

//            if (leftChildIndex < heap.Count)
//            {
//                smallestChildIndex = leftChildIndex;
//                if (rightChildIndex < heap.Count && DistanceToTransform(heap[rightChildIndex], referenceTransform) < DistanceToTransform(heap[leftChildIndex], referenceTransform))
//                {
//                    smallestChildIndex = rightChildIndex;
//                }

//                if (DistanceToTransform(heap[currentIndex], referenceTransform) <= DistanceToTransform(heap[smallestChildIndex], referenceTransform))
//                {
//                    break;
//                }

//                GameObject temp = heap[currentIndex];
//                heap[currentIndex] = heap[smallestChildIndex];
//                heap[smallestChildIndex] = temp;
//                currentIndex = smallestChildIndex;
//            }
//            else
//            {
//                break;
//            }
//        }
//        return frontItem;
//    }

//    public GameObject Peek() // peek() == null �̸� ����ִ� �Լ�
//    {
//        if (heap.Count == 0)
//        {
//            return null;
//        }
//        return heap[0];
//    }

//    public bool IsEmpty()
//    {
//        return heap.Count == 0;
//    }

//    private float DistanceToTransform(GameObject gameObject, Transform referenceTransform)
//    {
//        return Vector3.Distance(gameObject.transform.position, referenceTransform.position);
//    }
//}


//public class TurretObject : MonoBehaviour
//{
//    SortedSet<GameObject> surroundingsObj;
//    TurretState turretState;
//    [SerializeField] GameObject bulletPrefab;
//    [SerializeField] Transform firePos;
//    float rateOfFire = 1f;
//    float currentRateOfFire = 1f;

//    float hp = 100;

//    Quaternion originalRotation;

//    // Start is called before the first frame update
//    void Start()
//    {
//        turretState = TurretState.Idle;
//        originalRotation = transform.rotation;
//        surroundingsObj = new SortedSet<GameObject>(new DistanceComparer(transform.position));
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        switch (turretState)
//        {
//            case TurretState.Idle:
//                transform.Rotate(new Vector3(0, 45, 0) * Time.deltaTime);
//                if (hp <= 0)
//                {
//                    turretState = TurretState.Die;
//                    break;
//                }
//                else if (surroundingsObj.Min != null)
//                    turretState = TurretState.Attack;
//                break;
//            case TurretState.Attack:
//                if (surroundingsObj.Min.GetComponent<Enemy>().hp < 0) surroundingsObj.Remove(surroundingsObj.Min);
//                if (hp <= 0)
//                {
//                    turretState = TurretState.Die;
//                    break;
//                }
//                else if (surroundingsObj.Min == null)
//                {  
//                    transform.rotation = originalRotation;
//                    turretState = TurretState.Idle;
//                    break;
//                }
//                Attack(surroundingsObj.Min);
//                break;
//            case TurretState.Die:
//                Die();
//                break;
//        }

//        Debug.Log(surroundingsObj.Min);
//    }

//    private void OnTriggerStay(Collider other)
//    {
//        if (other.gameObject.tag == "Enemy") surroundingsObj.Add(other.gameObject);
//    }

//    private void OnTriggerExit(Collider other)
//    {
//        if (other.gameObject.tag == "Enemy") surroundingsObj.Remove(other.gameObject);
//    }

//    void Attack(GameObject target)
//    {
//        if (target == null) return;
//        Vector3 direction = target.transform.position - transform.position;
//        Quaternion lookRotation = Quaternion.LookRotation(direction);
//        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
//        currentRateOfFire += Time.deltaTime;

//        if (currentRateOfFire >= rateOfFire)
//        {
//            GameObject currentBullet = Instantiate(bulletPrefab, firePos.position, firePos.rotation);
//            Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
//            rb.AddForce(firePos.forward * 2f, ForceMode.Impulse);
//            currentRateOfFire = 0;
//        }
//    }
//    void Die()
//    {
//        Destroy(this.gameObject);
//    }
//}
//public class DistanceComparer : IComparer<GameObject>
//{
//    private Vector3 turretPosition; 
//    public DistanceComparer(Vector3 turretPosition)
//    {
//        this.turretPosition = turretPosition;
//    }

//    public int Compare(GameObject x, GameObject y)
//    {
//        float distanceX = Vector3.Distance(x.transform.position, turretPosition);
//        float distanceY = Vector3.Distance(y.transform.position, turretPosition);

//        if (distanceX < distanceY) return -1;
//        if (distanceX > distanceY) return 1;
//        return 0;
//    }
//}
