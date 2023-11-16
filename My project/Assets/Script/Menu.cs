using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Menu : MonoBehaviour
{
    [SerializeField] GameObject menuUI;
    [SerializeField] SaveAndLoad saveAndLoad;
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

    void OpenMenu()
    {
        GameManager.Instance.isMenuOpen = true;
        menuUI.SetActive(true);
        Time.timeScale = 0f;
    }

    void ClosMenu()
    {
        GameManager.Instance.isMenuOpen = false;
        menuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ClickSave()
    {
        saveAndLoad.SaveData();
    }

    public void ClickLoad()
    {
        //saveAndLoad.LoadData();
    }

    public void ClickExit()
    {
        Application.Quit();
    }
}
