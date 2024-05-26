using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus
{
    public float currentHP;
    public float maxHP;
    public int money;
    public List<InventoryItem> inventoryItems;


    public PlayerStatus(float currentHP, float maxHP, int money, List<InventoryItem> inventoryItems)
    {
        this.currentHP = currentHP;
        this.maxHP = maxHP;
        this.money = money;
        this.inventoryItems = inventoryItems;
    }
}
