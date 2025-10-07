using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static bool inventoryActivated = false;

    [SerializeField] GameObject inventoryBase;
    [SerializeField] GameObject slotParent;

    Slot[] slots;

    public Slot[] GetSlots() { return slots; }

    [SerializeField] private Item[] items;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

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
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Tab))
        {
            inventoryActivated = !inventoryActivated;

            if (inventoryActivated)
            {
                inventoryBase.SetActive(true);
                GameManager.Instance.isOpenInventory = true;
            }
            else
            {
                inventoryBase.SetActive(false);
                GameManager.Instance.isOpenInventory = false;
            }
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
                        return;
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

    public void LoadToInven(int arrayNum, string itemName, int itemNum)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].itemName == itemName)
                slots[arrayNum].AddItem(items[i], itemNum);
        }
    }

    public List<InventoryItem> GetItems()
    {
        List<InventoryItem> itemList = new List<InventoryItem>();
        foreach (var slot in slots)
        {
            if (slot.item != null)
            {
                itemList.Add(new InventoryItem(slot.item.itemName, slot.itemCount));
            }
        }
        return itemList;
    }

    public void SetItems(List<InventoryItem> itemList) // 지금은 인벤토리에 저장하는 것만 들어가 있음. set items 를 하면서 player 에 아이템 효과도 적용시켜줘야함.
    {
        foreach (var slot in slots)
        {
            slot.ClearSlot();
        }

        foreach (var inventoryItem in itemList)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].itemName == inventoryItem.itemName)
                {
                    for (int j = 0; j < slots.Length; j++)
                    {
                        if (slots[j].item == null)
                        {
                            slots[j].AddItem(items[i], inventoryItem.count);
                            break;
                        }
                    }
                    break;
                }
            }
        }
    }
}

public class InventoryItem
{
    public string itemName;
    public int count;

    public InventoryItem(string itemName, int count)
    {
        this.itemName = itemName;
        this.count = count;
    }
}