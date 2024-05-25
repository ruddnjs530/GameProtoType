using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus
{
    public float currentHP;
    public float maxHP;
    public int money;
    public List<InventoryItem> inventoryItems;
    public List<GameObject> turrets;
    public List<GameObject> drones;

    public PlayerStatus(float currentHP, float maxHP, int money, List<InventoryItem> inventoryItems, List<GameObject> turrets, List<GameObject> drones)
    {
        this.currentHP = currentHP;
        this.maxHP = maxHP;
        this.money = money;
        this.inventoryItems = inventoryItems;
        this.turrets = turrets;
        this.drones = drones;
    }
}
