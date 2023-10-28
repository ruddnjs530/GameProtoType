using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState { Idle, Run, Jump, StopAttack, MoveAttack, DiveRoll}

public class Player : MonoBehaviour
{
    [SerializeField]
    float moveSpeed = 5;
    float hzInput;
    float vInput;
    Vector3 dir;
    CharacterController cc;

    [SerializeField]
    float groundYOffset;
    [SerializeField]
    LayerMask groundMask;
    //Vector3 playerPos;

    [SerializeField]
    float gravity = -9.8f;
    Vector3 velocity;

    float jumpSpeed = 5;

    Animator anim;

    PlayerState playerState = PlayerState.Idle;

    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();

        anim.SetLayerWeight(1, 0);
    }

    // Update is called once per frame
    void Update()
    {
        switch (playerState)
        {
            case PlayerState.Idle:
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
                    playerState = PlayerState.Run;

                else if (Input.GetKey(KeyCode.Space)) playerState = PlayerState.Jump;
                else if (Input.GetMouseButton(0)) playerState = PlayerState.StopAttack;
                else if (Input.GetKey(KeyCode.LeftShift)) playerState = PlayerState.DiveRoll;
                break;

            case PlayerState.Run:
                Move();
                if (dir.magnitude < 0.1f)
                {
                    playerState = PlayerState.Idle;
                    anim.SetBool("Running", false);
                }
                else if (Input.GetKey(KeyCode.Space)) playerState = PlayerState.Jump;
                else if (Input.GetMouseButton(0)) playerState = PlayerState.MoveAttack;
                else if (Input.GetKey(KeyCode.LeftShift)) playerState = PlayerState.DiveRoll;
                break;

            case PlayerState.Jump:
                Jump();
                if (cc.isGrounded)
                {
                    playerState = PlayerState.Idle;
                    anim.SetBool("Jump", false);
                }
                else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
                    playerState = PlayerState.Run;
                else if (Input.GetMouseButton(0)) playerState = PlayerState.MoveAttack;
                else if (Input.GetKey(KeyCode.LeftShift)) playerState = PlayerState.DiveRoll;
                break;

            case PlayerState.StopAttack:
                Attack();
                if (Input.GetMouseButtonUp(0)) playerState = PlayerState.Idle;
                else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) playerState = PlayerState.Run;
                else if (Input.GetKey(KeyCode.Space)) playerState = PlayerState.Jump;
                else if (Input.GetKey(KeyCode.LeftShift)) playerState = PlayerState.DiveRoll;
                break;

            case PlayerState.MoveAttack:
                Move();
                Attack();
                Jump();
                if (Input.GetMouseButtonUp(0)) playerState = PlayerState.Run;
                else if (Input.GetKey(KeyCode.LeftShift)) playerState = PlayerState.DiveRoll;
                else if (dir.magnitude < 0.1f)
                {
                    playerState = PlayerState.StopAttack;
                    anim.SetBool("Running", false);
                }
                else if (Input.GetMouseButtonUp(0) && dir.magnitude < 0.1f)
                {
                    playerState = PlayerState.Idle;
                    anim.SetBool("Running", false);
                }
                break;

            case PlayerState.DiveRoll:
                DiveRoll();
                break;
        }

        Gravity();
        anim.SetFloat("horizontal", hzInput); // �̰ɷ� �ִϸ��̼� float�� �޾Ƽ� �ϴµ�
        anim.SetFloat("vertical", vInput);
    }

    void Move()
    {
        hzInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");

        dir = transform.forward * vInput + transform.right * hzInput;
        cc.Move(dir.normalized * moveSpeed * Time.deltaTime); // nomalized ���ִ� ������ �밢������ ���� �ӵ��� �� �������� �ʰ� �ϱ� ����
        anim.SetBool("Running", true);
        anim.SetFloat("horizontal", hzInput);
        anim.SetFloat("vertical", vInput);
    }

    //bool IsGrounded()
    //{
    //    playerPos = new Vector3(transform.position.x, transform.position.y - groundYOffset, transform.position.z);
    //    // checksphere�� ���� ����� �Լ�. playerPos�� �������� ������(radius) - 0.05��ŭ �� ũ���� ���� ����� �Ӽ���
    //    // layerMask�� groundMask�� �޾ƿ��� ����
    //    if (Physics.CheckSphere(playerPos, cc.radius - 0.05f, groundMask)) return true;
    //    return false;
    //}

    void Gravity()
    {
        if (!cc.isGrounded) velocity.y += gravity * Time.deltaTime;
        if (velocity.y < 0) velocity.y = -2;

        cc.Move(velocity * Time.deltaTime);
    }
    void Jump()
    {
        if (cc.isGrounded && Input.GetButton("Jump"))
        {
            anim.SetBool("Jump", true);
            velocity.y = jumpSpeed;
        }
           
        cc.Move(velocity * Time.deltaTime);
    }

    void Attack()
    {      
        if (Input.GetMouseButton(0))
        {
            anim.SetLayerWeight(1, 1);
            anim.SetBool("Shooting", true);
        }
        else 
        {
            anim.SetBool("Shooting", false);
            anim.SetLayerWeight(1, 0);
            playerState = PlayerState.Idle;
        }
    }

    void DiveRoll()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Debug.Log("hi");
            anim.SetBool("DiveRoll", true);
            dir = transform.forward * vInput + transform.right * hzInput;
            cc.Move(dir.normalized * 7 * Time.deltaTime);
        }
        else
        {
            anim.SetBool("DiveRoll", false);
            playerState = PlayerState.Idle;
        }
    }
}


