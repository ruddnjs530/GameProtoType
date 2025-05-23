using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum PlayerState { Idle, Move, Attack, Die }

public class Player : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5;
    private float hzInput;
    private float vInput;
    private Vector3 dir;
    private CharacterController cc;

    [SerializeField]
    private float groundYOffset;
    [SerializeField]
    private LayerMask groundMask;

    [SerializeField]
    private float gravity = -9.8f;
    private Vector3 velocity;

    private float jumpSpeed = 5;

    public Animator anim;

    PlayerState playerState = PlayerState.Idle;

    private float maxHP = 100;
    private float currentHP = 100;
    [SerializeField] private Slider hpBar;
    private HealthBar healthBar;

    [SerializeField] private Inventory theInventory;

    [SerializeField] public Transform characterBody;
    [SerializeField] private Transform cameraArm;
    public Transform aimPos;
    [SerializeField] private float aimSpeed = 20;
    [SerializeField] private LayerMask aimMask;

    private bool isDash = false;

    private float lastInputTime;
    private float inputBufferTime = 0.01f;

    [SerializeField] private Image coolTimeImage;
    private float maxCoolTime = 1.0f;
    private CoolTime coolTime;

    public static event System.Action OnPlayerDeath;

    // Start is called before the first frame update

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(aimPos.gameObject);
    }

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
                    || Input.GetKey(KeyCode.LeftShift))
                {
                    anim.SetBool("Running", true);
                    playerState = PlayerState.Move;
                    break;
                }
                else if (Input.GetKey(KeyCode.Space))
                {
                    playerState = PlayerState.Move;
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
                Jump();
                if (dir.magnitude < 0.5f && (Time.time - lastInputTime > inputBufferTime))
                {
                    anim.SetBool("Running", false);
                    playerState = PlayerState.Idle;
                    break;
                }
                StartCoroutine(Dash());
                break;

            case PlayerState.Attack:
                if (currentHP <= 0)
                {
                    playerState = PlayerState.Die;
                    break;
                }
                else if (Input.GetMouseButtonUp(0))
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
                AttackMovement();
                Move();
                Jump();
                StartCoroutine(Dash());
                break;

            case PlayerState.Die:
                OnPlayerDeath?.Invoke();
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

    private void Move()
    {
        if (isDash) return;

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

    private IEnumerator Dash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDash)
        {
            isDash = true;
            anim.SetTrigger("Dash");
            //StartCoroutine(coolTime.ShowCoolTime());

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
            isDash = false;
        }
    }

    private void Gravity()
    {
        if (!cc.isGrounded) velocity.y += gravity * Time.deltaTime;
        else velocity.y = 0;

        cc.Move(velocity * Time.deltaTime);
    }

    private void Jump()
    {
        if (cc.isGrounded && Input.GetButton("Jump"))
        {
            anim.SetTrigger("Jump");
            velocity.y = jumpSpeed;
        }

        cc.Move(velocity * Time.deltaTime);
    }

    private void AttackMovement()
    {
        Vector3 lookForward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(lookForward);
        characterBody.rotation = Quaternion.RotateTowards(characterBody.rotation, targetRotation, Time.deltaTime * 1000.0f);
    }

    public void TakeDamage(float damage)
    {
        if (currentHP <= 0) return;
        currentHP -= damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HealthBuff"))
        {
            theInventory.AcquireItem(other.gameObject.transform.GetComponent<ItemManager>().healthBuff, 1);
            maxHP += 20;
            currentHP += 20;
            Destroy(other.gameObject);
        }
        if (other.CompareTag("DamageBuff"))
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
    public void SetHP(float hp) { currentHP = hp;
        healthBar.SetHealth(currentHP);  }

    public float GetMaxHP() { return maxHP; }

    public void SetMaxHP(float hp) { maxHP = hp;  }


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