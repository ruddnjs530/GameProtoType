using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


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

    float maxHP = 100f;
    float currentHP = 100f;
    [SerializeField] Slider hpBar;

    [SerializeField] Inventory theInventory;

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
        if (!GameManager.Instance.canPlayerMove) return;
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
                    //anim.SetBool("Jump", false);
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
        anim.SetFloat("horizontal", hzInput); // 이걸로 애니메이션 float값 받아서 하는듯
        anim.SetFloat("vertical", vInput);

        hpBar.value = currentHP / maxHP;
    }

    void Move()
    {
        hzInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");

        dir = transform.forward * vInput + transform.right * hzInput;
        cc.Move(dir.normalized * moveSpeed * Time.deltaTime); // nomalized 해주는 이유는 대각선으로 갈때 속도가 더 빨라지지 않게 하기 위함
        anim.SetBool("Running", true);
        anim.SetFloat("horizontal", hzInput);
        anim.SetFloat("vertical", vInput);
    }

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
            //anim.SetBool("Jump", true);
            anim.SetTrigger("Jump");
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

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Item")
        {
            theInventory.AcquireItem(other.gameObject.transform.GetComponent<ItemManager>().item, 1);
            Destroy(other.gameObject);  
        }
    }

    public void IncreaseHP(float hp)
    {
        if (currentHP >= maxHP) return;
        if (currentHP <= maxHP)
            currentHP += hp;
    }

    public void GivingHPAndGetMoney(float hp, int money)
    {
        currentHP -= hp;
        GameManager.Instance.money += money;
    }

    public float GetHP() { return currentHP; }
    public void SetHP(float hp) { currentHP = hp; }

    void DiveRoll()
    {
        anim.SetTrigger("DiveRoll");
        //dir = transform.forward * vInput + transform.right * hzInput;
        //cc.Move(dir.normalized * 7 * Time.deltaTime);
        playerState = PlayerState.Idle;

    }

    //bool IsGrounded()
    //{
    //    playerPos = new Vector3(transform.position.x, transform.position.y - groundYOffset, transform.position.z);
    //    // checksphere는 구를 만드는 함수. playerPos를 기준으로 반지름(radius) - 0.05만큼 뺀 크기의 구를 만들고 속성을
    //    // layerMask인 groundMask로 받아오는 느낌
    //    if (Physics.CheckSphere(playerPos, cc.radius - 0.05f, groundMask)) return true;
    //    return false;
    //}
}





