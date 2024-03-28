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
                    priorityQueueDequeue(); // 피가 0이하이면 surroundingObj에서 target을 삭제
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

        while (nowIndex > 0) // 여기서 빠져나가는 구문이 없어서 무한 while루프에 빠짐
        {
            int parentNode = (nowIndex - 1) / 2;
            if (DistanceWithEnemy(priorityQueue[parentNode]) < DistanceWithEnemy(priorityQueue[nowIndex])) break;
            // 부모 노드와 자식노드를 비교해서 부모가 더 작으면 while문 빠져나감. 거리가 더 가까울 수록 우선순위

            Swap(priorityQueue[parentNode], priorityQueue[nowIndex]);

            nowIndex = parentNode;
        }

    }

    void priorityQueueDequeue() // 리스트에서 요소 삭제시 뒤에 있는 것들이 자동으로 당겨지지만 우선순위큐를 사용하기에 자동으로 당겨지는건 무쓸모
    {
        if (priorityQueue.Count - 1 < 0) return;

        int lastIndex = priorityQueue.Count - 1;

        priorityQueue[0] = priorityQueue[priorityQueue.Count - 1];
        priorityQueue.RemoveAt(priorityQueue.Count - 1);

        int nowIndex = 0; // 현재 노드
        while (true)
        {
            int leftIndex = 2 * nowIndex + 1; // 왼쪽 자식
            int rightIndex = 2 * nowIndex + 2; // 오른쪽 자식

            if (leftIndex <= lastIndex && DistanceWithEnemy(priorityQueue[leftIndex]) > DistanceWithEnemy(priorityQueue[nowIndex])) // 마지막 인덱스보다 작고 ( 위에 있을 수록 작은 인덱스 )
                // 거리를 비교했을 때 더 자식이 더 크면 위치 바꿈.
            {
                nowIndex = leftIndex;
                Swap(priorityQueue[leftIndex], priorityQueue[nowIndex]);
            }
            else if (rightIndex <= lastIndex && DistanceWithEnemy(priorityQueue[rightIndex]) > DistanceWithEnemy(priorityQueue[nowIndex]))
            {
                nowIndex = rightIndex;
                Swap(priorityQueue[rightIndex], priorityQueue[nowIndex]);
            }
            else break; // 두 경우 모두 아니라면 while문 종료
        }
    }

    // 거리에 따라 정렬하는 함수를 따로 만들어야함. 그래야 위치가 바뀔 때 정렬이 됨.
    // 그리고 우선순위큐 안에 들어가 있는데 플레이어가 죽여서 없어질 경우에는 어떻게 알아채서 삭제하지?
    
    // 1. 죽었는지 어떻게 확인할까
    // 2. 죽은 적이 우선순위큐 내에서 몇번째 인지 어떻게 알지?

    void Swap (GameObject obj1, GameObject obj2)
    {
        GameObject temp = obj1;
        obj1 = obj2;
        obj2 = temp;
    }

    public GameObject priorityQueuePeek() // peek() == null 이면 비어있는 함수
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
//                surroundingsObj.Dequeue(target.transform); // 피가 0이하이면 surroundingObj에서 target을 삭제
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

//    public GameObject Peek() // peek() == null 이면 비어있는 함수
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
