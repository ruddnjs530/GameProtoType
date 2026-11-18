using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class SaveData
{
    public int money;
    public float hp;
    public float maxHp;
    public int droneCount;
    public List<int> inventoryArrayNumber = new List<int>();
    public List<string> inventoryItemName = new List<string>();
    public List<int> inventoryItemNumber = new List<int>();
}
public class SaveAndLoad : MonoBehaviour
{
    private SaveData saveData = new SaveData();
    private Player player;
    private Inventory inventory;

    public void SaveData()
    {
        player = FindObjectOfType<Player>();
        inventory = FindObjectOfType<Inventory>();

        saveData.hp = player.GetHP();
        saveData.maxHp = player.GetMaxHP();
        saveData.money = GameManager.Instance.money;
        saveData.droneCount = SpawnManager.Instance.GetDroneCount();

        Slot[] slots = inventory.GetSlots();
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item != null)
            {
                saveData.inventoryArrayNumber.Add(i);
                saveData.inventoryItemName.Add(slots[i].item.itemName);
                saveData.inventoryItemNumber.Add(slots[i].itemCount);
            }
        }

        string content = JsonUtility.ToJson(saveData);

        File.WriteAllText(Application.dataPath + "/SaveFile.json", content);

        Debug.Log("데이터 저장 " + content);
    }

    public void LoadData()
    {
        if (File.Exists(Application.dataPath + "/SaveFile.json")) 
        {
            string content = File.ReadAllText(Application.dataPath + "/SaveFile.json");

            saveData = JsonUtility.FromJson<SaveData>(content);

            player = FindObjectOfType<Player>();
            inventory = FindObjectOfType<Inventory>();

            player.SetHP(saveData.hp);
            player.SetMaxHP(saveData.maxHp);
            GameManager.Instance.money = saveData.money;
            UIManager.Instance.UpdateMoneyText(GameManager.Instance.money);
            SpawnManager.Instance.SetDrone(saveData.droneCount, player);

            for (int i = 0; i < saveData.inventoryItemName.Count; i++)
            {
                inventory.LoadToInven(saveData.inventoryArrayNumber[i], saveData.inventoryItemName[i], saveData.inventoryItemNumber[i]);
            }
        }
        else Debug.Log("데이터가 로드 되지 않음");
        
    }
}
