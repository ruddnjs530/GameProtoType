using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrineRangeManager : MonoBehaviour
{
    float hpValue = 3f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            StartCoroutine("IncreaseHP", other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            StopCoroutine("IncreaseHP");
        }
    }

    IEnumerator IncreaseHP(Collider col)
    {
        while (true)
        {    
            col.gameObject.GetComponent<Player>().IncreaseHP(hpValue);
            yield return new WaitForSeconds(1.0f);
        }
    }
}
