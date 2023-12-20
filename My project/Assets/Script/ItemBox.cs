using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemBox : MonoBehaviour
{
    bool isPlayerEnter = false;

    [SerializeField] TextMeshPro itemPriceUI;
    [SerializeField] GameObject[] items;

    int itemPrice = 15;
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
            Instantiate(items[Random.Range(0,items.Length)], transform.position, Quaternion.identity);
            GameManager.Instance.DecreaseMoney(itemPrice);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player") isPlayerEnter = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player") isPlayerEnter = false;
    }

}
