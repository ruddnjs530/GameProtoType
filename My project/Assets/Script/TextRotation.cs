using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextRotation : MonoBehaviour
{
    GameObject cam;

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera");
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = cam.transform.rotation;
    }
}
