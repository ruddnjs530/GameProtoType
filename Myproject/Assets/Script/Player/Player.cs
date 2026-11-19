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
    [SerializeField] private float animDampTime = 0.1f; // 이동 블렌드 파라미터 감쇠 시간
    [SerializeField] private float jumpSpeed = 5; // 점프력
    [SerializeField] private float gravity = -9.8f; // 중력
    // 접지 중에도 아래로 살짝 눌러 주는 속도. 0으로 두면 한 프레임 이동량이 없어 isGrounded가 false로 깜빡이고,
    // 그 프레임에는 점프가 무시된다(고프레임에서 Idle 점프가 거의 안 되던 원인).
    [SerializeField] private float groundStickSpeed = 2f;

    [Header("Jump Animation")]
    // Jump 클립(1.2초)은 제자리 점프라 클립 안에서 스스로 착지한다(정규화 시간 0.70에 발이 땅에 닿음).
    // 클립을 시간대로 재생하면 실제 착지(물리)와 어긋나 공중에서 다리가 먼저 펴지므로,
    // Animator의 JumpTime(Motion Time)으로 재생 위치를 물리 상태에 맞춰 직접 정한다.
    [SerializeField] private float jumpClipLength = 1.2f; // Jump 클립 길이(초)
    [SerializeField] private float jumpTouchdownTime = 0.70f; // 클립에서 발이 땅에 닿는 시점
    [SerializeField] private float jumpFallbackHoldTime = 0.60f; // 아래에 땅이 감지되지 않을 때(낭떠러지 등)만 이 자세에서 기다린다
    [SerializeField] private float jumpMaxPlaySpeed = 2.5f; // 착지 예측이 갑자기 짧아져도(턱 위 착지 등) 클립이 이 배속보다 빨리 넘어가지 않게 한다
    [SerializeField] private LayerMask groundMask = ~0; // 착지까지 남은 거리를 잴 때 맞출 레이어(Player 레이어는 코드에서 제외)

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
    [SerializeField] private LayerMask aimMask; // 조준 레이가 맞는 레이어(지형·적 피격 콜라이더). 트리거는 무시한다
    [SerializeField] private float maxAimDistance = 200f; // 레이가 아무것도 맞추지 못했을 때 조준점을 둘 거리
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
    private float dashEndTime;    // 대시가 끝나는 시각
    private Vector3 dashDirection; // 현재 대시 방향(수평)

    private bool isJumpAnimating; // JumpTime을 코드에서 갱신 중인지
    private bool hasJumpLanded; // 이번 점프에서 실제로 착지했는지
    private float jumpAnimTime; // Jump 상태의 정규화 재생 위치(0~1)
    private float jumpStartTime; // 이번 점프를 시작한 시각
    // 수직 이동(Gravity의 cc.Move) 결과로 구한 접지 여부. cc.isGrounded는 마지막 Move 호출 결과라서,
    // 이동 중에는 수평 이동(Move/DashMove) 결과가 되어 평지에서도 false로 깜빡였다(이동 중 점프 무시, 낙하 속도 누적).
    private bool isGrounded;
    private static readonly int JumpStateHash = Animator.StringToHash("Base Layer.Jump");

    private float lastInputTime;
    private float inputBufferTime = 0.01f;

    private float maxCoolTime = 1.0f; // 스킬 쿨타임
    private CoolTime coolTime;

    public static event System.Action OnPlayerDeath;

    // 경직이 끝나는 시각. GameManager.canPlayerMove는 UI 상태로 매 프레임 다시 계산되므로
    // 경직을 거기에 쓰면 다음 프레임에 풀린다. 경직은 플레이어가 직접 소유한다.
    private float stunEndTime = -1f;
    public bool IsStunned => Time.time < stunEndTime;

    /// <summary>이동·사격·대시가 가능한 상태인지. 무기 등 다른 컴포넌트도 이 값으로 판단한다.</summary>
    public bool CanAct => playerState != PlayerState.Die && !IsStunned && GameManager.Instance.canPlayerMove;

    // Start is called before the first frame update

    void Awake()
    {
        GameManager.KeepForRun(gameObject);
        GameManager.KeepForRun(aimPos.gameObject);
    }

    void Start()
    {
        cc = GetComponent<CharacterController>();
        // 기본값 0.001이면 고프레임(dt가 작을 때)의 작은 이동이 무시되어 접지 판정이 끊긴다.
        cc.minMoveDistance = 0f;
        anim = GetComponent<Animator>();

        anim.SetLayerWeight(1, 0);

        healthBar = new HealthBar(hpBar, maxHP, false, new Vector2(0.05f, 0.05f));
        coolTime = new CoolTime(coolTimeImage, maxCoolTime);
    }

    // Update is called once per frame
    void Update()
    {
        // 사망은 UI가 열려 있어도 판정해야 하므로 이동 가능 여부보다 먼저 검사한다.
        if (playerState == PlayerState.Die)
        {
            Gravity();
            return;
        }
        if (currentHP <= 0)
        {
            EnterDie();
            return;
        }

        if (!GameManager.Instance.canPlayerMove) return;

        // 경직 중에는 시점 회전과 중력(넉백 후 낙하)만 처리한다.
        if (IsStunned)
        {
            LookAround();
            Gravity();
            UpdateJumpAnimation();
            healthBar.SetHealth(currentHP);
            return;
        }

        switch (playerState)
        {
            case PlayerState.Idle:
                if (InputManager.Instance.IsMoveKey || InputManager.Instance.IsRun)
                {
                    anim.SetBool("Running", true);
                    playerState = PlayerState.Move;
                    // 대시 키(Shift 누름)는 한 프레임만 true라서, 여기서 처리하지 않으면 정지 상태에서의 대시가 무시된다.
                    TryStartDash();
                    break;
                }
                else if (InputManager.Instance.IsFire)
                {
                    Vector3 lookForward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z).normalized;
                    characterBody.forward = lookForward;

                    anim.SetLayerWeight(1, 1);
                    anim.SetBool("Shooting", true);
                    playerState = PlayerState.Attack;
                    break;
                }
                // 제자리 점프는 Idle에서 바로 처리한다(이전에는 Move로 넘어갔다가 다음 프레임에 점프).
                Jump();
                break;

            case PlayerState.Move:
                if (InputManager.Instance.IsFire)
                {
                    anim.SetLayerWeight(1, 1);
                    anim.SetBool("Shooting", true);
                    playerState = PlayerState.Attack;
                    break;
                }

                Move();
                Jump();
                TryStartDash();
                // 입력 없이 대시 중이어도 대시가 끝날 때까지는 Move 상태를 유지한다.
                if (!isDash && dir.magnitude < 0.5f && (Time.time - lastInputTime > inputBufferTime))
                {
                    anim.SetBool("Running", false);
                    playerState = PlayerState.Idle;
                    break;
                }
                break;

            case PlayerState.Attack:
                if (InputManager.Instance.IsFireUp)
                {
                    anim.SetLayerWeight(1, 0);
                    anim.SetBool("Shooting", false);

                    if (!isDash && dir.magnitude < 0.5f)
                    {
                        // 이동하며 쏘다가 멈춘 경우 Running이 켜진 채 남아 이동 애니메이션이 계속 재생됐다.
                        anim.SetBool("Running", false);
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
                TryStartDash();
                break;
        }

        //anim.SetLayerWeight(1, Mathf.Lerp(0, 1, Time.deltaTime * 2.0f));
        LookAround();
        Gravity();
        UpdateJumpAnimation();

        UpdateMoveAnimation();

        healthBar.SetHealth(currentHP);
    }

    // 입력은 Raw 값(즉시 0/1)이므로, 블렌드 트리 파라미터만 감쇠를 줘서 애니메이션이 튀지 않게 한다.
    private void UpdateMoveAnimation()
    {
        anim.SetFloat("horizontal", hzInput, animDampTime, Time.deltaTime);
        anim.SetFloat("vertical", vInput, animDampTime, Time.deltaTime);
    }

    // 사망 처리는 Die 상태에 진입할 때 한 번만 한다. 매 프레임 이벤트를 보내면 씬 로드가 중복 요청된다.
    private void EnterDie()
    {
        playerState = PlayerState.Die;
        StopAllCoroutines();
        isDash = false;

        anim.SetLayerWeight(1, 0);
        anim.SetBool("Shooting", false);
        anim.SetBool("Running", false);
        anim.SetTrigger("Die");
        healthBar.SetHealth(0);

        OnPlayerDeath?.Invoke();
    }

    /// <summary>
    /// duration초 동안 이동·사격·대시를 막는다. 진행 중인 대시는 취소하고 사격·달리기 애니메이션을 끈다.
    /// </summary>
    public void Stun(float duration)
    {
        if (playerState == PlayerState.Die) return;

        stunEndTime = Mathf.Max(stunEndTime, Time.time + duration);

        StopAllCoroutines();
        isDash = false;

        hzInput = 0f;
        vInput = 0f;
        dir = Vector3.zero;
        anim.SetLayerWeight(1, 0);
        anim.SetBool("Shooting", false);
        anim.SetBool("Running", false);
        anim.SetFloat("horizontal", 0f);
        anim.SetFloat("vertical", 0f);

        // 경직이 끝나면 Idle에서 현재 입력을 다시 읽어 상태를 정한다.
        playerState = PlayerState.Idle;
    }

    // 카메라 기준 수평 이동 방향. 카메라 피치가 섞이지 않도록 수평으로 투영한다.
    private Vector3 GetCameraRelativeDirection(float x, float z)
    {
        Vector3 lookForward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z).normalized;
        Vector3 lookRight = new Vector3(cameraArm.right.x, 0f, cameraArm.right.z).normalized;
        return lookForward * z + lookRight * x;
    }

    private void Move()
    {
        hzInput = InputManager.Instance.Horizontal;
        vInput = InputManager.Instance.Vertical;

        dir = new Vector3(hzInput, 0, vInput);

        if (isDash)
        {
            DashMove();
            return;
        }

        if (dir != Vector3.zero)
        {
            Vector3 lookForward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z).normalized;
            Vector3 moveDir = GetCameraRelativeDirection(dir.x, dir.z);

            Quaternion targetRotation;

            if (InputManager.Instance.IsFire)
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
    }

    // 대시 시작. 이전에는 매 프레임 코루틴을 만들었고(대부분 즉시 종료) 방향을 시작 시점에 고정했다.
    private void TryStartDash()
    {
        if (isDash || !InputManager.Instance.IsDash) return;

        Vector3 inputDir = GetCameraRelativeDirection(InputManager.Instance.Horizontal, InputManager.Instance.Vertical);
        // 방향 입력 없이 대시하면 캐릭터가 보는 방향으로 나간다(이전: 방향이 0이라 제자리에 1초간 묶였음).
        dashDirection = inputDir.sqrMagnitude > 0.01f ? inputDir.normalized : FlatForward(characterBody.forward);

        isDash = true;
        dashEndTime = Time.time + rollDuration;
        anim.SetTrigger("Dash");
    }

    // 대시 중 이동. 방향 입력이 있으면 그 방향으로 즉시 바꿔 대시가 끝나기 전에도 방향 전환이 반영되게 한다.
    private void DashMove()
    {
        Vector3 inputDir = GetCameraRelativeDirection(dir.x, dir.z);
        if (inputDir.sqrMagnitude > 0.01f) dashDirection = inputDir.normalized;

        // 사격 중에는 몸을 카메라 방향으로 유지하므로(AttackMovement) 대시 방향으로 돌리지 않는다.
        if (playerState != PlayerState.Attack && dashDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dashDirection);
            characterBody.rotation = Quaternion.Slerp(characterBody.rotation, targetRotation, Time.deltaTime * rotationSpeed * 2f);
        }

        cc.Move(dashDirection * moveSpeed * rollSpeedMultiplier * Time.deltaTime);
        lastInputTime = Time.time;

        if (Time.time >= dashEndTime) isDash = false;
    }

    private static Vector3 FlatForward(Vector3 v)
    {
        v.y = 0f;
        return v.sqrMagnitude > 0.0001f ? v.normalized : Vector3.forward;
    }

    // 수직 이동은 이곳에서만 한다. 예전에는 Jump()도 cc.Move를 호출해 Move/Attack 상태에서 수직 이동이 두 번 적용됐다.
    private void Gravity()
    {
        if (isGrounded && velocity.y < 0f)
            velocity.y = -groundStickSpeed;
        else
            velocity.y += gravity * Time.deltaTime;

        CollisionFlags flags = cc.Move(velocity * Time.deltaTime);
        isGrounded = (flags & CollisionFlags.Below) != 0;
    }

    // 점프 속도만 설정하고, 실제 이동은 같은 프레임의 Gravity()에서 처리한다.
    private void Jump()
    {
        if (isGrounded && velocity.y <= 0f && InputManager.Instance.IsJump)
        {
            velocity.y = jumpSpeed;
            isGrounded = false;

            // 트리거(AnyState → Jump)는 Idle↔WalkFront 전이 중에는 전이가 끝날 때까지 기다려서,
            // 이동을 시작하자마자 점프하면 애니메이션이 물리보다 늦게 시작됐다. 상태를 직접 재생한다.
            isJumpAnimating = true;
            hasJumpLanded = false;
            jumpAnimTime = 0f;
            jumpStartTime = Time.time;
            anim.SetFloat("JumpTime", 0f);
            anim.Play(JumpStateHash, 0, 0f);
        }
    }

    // Jump 상태의 재생 위치를 물리 상태에 맞춘다. Gravity() 다음에 호출해야 이번 프레임의 접지 결과를 쓴다.
    //  - 공중: (지난 시간 / 전체 체공 시간 예측)의 비율로 클립의 0 ~ jumpTouchdownTime을 끊김 없이 재생한다.
    //          예전에는 다리를 모은 자세에서 멈췄다가 착지 직전에 다시 재생해서 공중에서 멈칫했다.
    //  - 착지 후: 착지 동작을 시간대로 재생하고, Animator 전이(JumpTime 조건)가 이동/정지 상태로 넘긴다.
    private void UpdateJumpAnimation()
    {
        if (!isJumpAnimating) return;

        // 대시·레비테이션·사망 등으로 Jump 상태를 벗어났으면 더 이상 갱신하지 않는다.
        bool inJump = anim.GetCurrentAnimatorStateInfo(0).fullPathHash == JumpStateHash
                   || (anim.IsInTransition(0) && anim.GetNextAnimatorStateInfo(0).fullPathHash == JumpStateHash);
        if (!inJump && jumpAnimTime > 0f)
        {
            isJumpAnimating = false;
            return;
        }

        float step = Time.deltaTime / jumpClipLength;

        if (!hasJumpLanded && isGrounded && velocity.y <= 0f)
        {
            hasJumpLanded = true;
            // 예상보다 일찍 착지했으면(턱 위로 점프 등) 착지 시점으로 바로 맞춘다.
            jumpAnimTime = Mathf.Max(jumpAnimTime, jumpTouchdownTime);
        }

        if (hasJumpLanded)
        {
            jumpAnimTime += step;
        }
        else
        {
            float elapsed = Time.time - jumpStartTime;
            float target;
            if (TryGetTimeToLand(out float timeToLand) && elapsed + timeToLand > 0.0001f)
                target = jumpTouchdownTime * elapsed / (elapsed + timeToLand);
            else
                target = Mathf.Min(elapsed / jumpClipLength, jumpFallbackHoldTime);

            // 뒤로 가지 않고, 예측이 바뀌어도 한 프레임에 너무 많이 넘어가지 않게 한다.
            float maxNext = jumpAnimTime + step * jumpMaxPlaySpeed;
            jumpAnimTime = Mathf.Clamp(target, jumpAnimTime, Mathf.Min(maxNext, jumpTouchdownTime));
        }

        jumpAnimTime = Mathf.Min(jumpAnimTime, 1f);
        anim.SetFloat("JumpTime", jumpAnimTime);

        if (jumpAnimTime >= 1f) isJumpAnimating = false;
    }

    // 현재 수직 속도와 중력으로 땅에 닿기까지 남은 시간을 구한다(상승 중에도 유효). 아래에 땅이 없으면 false.
    private bool TryGetTimeToLand(out float timeToLand)
    {
        timeToLand = 0f;
        Vector3 center = transform.TransformPoint(cc.center);
        float halfHeight = cc.height * 0.5f * transform.lossyScale.y;
        int mask = groundMask & ~(1 << gameObject.layer);
        if (!Physics.Raycast(center, Vector3.down, out RaycastHit hit, 50f, mask, QueryTriggerInteraction.Ignore))
            return false;

        float distance = Mathf.Max(0f, hit.distance - halfHeight);
        float fallSpeed = -velocity.y;
        float g = -gravity;
        // distance = fallSpeed * t + 0.5 * g * t^2
        timeToLand = (-fallSpeed + Mathf.Sqrt(fallSpeed * fallSpeed + 2f * g * distance)) / g;
        return true;
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
            // 최대 HP와 현재 HP를 함께 올리고, HP바의 기준 최대치도 갱신한다.
            maxHP += 20;
            currentHP += 20;
            healthBar.SetMaxHealth(maxHP);
            healthBar.SetHealth(currentHP);
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
    public void SetHP(float hp)
    {
        currentHP = hp;
        if (healthBar != null) healthBar.SetHealth(currentHP);
    }

    public float GetMaxHP() { return maxHP; }

    public void SetMaxHP(float hp)
    {
        maxHP = hp;
        if (healthBar != null) healthBar.SetMaxHealth(maxHP);
    }


    private void LookAround()
    {
        Vector2 mouseDelta = new Vector2(InputManager.Instance.MouseX, InputManager.Instance.MouseY);
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

        // 총알은 총구에서 조준점으로 날아가므로 조준점이 크로스헤어가 가리키는 지점과 정확히 같아야 한다.
        // - 적 탐지 범위 같은 트리거에 걸리지 않도록 트리거를 무시한다.
        // - 하늘처럼 맞는 것이 없으면 이전 지점에 남지 않도록 레이 방향의 먼 점을 쓴다.
        // - 보간(Lerp)을 쓰면 시점을 돌린 직후 몇 발이 이전 위치로 날아가므로 즉시 갱신한다.
        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, aimMask, QueryTriggerInteraction.Ignore))
            aimPos.position = hit.point;
        else
            aimPos.position = ray.GetPoint(maxAimDistance);
    }
}