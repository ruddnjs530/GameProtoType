using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static bool inventoryActivated = false;

    [SerializeField] GameObject inventoryBase;
    [SerializeField] GameObject slotParent;

    Slot[] slots;

    // Start is called before the first frame update
    void Start()
    {
        slots = slotParent.GetComponentsInChildren<Slot>();
    }

    // Update is called once per frame
    void Update()
    {
        OpenInventory();
    }

    void OpenInventory()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            inventoryActivated = !inventoryActivated;

            if (inventoryActivated) inventoryBase.SetActive(true);
            else inventoryBase.SetActive(false);
        }
    }

    public void AcquireItem(Item itemIn, int count)
    {
        if (Item.ItemType.Equipment != itemIn.itemType)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if(slots[i].item != null)
                {
                    if (slots[i].item.itemName == itemIn.itemName)
                    {
                        slots[i].SetSlotCount(count);
                    }
                } 
            }
        }

        for (int i = 0; i <slots.Length; i++)
        {
            if (slots[i].item == null)
            {
                slots[i].AddItem(itemIn, count);
                return;
            }
        }
    }
}
