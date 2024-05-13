using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum PlayerState { Idle, Move, Attack, Die }

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

    [SerializeField]
    float gravity = -9.8f;
    Vector3 velocity;

    float jumpSpeed = 5;

    public Animator anim;

    PlayerState playerState = PlayerState.Idle;

    float maxHP = 100f;
    float currentHP = 100f;
    [SerializeField] Slider hpBar;
    HealthBar healthBar;

    [SerializeField] Inventory theInventory;

    [SerializeField] public Transform characterBody;
    [SerializeField] Transform cameraArm;
    public Transform aimPos;
    [SerializeField] float aimSpeed = 20;
    [SerializeField] LayerMask aimMask;

    bool isDiveRoll = false;

    float lastInputTime;
    float inputBufferTime = 0.01f;

    [SerializeField] Image coolTimeImage;
    float maxCoolTime = 1.0f;
    CoolTime coolTime;

    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();

        anim.SetLayerWeight(1, 0);

        healthBar = new HealthBar(hpBar, maxHP, false, new Vector2(0.05f, 0.05f));
        coolTime = new CoolTime(coolTimeImage, maxCoolTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.Instance.canPlayerMove) return;
        switch (playerState)
        {
            case PlayerState.Idle:
                if (currentHP <= 0)
                {
                    playerState = PlayerState.Die;
                    break;
                }
                else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)
                    || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Space))
                {
                    anim.SetBool("Running", true);
                    playerState = PlayerState.Move;
                    break;
                }
                else if (Input.GetMouseButton(0))
                {
                    Vector3 lookForward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z).normalized;
                    characterBody.forward = lookForward;

                    anim.SetLayerWeight(1, 1);
                    anim.SetBool("Shooting", true);
                    playerState = PlayerState.Attack;
                    break;
                }
                break;

            case PlayerState.Move:
                if (currentHP <= 0)
                {
                    playerState = PlayerState.Die;
                    break;
                }
                else if (Input.GetMouseButton(0))
                {
                    anim.SetLayerWeight(1, 1);
                    anim.SetBool("Shooting", true);
                    playerState = PlayerState.Attack;
                    break;
                }

                Move();
                if (dir.magnitude < 0.5f && (Time.time - lastInputTime > inputBufferTime))
                {
                    anim.SetBool("Running", false);
                    playerState = PlayerState.Idle;
                    break;
                }
                StartCoroutine(DiveRoll());
                Jump();
                break;

            case PlayerState.Attack:
                if (currentHP <= 0)
                {
                    playerState = PlayerState.Die;
                    break;
                }
                if (Input.GetMouseButtonUp(0))
                {
                    anim.SetLayerWeight(1, 0);
                    anim.SetBool("Shooting", false);

                    if (dir.magnitude < 0.5f)
                    {
                        playerState = PlayerState.Idle;
                        break;
                    }
                    anim.SetBool("Running", true);
                    playerState = PlayerState.Move;

                    break;
                }

                Move();
                Jump();
                StartCoroutine(DiveRoll());
                break;

            case PlayerState.Die:
                Destroy(this.gameObject, 2f);
                GameManager.Instance.isPlayerAlive = false;
                break;
        }

        //anim.SetLayerWeight(1, Mathf.Lerp(0, 1, Time.deltaTime * 2.0f));
        LookAround();
        Gravity();

        anim.SetFloat("horizontal", hzInput);
        anim.SetFloat("vertical", vInput);

        healthBar.SetHealth(currentHP);
    }

    void Move()
    {
        if (isDiveRoll) return;

        hzInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");

        dir = new Vector3(hzInput, 0, vInput);

        if (dir != Vector3.zero)
        {
            Vector3 lookForward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z).normalized;
            Vector3 lookRight = new Vector3(cameraArm.right.x, 0f, cameraArm.right.z).normalized;
            Vector3 moveDir = lookForward * dir.z + lookRight * dir.x;

            Quaternion targetRotation;

            if (Input.GetMouseButton(0))
            {
                targetRotation = Quaternion.LookRotation(lookForward);
            }
            else
            {
                targetRotation = Quaternion.LookRotation(moveDir);
            }

            characterBody.rotation = Quaternion.Slerp(characterBody.rotation, targetRotation, Time.deltaTime * 6.0f);
            cc.Move(moveDir.normalized * moveSpeed * Time.deltaTime);

            lastInputTime = Time.time;
        }
        anim.SetFloat("horizontal", hzInput);
        anim.SetFloat("vertical", vInput);
    }

    IEnumerator DiveRoll()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDiveRoll)
        {
            isDiveRoll = true;
            anim.SetTrigger("DiveRoll");
            StartCoroutine(coolTime.ShowCoolTime());

            Vector3 horizontalMove = cameraArm.right * hzInput;
            Vector3 verticalMove = cameraArm.forward * vInput;
            Vector3 moveDirection = (horizontalMove + verticalMove).normalized;

            float rollSpeed = moveSpeed * 2;

            float startTime = Time.time;
            float rollDuration = 1.0f;
            while (Time.time - startTime < rollDuration)
            {
                cc.Move(moveDirection * rollSpeed * Time.deltaTime);
                yield return null;
            }
            isDiveRoll = false;
        }
    }

    void Gravity()
    {
        if (!cc.isGrounded) velocity.y += gravity * Time.deltaTime;
        else velocity.y = 0;

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





