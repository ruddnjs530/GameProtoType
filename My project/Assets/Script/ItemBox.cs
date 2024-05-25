using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemBox : MonoBehaviour
{
    private bool isPlayerEnter = false;

    [SerializeField] private TextMeshPro itemPriceUI;
    [SerializeField] private GameObject[] items;

    private int itemPrice = 15;
    // Start is called before the first frame update
    void Start()
    {
        itemPriceUI.text = itemPrice.ToString() + " $";
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayerEnter && Input.GetKeyDown(KeyCode.E) && GameManager.Instance.money >= itemPrice)
        {
            GameObject item = Instantiate(items[Random.Range(0,items.Length)], transform.position, Quaternion.identity);

            if (item.GetComponent<TurretObject>() != null)
            {
                GameManager.Instance.AddTurret(item);
            }
            if (item.GetComponent<DroneObject>() != null)
            {
                GameManager.Instance.AddTurret(item);
            }

            //GameObject turret = item.GetComponent<TurretObject>().gameObject;
            //if (turret != null)
            //{
            //    GameManager.Instance.AddTurret(turret);
            //}

            //GameObject drone = item.GetComponent<DroneObject>().gameObject;
            //if (drone != null)
            //{
            //    GameManager.Instance.AddDrone(drone);
            //}

            GameManager.Instance.DecreaseMoney(itemPrice);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerEnter = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerEnter = false;
    }

}
