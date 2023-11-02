using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class NomalEnemy : Enemy
//{
//    private float attackDistance = 5f;
//    private float attackDamage = 3f;

//    public Transform targetTransform;
//    protected override void Attack()
//    {
//        //Debug.Log("attack");

//        targetTransform.gameObject.GetComponent<Player>().TakeDamage(attackDamage);
//    }

//    private void CanAttack()
//    {
//        Collider[] target = Physics.OverlapSphere(transform.position, attackDistance, targetMask);
//        for (int i = 0; i < target.Length; i++)
//        {
//            Transform targetTf = target[i].transform;
//            if (targetTf.tag == "Player")
//            {
//                //Debug.Log("can attack in tag " + canAttack);
//                canAttack = true;
//                transform.LookAt(targetTf);
//                agentTarget = targetTf;
//            }
//            else
//            {
//                canAttack = false;
//                //Debug.Log("can attack in else " + canAttack);
//            }
//        }
//    }

//    protected override void Update()
//    {
//        base.Update();
//        CanAttack();

//        //switch (enemyState)
//        //{
//        //    case EnemyState.Idle:
//        //        LookAround();
//        //        currentTime += Time.deltaTime;

//        //        if (currentTime >= 3f && !seePlayer)
//        //            enemyState = EnemyState.SimpleMove;
//        //        else if (seePlayer)
//        //            enemyState = EnemyState.Chase;
//        //        else if (hp <= 0) enemyState = EnemyState.Die;
//        //        break;

//        //    case EnemyState.SimpleMove:
//        //        ReSetDestination();
//        //        SimpleMove();
//        //        currentTime = 0;

//        //        if (hp <= 0) enemyState = EnemyState.Die;
//        //        else enemyState = EnemyState.Idle;
//        //        break;

//        //    case EnemyState.Chase:
//        //        Chase();

//        //        if (hp <= 0) enemyState = EnemyState.Die;
//        //        else if (canAttack) enemyState = EnemyState.Attack;
//        //        else if (!seePlayer) enemyState = EnemyState.Idle;
//        //        break;

//        //    case EnemyState.Attack:
//        //        this.Attack();

//        //        if (!canAttack) enemyState = EnemyState.Idle;
//        //        break;

//        //    case EnemyState.Die:
//        //        Die();
//        //        break;
//        //}
//    }

//    //private void OnTriggerEnter(Collider other)
//    //{
//    //    if (other.transform.tag == "Player")
//    //    {
//    //        canAttack = true;
//    //        target = other.transform.gameObject;
//    //    }
//    //}

//    //private void OnTriggerExit(Collider other)
//    //{
//    //    if (other.transform.tag == "Player")
//    //    {
//    //        canAttack = false;
//    //        target = null;
//    //    }
//    //}
//}
