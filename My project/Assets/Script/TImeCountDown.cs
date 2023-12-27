using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TImeCountDown : MonoBehaviour
{
    TMP_Text text;

    private float countdownTime = 30f;
    private bool isCountingDown = true;

    [SerializeField] GameObject cam;

    private void Start()
    {
        text = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isCountingDown)
        {
            countdownTime -= Time.deltaTime;

            if (countdownTime <= 0f)
            {
                GameManager.Instance.isGameClear = true;
                countdownTime = 0f;
                isCountingDown = false;
            }
        }
        int minutes = Mathf.FloorToInt(countdownTime / 60f);
        int seconds = Mathf.FloorToInt(countdownTime % 60f);
        string formattedTime = string.Format("{0:00}:{1:00}", minutes, seconds);

        text.text = formattedTime;

        transform.rotation = cam.transform.rotation;
    }
}
