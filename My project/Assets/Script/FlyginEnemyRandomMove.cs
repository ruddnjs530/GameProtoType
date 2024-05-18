using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FlyginEnemyRandomMove : MonoBehaviour
{
    private float moveSpeed = 0.3f;
    private Vector3 targetPosition;
    private float timeBetweenMoves = 1f;
    private float timeSinceLastMove = 0f;

    bool isLook = false;

    // Start is called before the first frame update
    void Start()
    {
        SetRandomTargetPosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLook)
        {
            timeSinceLastMove += Time.deltaTime;
            if (timeSinceLastMove >= timeBetweenMoves)
            {
                SetRandomTargetPosition();
                timeSinceLastMove = 0f;
            }
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
        }
    }

    void SetRandomTargetPosition()
    {
        // 랜덤한 방향으로 이동할 목표 위치 설정
        float randomAngle = Random.Range(0f, 360f);
        targetPosition = transform.position + Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward * 2f;
    }

    void LookAt(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private void OnTriggerStay(Collider other) // 이거는 안됨. 여기도 body를 받고 body를 움직이게 해야할듯. 이러면 randommove도 enemy에 넣을 수 있음. 하나로
    {
        if (other.transform.CompareTag("Player")) isLook = true;
        if (GameObject.FindWithTag("Player"))
            LookAt(GameObject.FindWithTag("Player").transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.CompareTag("Player")) isLook = false;
    }
}
