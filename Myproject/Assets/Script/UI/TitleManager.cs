using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    private SaveAndLoad saveAndLoad;

    // Title·Clear·GameOver 씬의 버튼만 담당하므로 싱글톤·DDOL이 필요 없다.
    // 예전에는 싱글톤이라 Title Canvas가 남고, Clear/GameOver 씬의 Canvas(버튼 포함)가 파괴됐다.

    private bool isLoading = false;

    // 동기 LoadScene은 로딩하는 동안(재시작 후 최대 2초) 화면 갱신이 멈춰, 그 사이 빈 화면이 보일 수 있었다.
    // 비동기로 불러오면 새 씬이 준비될 때까지 Title 화면이 계속 그려지고, 준비되면 바로 넘어간다.
    public void ClickStart()
    {
        if (isLoading) return; // 버튼을 여러 번 눌러도 한 번만 불러온다
        isLoading = true;
        StartCoroutine(LoadGameRoutine());
    }

    private IEnumerator LoadGameRoutine()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync("SampleScene");
        while (!operation.isDone) yield return null;
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
