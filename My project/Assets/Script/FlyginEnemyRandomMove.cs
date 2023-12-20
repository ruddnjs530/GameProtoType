using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FlyginEnemyRandomMove : Enemy
{
    private float moveSpeed = 0.3f;
    private Vector3 targetPosition;
    private float timeBetweenMoves = 1f;
    private float timeSinceLastMove = 0f;

    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        SetRandomTargetPosition();
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastMove += Time.deltaTime;
        if (timeSinceLastMove >= timeBetweenMoves)
        {
            SetRandomTargetPosition();
            timeSinceLastMove = 0f;
        }
        // 현재 위치에서 목표 위치로 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);

    }

    void SetRandomTargetPosition()
    {
        // 랜덤한 방향으로 이동할 목표 위치 설정
        float randomAngle = Random.Range(0f, 360f);
        targetPosition = transform.position + Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward * 2f;
    }
}
