using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Slot : MonoBehaviour
{
    public Item item;
    public int itemCount;
    public Image itemImange;

    [SerializeField] private TMP_Text textCount;
    [SerializeField] private GameObject CountImage;

    private void SetColor(float alpha)
    {
        Color color = itemImange.color;
        color.a = alpha;
        itemImange.color = color;
    }

    public void AddItem(Item itemIn, int count = 1)
    {
        item = itemIn;
        itemCount = count;
        itemImange.sprite = item.itemImage;

        if (item.itemType == Item.ItemType.Buff)
        {
            CountImage.SetActive(true);
            textCount.text = itemCount.ToString();
        }
        else
        {
            textCount.text = "0";
            CountImage.SetActive(false);
        }


        SetColor(1);
    }

    public void SetSlotCount(int count)
    {
        itemCount += count;
        textCount.text = itemCount.ToString();

        if (itemCount <= 0) ClearSlot();
    }

    public void ClearSlot()
    {
        item = null;
        itemCount = 0;
        itemImange.sprite = null;
        SetColor(0);

        textCount.text = "0";
        CountImage.SetActive(false);
    }
}
