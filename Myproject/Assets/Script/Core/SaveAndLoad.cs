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
    // 에디터에서 Assets 폴더에 쓰면 에셋 임포트·Git 변경이 생기고, 빌드에서는 쓰기 권한이 없을 수 있어 persistentDataPath를 쓴다.
    private static string SavePath => Path.Combine(Application.persistentDataPath, "SaveFile.json");

    public void SaveData()
    {
        Player player = FindObjectOfType<Player>();
        Inventory inventory = FindObjectOfType<Inventory>();
        if (player == null || inventory == null)
        {
            Debug.LogWarning("저장 실패: Player 또는 Inventory가 없음");
            return;
        }

        // 저장할 때마다 새로 만든다. 필드에 두고 재사용하면 인벤토리 목록이 저장할 때마다 누적된다.
        SaveData saveData = new SaveData
        {
            hp = player.GetHP(),
            maxHp = player.GetMaxHP(),
            money = GameManager.Instance.money,
            droneCount = SpawnManager.Instance.GetDroneCount(),
        };

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
        File.WriteAllText(SavePath, content);

        Debug.Log("데이터 저장 " + SavePath + "\n" + content);
    }

    public void LoadData()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("데이터가 로드 되지 않음: 세이브 파일 없음");
            return;
        }

        SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
        Player player = FindObjectOfType<Player>();
        Inventory inventory = FindObjectOfType<Inventory>();
        if (saveData == null || player == null || inventory == null)
        {
            Debug.LogWarning("로드 실패: 세이브 데이터 또는 Player/Inventory가 없음");
            return;
        }

        // 최대 HP를 먼저 설정해야 현재 HP와 HP바 비율이 올바르게 계산된다.
        player.SetMaxHP(saveData.maxHp);
        player.SetHP(saveData.hp);
        GameManager.Instance.money = saveData.money;
        UIManager.Instance.UpdateMoneyText(GameManager.Instance.money);
        SpawnManager.Instance.SetDrone(saveData.droneCount, player);

        // 기존 슬롯을 비운 뒤 채운다. 비우지 않으면 이전 내용과 섞인다.
        inventory.ClearAllSlots();
        for (int i = 0; i < saveData.inventoryItemName.Count; i++)
        {
            inventory.LoadToInven(saveData.inventoryArrayNumber[i], saveData.inventoryItemName[i], saveData.inventoryItemNumber[i]);
        }
    }
}
