using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Weapon : MonoBehaviour
{
    [SerializeField] private float fireRate = 2.0f;
    private float fireRateTimer;

    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform barrelPos;
    [SerializeField] private float bulletVelocity;
    [SerializeField] private int bulletPerShot;

    private Mouse aim;

    private Light muzzleFlashLight;
    private ParticleSystem muzzleFlashParticles;
    private float lightIntensity;
    [SerializeField]
    private float lightReturnTime = 20;
    private Player player;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireSound;

    // Start is called before the first frame update
    void Start()
    {
        aim = GetComponentInParent<Mouse>();
        fireRateTimer = fireRate;
        muzzleFlashLight = GetComponentInChildren<Light>();
        lightIntensity = muzzleFlashLight.intensity;
        muzzleFlashLight.intensity = 0;
        muzzleFlashParticles = GetComponentInChildren<ParticleSystem>();
        player = GetComponentInParent<Player>();

        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ShouldFire()) Fire();
        muzzleFlashLight.intensity = Mathf.Lerp(muzzleFlashLight.intensity, 0, lightReturnTime * Time.deltaTime);
        
    }

    private bool ShouldFire()
    {
        fireRateTimer += Time.deltaTime;
        if (!GameManager.Instance.canPlayerMove) return false;
        if (fireRateTimer < fireRate) return false;
        if (Input.GetKey(KeyCode.LeftShift)) return false;
        if (Input.GetMouseButton(0))
        {
            Vector3 lookDirection = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized;
            Vector3 bodyDirection = player.characterBody.forward;

            float alignment = Vector3.Dot(lookDirection, bodyDirection);
            if (alignment > 0.95f)
            {
                return true;
            }
        }
        return false;
    }

    private void Fire()
    {
        fireRateTimer = 0;
        Vector3 aimDirection = (aim.aimPos.position - barrelPos.position).normalized;

        muzzleFalsh();
        audioSource.PlayOneShot(fireSound);

        for (int i = 0; i < bulletPerShot; i++)
        {
            GameObject currentBullet = Instantiate(bullet, barrelPos.position, Quaternion.LookRotation(aimDirection));
            Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
            rb.AddForce(aimDirection * bulletVelocity, ForceMode.Impulse);
        }
    }

    private void muzzleFalsh()
    {
        muzzleFlashParticles.Play();
        muzzleFlashLight.intensity = lightIntensity;
    }
}
