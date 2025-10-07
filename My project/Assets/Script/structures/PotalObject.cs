using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PotalObject : MonoBehaviour
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

            GameObject gameManager = GameObject.Find("GameManager");
            SaveAndLoad save = gameManager.GetComponent<SaveAndLoad>();
            save.SaveData();

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene("BossScene");
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "BossScene") return;
        Player player = GameObject.Find("Player").GetComponent<Player>();
        Inventory inventory = FindObjectOfType<Inventory>();
        GameManager.Instance.InitializeBoss();
        //GameManager.Instance.LoadPlayerStatus(player, inventory, GameManager.Instance.playerStatus);

        GameObject gameManager = GameObject.Find("GameManager");
        SaveAndLoad save = gameManager.GetComponent<SaveAndLoad>();
        save.LoadData();
    }
}
