using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurretState { Idle, Attack, Die }

public class TurretObject : MonoBehaviour
{
    GameObjectPriorityQueue surroundingsObj;
    TurretState turretState;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePos;
    float rateOfFire = 1f;
    float currentRateOfFire = 1f;

    float hp = 100;

    Quaternion originalRotation;

    // Start is called before the first frame update
    void Start()
    {
        turretState = TurretState.Idle;
        originalRotation = transform.rotation;
        surroundingsObj = new GameObjectPriorityQueue();
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
                else if (!surroundingsObj.IsEmpty())
                    turretState = TurretState.Attack;
                break;
            case TurretState.Attack:
                GameObject target = surroundingsObj.Peek();
                if (target == null || target.GetComponent<Enemy>().hp < 0)
                {
                    surroundingsObj.Dequeue(target.transform); // 피가 0이하이면 surroundingObj에서 target을 삭제
                    if (surroundingsObj.IsEmpty())
                    {
                        transform.rotation = originalRotation;
                        turretState = TurretState.Idle;
                        break;
                    }
                    target = surroundingsObj.Peek();
                }

                Attack(target);
                break;
            case TurretState.Die:
                Die();
                break;
        }

        Debug.Log(surroundingsObj.Peek());
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Enemy") surroundingsObj.Enqueue(other.gameObject, transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Enemy") surroundingsObj.Dequeue(other.gameObject.transform);
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
}
public class GameObjectPriorityQueue : MonoBehaviour
{
    private List<GameObject> heap;

    public int Count
    {
        get { return heap.Count; }
    }

    public GameObjectPriorityQueue()
    {
        heap = new List<GameObject>();
    }

    public void Enqueue(GameObject item, Transform referenceTransform)
    {
        heap.Add(item);
        int currentIndex = heap.Count - 1;
        while (currentIndex > 0)
        {
            int parentIndex = (currentIndex - 1) / 2;
            if (DistanceToTransform(heap[currentIndex], referenceTransform) >= DistanceToTransform(heap[parentIndex], referenceTransform))
            {
                break;
            }
            GameObject temp = heap[currentIndex];
            heap[currentIndex] = heap[parentIndex];
            heap[parentIndex] = temp;
            currentIndex = parentIndex;
        }
    }

    public GameObject Dequeue(Transform referenceTransform)
    {
        if (heap.Count == 0)
        {
            return null;
        }
        GameObject frontItem = heap[0];
        int lastIndex = heap.Count - 1;
        heap[0] = heap[lastIndex];
        heap.RemoveAt(lastIndex);

        int currentIndex = 0;
        while (true)
        {
            int leftChildIndex = currentIndex * 2 + 1;
            int rightChildIndex = currentIndex * 2 + 2;
            int smallestChildIndex = 0;

            if (leftChildIndex < heap.Count)
            {
                smallestChildIndex = leftChildIndex;
                if (rightChildIndex < heap.Count && DistanceToTransform(heap[rightChildIndex], referenceTransform) < DistanceToTransform(heap[leftChildIndex], referenceTransform))
                {
                    smallestChildIndex = rightChildIndex;
                }

                if (DistanceToTransform(heap[currentIndex], referenceTransform) <= DistanceToTransform(heap[smallestChildIndex], referenceTransform))
                {
                    break;
                }

                GameObject temp = heap[currentIndex];
                heap[currentIndex] = heap[smallestChildIndex];
                heap[smallestChildIndex] = temp;
                currentIndex = smallestChildIndex;
            }
            else
            {
                break;
            }
        }
        return frontItem;
    }

    public GameObject Peek() // peek() == null 이면 비어있는 함수
    {
        if (heap.Count == 0)
        {
            return null;
        }
        return heap[0];
    }

    public bool IsEmpty()
    {
        return heap.Count == 0;
    }

    private float DistanceToTransform(GameObject gameObject, Transform referenceTransform)
    {
        return Vector3.Distance(gameObject.transform.position, referenceTransform.position);
    }
}


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
