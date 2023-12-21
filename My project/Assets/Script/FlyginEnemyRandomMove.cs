using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FlyginEnemyRandomMove : FlyingEnemy
{
    private float moveSpeed = 0.3f;
    private Vector3 targetPosition;
    private float timeBetweenMoves = 1f;
    private float timeSinceLastMove = 0f;

    // Start is called before the first frame update
    new void Start()
    {
        SetRandomTargetPosition();
    }

    // Update is called once per frame
    new void Update()
    {
        timeSinceLastMove += Time.deltaTime;
        if (timeSinceLastMove >= timeBetweenMoves)
        {
            SetRandomTargetPosition();
            timeSinceLastMove = 0f;
        }
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);

        LookAt(GameObject.Find("Player").transform);
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
}
