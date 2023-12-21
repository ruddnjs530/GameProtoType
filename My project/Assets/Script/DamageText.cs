using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    float upSpeed;
    float alphaSpeed; // 투명해지게
    float destroyTime;
    TextMeshPro text;
    Color alpha;
    public int damage;

    GameObject cam;
    // Start is called before the first frame update
    void Start()
    {
        upSpeed = 2.0f;
        alphaSpeed = 2.0f;
        destroyTime = 1.0f;

        text = GetComponent<TextMeshPro>();
        text.text = damage.ToString();
        alpha = text.color;
        Invoke("DestroyText", destroyTime);

        cam = GameObject.Find("Main Camera");
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector3(0, upSpeed * Time.deltaTime, 0));

        alpha.a = Mathf.Lerp(alpha.a, 0, Time.deltaTime * alphaSpeed);
        text.color = alpha;

        transform.rotation = cam.transform.rotation;
    }

    void DestroyText()
    {
        Destroy(gameObject);
    }
}
