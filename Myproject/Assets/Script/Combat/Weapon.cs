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
    private Player player; // 플레이어 참조(시점 회전과 조준점은 Player.LookAround가 담당한다)

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
        // UI 열림뿐 아니라 경직·사망 상태에서도 쏘지 못하게 플레이어의 행동 가능 여부를 따른다.
        if (!player.CanAct) return false;
        if (player.IsDashing) return false; // 대시 중에는 쏘지 않는다(Player도 대시 중에는 Attack 상태로 가지 않음)
        if (fireRateTimer < fireRate) return false;
        if (InputManager.Instance.IsRun) return false;
        if (InputManager.Instance.IsFire)
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
        Vector3 aimDirection = (player.aimPos.position - barrelPos.position).normalized;

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
