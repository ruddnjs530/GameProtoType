using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public enum PlayerState { Idle, Run, Jump, StopAttack, MoveAttack, DiveRoll, Die }
public class Test : MonoBehaviour
{
    float hzInput;
    float vInput;
    Vector3 dir;
    private float moveSpeed = 5f;//움직이는 속도

    // Start is called before the first frame update
    CharacterController cc;

    [SerializeField]
    private Transform characterBody;
    [SerializeField]
    private Transform cameraArm;

    public Transform aimPos;
    [SerializeField] float aimSpeed = 20;
    [SerializeField] LayerMask aimMask;

    Animator anim;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        LookAround();
        Move();
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

    private void Move()
    {
        hzInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");

        dir = new Vector3(hzInput, 0, vInput);

        if (dir != Vector3.zero)
        {
            Vector3 lookForward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z).normalized;
            Vector3 lookRight = new Vector3(cameraArm.right.x, 0f, cameraArm.right.z).normalized;
            Vector3 moveDir = lookForward * dir.z + lookRight * dir.x;

            float targetAngle = Mathf.Atan2(hzInput, vInput) * Mathf.Rad2Deg;

            Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);

            if (Input.GetMouseButton(0))
            {
                characterBody.forward = lookForward; //lookForward; //moveDir;

                cc.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
            }
            else
            {
                characterBody.forward = moveDir; //lookForward; //moveDir;

                cc.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
            }
        }
        anim.SetBool("Running", true);
        anim.SetFloat("horizontal", hzInput);
        anim.SetFloat("vertical", vInput);
    }
}