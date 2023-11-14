using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Menu : MonoBehaviour
{
    [SerializeField] GameObject menuUI;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!GameManager.isMenuOpen)
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
        GameManager.isMenuOpen = true;
        menuUI.SetActive(true);
        Time.timeScale = 0f;
    }

    void ClosMenu()
    {
        GameManager.isMenuOpen = false;
        menuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ClickSave()
    {
        //if(EventSystem.current.IsPointerOverGameObject())
        //{

        //}
        Debug.Log("ji");
    }

    public void ClickLoad()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("hi");
        }
    }

    public void ClickReturn()
    {
        GameManager.isMenuOpen = false;
        Time.timeScale = 1f;
    }
}
