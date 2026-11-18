using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodShrine : MonoBehaviour
{
    private float hp = 30f;
    private int money = 10;
    private GameObject player;

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
        if (other.CompareTag("Player"))
            player = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            player = null;
    }
}
