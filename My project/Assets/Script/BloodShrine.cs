using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodShrine : MonoBehaviour
{
    float hp = 30f;
    int money = 10;
    GameObject player;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && player != null)
        {
            player.GetComponent<Player>().GivingHPAndGetMoney(hp, money);
            GameManager.Instance.UpdateMoney();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
            player = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
            player = null;
    }
}
