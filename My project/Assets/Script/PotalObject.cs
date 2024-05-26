using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PotalObject : MonoBehaviour
{
    [SerializeField] private GameObject e;
    private bool isTransitioning = false;
    List<GameObject> turrets;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKey(KeyCode.E) && !isTransitioning)
        {
            isTransitioning = true;
            Player player = other.GetComponent<Player>();
            GameObject canvas = GameObject.Find("Canvas");
            Inventory inventory = canvas.GetComponentInChildren<Inventory>();
            turrets = GameManager.Instance.drones;
            List<GameObject> drones = GameManager.Instance.drones;

            GameManager.Instance.SavePlayerStatus(player, inventory, turrets);

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene("BossScene");
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Player player = GameObject.Find("Player").GetComponent<Player>();

        //GameObject canvas = GameObject.Find("Canvas");
        //Inventory inventory = canvas.GetComponentInChildren<Inventory>();

        Inventory inventory = FindObjectOfType<Inventory>();
        GameManager.Instance.InitializeBoss();
        GameManager.Instance.LoadPlayerStatus(player, inventory, GameManager.Instance.playerStatus);
    }
}
