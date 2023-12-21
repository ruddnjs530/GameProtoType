using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurretState { Idle, Attack, Die }
public class TurretObject : MonoBehaviour
{
    List<GameObject> surroundingsObj = new List<GameObject>();
    TurretState turretState;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePos;
    float rateOfFire = 2f;
    float currentRateOfFire = 2f;

    float hp = 100;

    // Start is called before the first frame update
    void Start()
    {
        turretState = TurretState.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        switch (turretState)
        {
            case TurretState.Idle:
                if (hp <= 0) turretState = TurretState.Die;
                else if (CalcurateNearestEnemy() != null)
                    turretState = TurretState.Attack;
                TurretRotate();
                break;
            case TurretState.Attack:
                if (hp <= 0) turretState = TurretState.Die;
                else if (CalcurateNearestEnemy() == null)
                    turretState = TurretState.Idle;
                Attack(CalcurateNearestEnemy());
                break;
            case TurretState.Die:
                Die();
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Enemy") AddObjToList(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Enemy") RemoveObjFromList(other.gameObject);
    }

    void AddObjToList(GameObject obj)
    {
        if (!surroundingsObj.Contains(obj))
            surroundingsObj.Add(obj);
    }

    void RemoveObjFromList(GameObject obj)
    {
        surroundingsObj.Remove(obj);
    }

    GameObject CalcurateNearestEnemy()
    {
        if (surroundingsObj.Count == 0) return null;
        GameObject nearestEnemy = null;
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject enemy in surroundingsObj)
        {
            if (enemy == null) return null;
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestEnemy = enemy;
                nearestDistance = distance;
            }
        }
        return nearestEnemy;
    }

    void TurretRotate()
    {
        this.transform.Rotate(new Vector3(0, 45, 0) * Time.deltaTime);
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

        if (target.GetComponent<Enemy>().isDie) RemoveObjFromList(target);
    }

    void Die()
    {
        Destroy(this.gameObject);
    }
}
