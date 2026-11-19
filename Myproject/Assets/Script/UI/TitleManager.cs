using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    private SaveAndLoad saveAndLoad;

    // Title·Clear·GameOver 씬의 버튼만 담당하므로 싱글톤·DDOL이 필요 없다.
    // 예전에는 싱글톤이라 Title Canvas가 남고, Clear/GameOver 씬의 Canvas(버튼 포함)가 파괴됐다.

    public void ClickStart()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // Clear/GameOver 화면의 재시작 버튼. 이전 판의 오브젝트는 씬 전환 전에 GameManager.EndRun()에서 이미 정리된다.
    public void ClickTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Title");
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
