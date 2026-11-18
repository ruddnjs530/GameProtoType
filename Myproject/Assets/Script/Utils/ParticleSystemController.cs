using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemController : MonoBehaviour
{
    [SerializeField] private ParticleSystem particleSystem2;

    private void Awake()
    {
        particleSystem2 = GetComponent<ParticleSystem>();
    }

    public void PlayAndDestroy()
    {
        if (particleSystem2 != null)
        {
            particleSystem2.Play();
            Destroy(gameObject, particleSystem2.main.duration);
        }
    }
}
