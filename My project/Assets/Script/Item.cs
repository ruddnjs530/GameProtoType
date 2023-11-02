using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "New Item/item")]
public class Item : ScriptableObject
{
   public enum ItemType  // 아이템 유형
    {
        Buff,
        Equipment,
    }

    public string itemName; 
    public ItemType itemType; 
    public Sprite itemImage;
    public GameObject itemPrefab;
}
