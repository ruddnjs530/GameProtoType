using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Weapon : MonoBehaviour
{
    [Header("Firing Settings")]
    [SerializeField] private float fireRate = 2.0f; // 발사 속도 (초 단위)
    private float fireRateTimer;

    [SerializeField] private GameObject bullet; // 총알 프리팹
    [SerializeField] private Transform barrelPos; // 총구 위치
    [SerializeField] private float bulletVelocity; // 총알 속도
    [SerializeField] private int bulletPerShot; // 발사 당 총알 수
    [SerializeField] private float fireAlignmentThreshold = 0.95f; // 발사 허용 정렬 임계값 (0~1)

    [Header("References")]
    private Mouse aim; // 마우스 조준 참조
    private Player player; // 플레이어 참조

    [Header("Effects")]
    private Light muzzleFlashLight; // 총구 섬광 라이트
    private ParticleSystem muzzleFlashParticles; // 총구 섬광 파티클
    private float lightIntensity;
    [SerializeField] private float lightReturnTime = 20; // 라이트 복귀 속도

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource; // 오디오 소스
    [SerializeField] private AudioClip fireSound; // 발사 사운드

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
            if (alignment > fireAlignmentThreshold)
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

        MuzzleFlash();
        audioSource.PlayOneShot(fireSound);

        for (int i = 0; i < bulletPerShot; i++)
        {
            GameObject currentBullet = Instantiate(bullet, barrelPos.position, Quaternion.LookRotation(aimDirection));
            Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
            rb.AddForce(aimDirection * bulletVelocity, ForceMode.Impulse);
        }
    }

    private void MuzzleFlash()
    {
        muzzleFlashParticles.Play();
        muzzleFlashLight.intensity = lightIntensity;
    }
}
