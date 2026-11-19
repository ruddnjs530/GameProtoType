using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalObject : MonoBehaviour
{
    [SerializeField] private GameObject e;
    private bool isTransitioning = false;
    List<GameObject> drones;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKey(KeyCode.E) && !isTransitioning)
        {
            isTransitioning = true;
            //Player player = other.GetComponent<Player>();
            //GameObject canvas = GameObject.Find("Canvas");
            //Inventory inventory = canvas.GetComponentInChildren<Inventory>();
            //drones = GameManager.Instance.drones;

            //GameManager.Instance.SavePlayerStatus(player, FindObjectOfType<Inventory>(), drones);

            // HP·돈·인벤토리는 Player·Inventory·GameManager가 씬을 넘어 유지되므로 세이브 파일을 거칠 필요가 없다.
            // 예전에는 여기서 저장·로드를 해서 포탈을 탈 때마다 사용자 세이브가 덮어써졌다.
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene("BossScene");
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "BossScene") return;
        // 포탈은 씬과 함께 파괴되므로 한 번 처리한 뒤 바로 해제한다. 남겨 두면 재시작 후 보스가 중복 생성된다.
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SpawnManager.Instance.InitializeBoss();

        // 드론은 씬과 함께 파괴되므로 보유 수만큼 보스 씬에서 다시 만든다.
        Player player = FindObjectOfType<Player>();
        if (player != null)
            SpawnManager.Instance.SetDrone(SpawnManager.Instance.GetDroneCount(), player);
    }
}
