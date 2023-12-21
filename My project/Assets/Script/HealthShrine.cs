using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthShrine : MonoBehaviour
{
    [SerializeField] TextMesh text;
    [SerializeField] GameObject range;
    bool isPlayerEnter = false;
    int shrinePrice = 10;

    // Start is called before the first frame update
    void Start()
    {
        text.text = shrinePrice.ToString() + " $";
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayerEnter && Input.GetKeyDown(KeyCode.E) && GameManager.Instance.money >= shrinePrice)
        {
            Debug.Log("hi");
            GameManager.Instance.DecreaseMoney(shrinePrice);
            range.SetActive(true);
            text.gameObject.SetActive(false);

            StartCoroutine(healthRangeOff(10.0f));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player") isPlayerEnter = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player") isPlayerEnter = false;
    }

    IEnumerator healthRangeOff(float time)
    {
        yield return new WaitForSeconds(time);
        range.SetActive(false);
        text.gameObject.SetActive(true);
    }
}
