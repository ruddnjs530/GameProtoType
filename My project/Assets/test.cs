using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{

    float moveSpeed = 5;
    float hzInput;
    float vInput;
    Vector3 dir;
    CharacterController cc;
    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        hzInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");

        dir = new Vector3(hzInput, 0, vInput).normalized;

        if (dir != Vector3.zero)
        {
            // 목표 회전 각도 계산 (이동 방향의 각도)
            float targetAngle = Mathf.Atan2(hzInput, vInput) * Mathf.Rad2Deg;
            // 목표 회전 방향 설정
            Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
            // 부드러운 회전 적용
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5 * Time.deltaTime);
        }

        cc.Move(dir * moveSpeed * Time.deltaTime);
    }
}
