using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject menuUI;
    [SerializeField] private SaveAndLoad saveAndLoad;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!GameManager.Instance.isMenuOpen)
            {
                OpenMenu();
            }
            else
            {
                ClosMenu();
            }
        }
    }

    private void OpenMenu()
    {
        GameManager.Instance.isMenuOpen = true;
        menuUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ClosMenu()
    {
        GameManager.Instance.isMenuOpen = false;
        menuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ClickSave()
    {
        saveAndLoad.SaveData();
    }


    public void ClickExit()
    {
        Application.Quit();
    }
}
