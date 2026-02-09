using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum PlayerState { Idle, Move, Attack, Die }

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5; // 이동 속도
    [SerializeField] private float rotationSpeed = 6.0f; // 회전 속도
    [SerializeField] private float jumpSpeed = 5; // 점프력
    [SerializeField] private float gravity = -9.8f; // 중력
    
    [Header("Dash Settings")]
    [SerializeField] private float rollSpeedMultiplier = 2.0f; // 구르기 속도 배율
    [SerializeField] private float rollDuration = 1.0f; // 구르기 지속 시간

    [Header("Combat Settings")]
    [SerializeField] private float attackRotationSpeed = 1000.0f; // 공격 시 회전 속도
    [SerializeField] private float maxHP = 100; // 최대 체력
    private float currentHP = 100; // 현재 체력

    [Header("References")]
    [SerializeField] private Slider hpBar; // 체력바 UI
    [SerializeField] private Inventory theInventory; // 인벤토리 참조
    [SerializeField] public Transform characterBody; // 캐릭터 모델
    [SerializeField] private Transform cameraArm; // 카메라 암
    public Transform aimPos; // 조준점
    [SerializeField] private float aimSpeed = 20; // 조준점 이동 속도
    [SerializeField] private LayerMask aimMask; // 조준 레이어 마스크
    [SerializeField] private Image coolTimeImage; // 쿨타임 이미지

    // State Variables
    private float hzInput; // 수평 입력
    private float vInput; // 수직 입력
    private Vector3 dir; // 이동 방향
    private CharacterController cc; // 캐릭터 컨트롤러
    private Vector3 velocity; // 현재 속도 (중력 포함)
    public Animator anim; // 애니메이터

    PlayerState playerState = PlayerState.Idle; // 현재 플레이어 상태
    private HealthBar healthBar; // 체력바 스크립트

    private bool isDash = false; // 대시 중 여부

    private float lastInputTime;
    private float inputBufferTime = 0.01f;

    private float maxCoolTime = 1.0f; // 스킬 쿨타임
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

            characterBody.rotation = Quaternion.Slerp(characterBody.rotation, targetRotation, Time.deltaTime * rotationSpeed);
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

            float rollSpeed = moveSpeed * rollSpeedMultiplier;

            float startTime = Time.time;
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
        characterBody.rotation = Quaternion.RotateTowards(characterBody.rotation, targetRotation, Time.deltaTime * attackRotationSpeed);
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