using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    private SaveAndLoad saveAndLoad;

    public static TitleManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(this.gameObject);
    }

    public void ClickStart()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void ClickExit()
    {
        Application.Quit();
    }

    public void ClickLoad()
    {
        StartCoroutine(LoadCoroutine());
    }

    IEnumerator LoadCoroutine()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync("SampleScene");

        while(!operation.isDone)
        {
            yield return null;
        }

        saveAndLoad = FindObjectOfType<SaveAndLoad>();
        saveAndLoad.LoadData();
        Destroy(gameObject);
    }
}
