using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurretState { Idle, Attack, Die }
public class TurretObject : MonoBehaviour
{
    SortedSet<GameObject> surroundingsObj;
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
        surroundingsObj = new SortedSet<GameObject>(new DistanceComparer(transform.position));
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
                else if (surroundingsObj.Min != null)
                    turretState = TurretState.Attack;
                break;
            case TurretState.Attack:
                if (surroundingsObj.Min.GetComponent<Enemy>().hp < 0) surroundingsObj.Remove(surroundingsObj.Min);
                if (hp <= 0)
                {
                    turretState = TurretState.Die;
                    break;
                }
                else if (surroundingsObj.Min == null)
                {  
                    transform.rotation = originalRotation;
                    turretState = TurretState.Idle;
                    break;
                }
                Attack(surroundingsObj.Min);
                break;
            case TurretState.Die:
                Die();
                break;
        }

        Debug.Log(surroundingsObj.Min);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Enemy") surroundingsObj.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Enemy") surroundingsObj.Remove(other.gameObject);
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
        Destroy(this.gameObject);
    }
}
public class DistanceComparer : IComparer<GameObject>
{
    private Vector3 turretPosition; 
    public DistanceComparer(Vector3 turretPosition)
    {
        this.turretPosition = turretPosition;
    }

    public int Compare(GameObject x, GameObject y)
    {
        float distanceX = Vector3.Distance(x.transform.position, turretPosition);
        float distanceY = Vector3.Distance(y.transform.position, turretPosition);

        if (distanceX < distanceY) return -1;
        if (distanceX > distanceY) return 1;
        return 0;
    }
}



//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public enum TurretState { Idle, Attack, Die }
//public class TurretObject : MonoBehaviour
//{
//    List<GameObject> surroundingsObj = new List<GameObject>();
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
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        switch (turretState)
//        {
//            case TurretState.Idle:
//                transform.Rotate(new Vector3(0, 45, 0) * Time.deltaTime);
//                if (hp <= 0) turretState = TurretState.Die;
//                else if (CalcurateNearestEnemy() != null)
//                    turretState = TurretState.Attack;
//                break;
//            case TurretState.Attack:
//                Attack(CalcurateNearestEnemy());

//                if (hp <= 0) turretState = TurretState.Die;
//                else if (CalcurateNearestEnemy() == null)
//                {
//                    transform.rotation = originalRotation;
//                    turretState = TurretState.Idle;
//                    break;
//                }
//                if (CalcurateNearestEnemy().GetComponent<Enemy>().hp <= 0)
//                {
//                    surroundingsObj.Remove(CalcurateNearestEnemy());
//                    Debug.Log("hello");
//                }
//                break;
//            case TurretState.Die:
//                Die();
//                break;
//        }

//        if (surroundingsObj.Count > 0) Debug.Log(surroundingsObj[0]);
//    }

//    private void OnTriggerStay(Collider other)
//    {
//        if (other.gameObject.tag == "Enemy") AddObjToList(other.gameObject);
//    }

//    private void OnTriggerExit(Collider other)
//    {
//        if (other.gameObject.tag == "Enemy") surroundingsObj.Remove(other.gameObject);
//    }

//    void AddObjToList(GameObject obj)
//    {
//        if (!surroundingsObj.Contains(obj))
//            surroundingsObj.Add(obj);
//    }

//    GameObject CalcurateNearestEnemy()
//    {
//        if (surroundingsObj.Count == 0) return null;
//        GameObject nearestEnemy = null;
//        float nearestDistance = Mathf.Infinity;

//        foreach (GameObject enemy in surroundingsObj)
//        {
//            if (enemy == null) return null;
//            float distance = Vector3.Distance(transform.position, enemy.transform.position);
//            if (distance < nearestDistance)
//            {
//                nearestEnemy = enemy;
//                nearestDistance = distance;
//            }
//        }
//        return nearestEnemy;
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
