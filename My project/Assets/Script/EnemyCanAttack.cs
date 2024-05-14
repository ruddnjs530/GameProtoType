using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCanAttack : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            //if (!this.transform.GetComponentInParent<Enemy>().IsAgentTargetExist()) 
            //    this.transform.GetComponentInParent<Enemy>().SetAgentTarget(other);
            this.transform.GetComponentInParent<Enemy>().LookAtTarget(other);

            this.transform.GetComponentInParent<Enemy>().canAttack= true;
            //Debug.Log("player in");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            this.transform.GetComponentInParent<Enemy>().canAttack = false;
        }
    }
}
