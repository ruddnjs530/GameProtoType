using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class SaveData
{
    public Vector3 playerPos;   // �÷��̾� ��ġ
    public Quaternion playerRot;   // �÷��̾� ȸ���� (��Ʃ�꿡���� Vector3 ������ ����ϰ� transform.eulerAngles�� �޾���)
    public List<int> inventoryArrayNumber = new List<int>();
    public List<string> inventoryItemName = new List<string>();
    public List<int> inventoryItemNumber = new List<int>();
}
public class SaveAndLoad : MonoBehaviour
{
    private SaveData saveData = new SaveData();
    private Player player;
    private Inventory inventory;
    // Application.dataPath + "/SaveFile.json";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SaveData()
    {
        player = FindObjectOfType<Player>();
        inventory = FindObjectOfType<Inventory>();

        saveData.playerPos = player.transform.position;
        saveData.playerRot = player.transform.rotation;

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

        Debug.Log("����Ϸ� " + content);
    }

    public void LoadData()
    {
        if (File.Exists(Application.dataPath + "/SaveFile.json"))   // ������ ���� ��츸 ����
        {
            string content = File.ReadAllText(Application.dataPath + "/SaveFile.json");

            saveData = JsonUtility.FromJson<SaveData>(content);

            player = FindObjectOfType<Player>();
            inventory = FindObjectOfType<Inventory>();

            player.transform.position = saveData.playerPos;
            player.transform.rotation = saveData.playerRot;

            for (int i = 0; i < saveData.inventoryItemName.Count; i++)
            {
                inventory.LoadToInven(saveData.inventoryArrayNumber[i], saveData.inventoryItemName[i], saveData.inventoryItemNumber[i]);
            }
        }
        else Debug.Log("����� ������ �����ϴ�.");
        
    }
}
