using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCanAttack : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            this.transform.GetComponentInParent<Enemy>().canAttack= true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            this.transform.GetComponentInParent<Enemy>().canAttack = false;
        }
    }
}
