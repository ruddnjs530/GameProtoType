using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum PlayerState { Idle, Run, Jump, StopAttack, MoveAttack, DiveRoll, Die}

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

    [SerializeField] Transform characterBody;
    [SerializeField] Transform cameraArm;
    public Transform aimPos;
    [SerializeField] float aimSpeed = 20;
    [SerializeField] LayerMask aimMask;

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
                else if (Input.GetMouseButton(0) && !Input.GetKey(KeyCode.LeftShift)) playerState = PlayerState.StopAttack;
                else if (Input.GetKeyDown(KeyCode.LeftShift)) playerState = PlayerState.DiveRoll;
                else if (currentHP <= 0) playerState = PlayerState.Die;
                break;

            case PlayerState.Run:
                Move();
                if (dir.magnitude < 0.1f)
                {
                    playerState = PlayerState.Idle;
                    anim.SetBool("Running", false);
                }
                else if (Input.GetKey(KeyCode.Space)) playerState = PlayerState.Jump;
                else if (Input.GetMouseButton(0) && !Input.GetKey(KeyCode.LeftShift)) playerState = PlayerState.MoveAttack;
                else if (Input.GetKeyDown(KeyCode.LeftShift)) playerState = PlayerState.DiveRoll;
                else if (currentHP <= 0) playerState = PlayerState.Die;
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
                else if (Input.GetKeyDown(KeyCode.LeftShift)) playerState = PlayerState.DiveRoll;
                else if (currentHP <= 0) playerState = PlayerState.Die;
                break;

            case PlayerState.StopAttack:
                Attack();
                if (Input.GetMouseButtonUp(0)) playerState = PlayerState.Idle;
                else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) playerState = PlayerState.Run;
                else if (Input.GetKey(KeyCode.Space)) playerState = PlayerState.Jump;
                else if (Input.GetKeyDown(KeyCode.LeftShift)) playerState = PlayerState.DiveRoll;
                else if (currentHP <= 0) playerState = PlayerState.Die;
                break;

            case PlayerState.MoveAttack:
                Move();
                Attack();
                Jump();
                if (Input.GetMouseButtonUp(0)) playerState = PlayerState.Run;
                else if (Input.GetKeyDown(KeyCode.LeftShift)) playerState = PlayerState.DiveRoll;
                else if (currentHP <= 0) playerState = PlayerState.Die;
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
                if (Input.GetKeyUp(KeyCode.LeftShift))
                {
                    playerState = PlayerState.Idle;
                }
                else if (currentHP <= 0) playerState = PlayerState.Die;
                break;
            case PlayerState.Die:
                Destroy(this.gameObject, 2f);
                GameManager.Instance.isPlayerAlive = false;
                break;
        }

        LookAround();
        Gravity();
        anim.SetFloat("horizontal", hzInput);
        anim.SetFloat("vertical", vInput);

        hpBar.value = currentHP / maxHP;
    }

    void Move()
    {
        hzInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");

        dir = new Vector3(hzInput, 0, vInput);

        if (dir != Vector3.zero)
        {
            Vector3 lookForward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z).normalized;
            Vector3 lookRight = new Vector3(cameraArm.right.x, 0f, cameraArm.right.z).normalized;
            Vector3 moveDir = lookForward * dir.z + lookRight * dir.x;

            if (Input.GetMouseButton(0))
            {
                characterBody.forward = lookForward;
                cc.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
            }
            else
            {
                characterBody.forward = moveDir;
                cc.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
            }
        }
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
        if (currentHP <= 0) return;
        currentHP -= damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "HealthBuff")
        {
            theInventory.AcquireItem(other.gameObject.transform.GetComponent<ItemManager>().healthBuff, 1);
            maxHP += 20;
            currentHP += 20;
            Destroy(other.gameObject);  
        }
        if (other.gameObject.tag == "DamageBuff")
        {
            theInventory.AcquireItem(other.gameObject.transform.GetComponent<ItemManager>().damageBuff, 1);
            GameManager.Instance.bulletDamage += 2;
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
        anim.SetBool("DiveRoll", true);
        hzInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");

        dir = transform.forward * vInput + transform.right * hzInput;
        cc.Move(dir.normalized * moveSpeed * 2 * Time.deltaTime);
    }

    private void LookAround()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Vector3 camAngle = cameraArm.rotation.eulerAngles;

        float x = camAngle.x - mouseDelta.y;
        if (x < 180f)
        {
            x = Mathf.Clamp(x, -1f, 70f);
        }
        else
        {
            x = Mathf.Clamp(x, 335f, 361f);
        }

        cameraArm.rotation = Quaternion.Euler(camAngle.x - mouseDelta.y, camAngle.y + mouseDelta.x, camAngle.z);

        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, aimMask))
            aimPos.position = Vector3.Lerp(aimPos.position, hit.point, aimSpeed * Time.deltaTime);
    }
}





