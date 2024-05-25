using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PotalObject : MonoBehaviour
{
    [SerializeField] GameObject e;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKey(KeyCode.E))
        {
            Player player = other.GetComponent<Player>();
            Inventory inventory = player.GetComponent<Inventory>();
            List<GameObject> turrets = GameManager.Instance.turrets;
            List<GameObject> drones = GameManager.Instance.drones;

            GameManager.Instance.SavePlayerStatus(player, inventory, turrets, drones);

            SceneManager.LoadScene("BossScene");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Player player = FindObjectOfType<Player>();
        Inventory inventory = player.GetComponent<Inventory>();
        List<GameObject> turrets = GameManager.Instance.turrets;
        List<GameObject> drones = GameManager.Instance.drones;

        GameManager.Instance.InitializeBoss();
        GameManager.Instance.LoadPlayerStatus(player, inventory, turrets, drones);

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
