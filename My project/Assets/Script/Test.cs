using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//public enum PlayerState { Idle, Run, Jump, StopAttack, MoveAttack, DiveRoll, Die }
public class Test : MonoBehaviour
{
    Animator anim;

    Transform target;

    NavMeshAgent agent;
    public AnimationCurve HeightCurve;
    float jumpSpeed = 2.0f;

    float timer;

    //List<Skill> skills = new List<Skill>
    //{
    //    new Skill("LaserAttack", 5f, 3),
    //    new Skill("EeightWayAttack", 10f, 2),
    //    new Skill("JumpAttack", 15f, 1)
    //};

    void Start()
    {
        //agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        //LookAround();
        //Move();

        //agent.SetDestination(target.position);
        //agent.speed = 2f;

        timer += Time.deltaTime;
        if (timer > 3f)
        {
            
            StartCoroutine(JumpAttack()); //target.position
            timer = 0.0f;
        }
    }

    IEnumerator JumpAttack() //Vector3 targetPosition
    {
        Debug.Log("hi");
        Vector3 startingPos = this.transform.position;
        for (float time = 0; time < 1; time += Time.deltaTime * jumpSpeed)
        {
            transform.position = Vector3.Lerp(transform.position, target.position, time) + Vector3.up * HeightCurve.Evaluate(time);

            yield return null;
        }

        //float distance = Vector3.Distance(this.transform.position, target.position);
        //if (distance <= 2f) target.GetComponent<Player>().TakeDamage(jumpDamage);
    }

    //void ExecuteSkill(Skill skill)
    //{
    //    switch (skill.name)
    //    {
    //        case "LaserAttack":
    //            StartCoroutine(LaserAttack());
    //            break;
    //        case "EeightWayAttack":
    //            EeightWayAttack();
    //            break;
    //        case "JumpAttack":
    //            StartCoroutine(JumpAttack(target.position));
    //            break;
    //    }
    //}

    //bool CheckSkillConditions(Skill skill)
    //{
    //    if (skill.name == "EightWayAttack" || skill.name == "JumpAttack")
    //    {
    //        return hp <= 30;
    //    }
    //    else if (skill.name == "LaserAttack")
    //    {
    //        return canAttack;
    //    }
    //    return false;
    //}


    

    //private void LookAround()
    //{
    //    Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    //    Vector3 camAngle = cameraArm.rotation.eulerAngles;

    //    float x = camAngle.x - mouseDelta.y;
    //    if (x < 180f)
    //    {
    //        x = Mathf.Clamp(x, -1f, 70f);
    //    }
    //    else
    //    {
    //        x = Mathf.Clamp(x, 335f, 361f);
    //    }

    //    cameraArm.rotation = Quaternion.Euler(camAngle.x - mouseDelta.y, camAngle.y + mouseDelta.x, camAngle.z);

    //    Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
    //    Ray ray = Camera.main.ScreenPointToRay(screenCenter);

    //    if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, aimMask))
    //        aimPos.position = Vector3.Lerp(aimPos.position, hit.point, aimSpeed * Time.deltaTime);
    //}

    //private void Move()
    //{
    //    hzInput = Input.GetAxis("Horizontal");
    //    vInput = Input.GetAxis("Vertical");

    //    dir = new Vector3(hzInput, 0, vInput);

    //    if (dir != Vector3.zero)
    //    {
    //        Vector3 lookForward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z).normalized;
    //        Vector3 lookRight = new Vector3(cameraArm.right.x, 0f, cameraArm.right.z).normalized;
    //        Vector3 moveDir = lookForward * dir.z + lookRight * dir.x;

    //        float targetAngle = Mathf.Atan2(hzInput, vInput) * Mathf.Rad2Deg;

    //        Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);

    //        if (Input.GetMouseButton(0))
    //        {
    //            characterBody.forward = lookForward; //lookForward; //moveDir;

    //            cc.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
    //        }
    //        else
    //        {
    //            characterBody.forward = moveDir; //lookForward; //moveDir;

    //            cc.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
    //        }
    //    }
    //    anim.SetBool("Running", true);
    //    anim.SetFloat("horizontal", hzInput);
    //    anim.SetFloat("vertical", vInput);
    //}
}

public class Skill
{
    public string name;
    public float coolTime;
    public float currentCoolTime;
    public int priority;

    public Skill(string name, float coolTime, int priority)
    {
        this.name = name;
        this.coolTime = coolTime;
        this.priority = priority;
        this.currentCoolTime = 0;
    }

    public void UpdateCoolTime(float deltaTime)
    {
        if (currentCoolTime > 0)
        {
            currentCoolTime -= deltaTime;
        }
    }

    public bool IsReady()
    {
        return currentCoolTime <= 0;
    }
}