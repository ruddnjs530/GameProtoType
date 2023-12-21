using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PotalObject : MonoBehaviour
{
    [SerializeField] GameObject e;
    [SerializeField] GameObject countDown;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player" && !GameManager.Instance.isEnemyWave && Input.GetKey(KeyCode.E))
        {
            GameManager.Instance.isEnemyWave = true;
            e.SetActive(false);
            countDown.SetActive(true);
        }

        if (other.gameObject.tag == "Player" && GameManager.Instance.isGameClear)
            SceneManager.LoadScene("ClearScene");
    }
}
