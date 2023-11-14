using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class SaveData
{
    public Vector3 playerPos;   // 플레이어 위치
    public Quaternion playerRot;   // 플레이어 회전값 (유튜브에서는 Vector3 변수를 사용하고 transform.eulerAngles를 받았음)
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

        Debug.Log("저장완료 " + content);
    }

    public void LoadData()
    {
        if (File.Exists(Application.dataPath + "/SaveFile.json"))   // 파일이 있을 경우만 실행
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
        else Debug.Log("저장된 파일이 없습니다.");
        
    }
}
