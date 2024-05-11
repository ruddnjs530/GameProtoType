using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] float fireRate = 2.0f;
    float fireRateTimer;

    [SerializeField] GameObject bullet;
    [SerializeField] Transform barrelPos;
    [SerializeField] float bulletVelocity;
    [SerializeField] int bulletPerShot;

    Mouse aim;

    Light muzzleFlashLight;
    ParticleSystem muzzleFlashParticles;
    float lightIntensity;
    [SerializeField] float lightReturnTime = 20;
    Player player;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (ShouldFire()) Fire();
        muzzleFlashLight.intensity = Mathf.Lerp(muzzleFlashLight.intensity, 0, lightReturnTime * Time.deltaTime);
        
    }

    bool ShouldFire()
    {
        fireRateTimer += Time.deltaTime;
        if (!GameManager.Instance.canPlayerMove) return false;
        if (fireRateTimer < fireRate) return false;
        if (Input.GetKey(KeyCode.LeftShift)) return false;
        if (Input.GetMouseButton(0))
        {
            Vector3 lookDirection = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized;
            Vector3 bodyDirection = player.characterBody.forward; // character body public으로 바꿈. 프로퍼티로 만드는게 좋을듯

            float alignment = Vector3.Dot(lookDirection, bodyDirection);
            if (alignment > 0.95f)
            {
                return true;
            }
        }
        return false;
    }

    void Fire()
    {
        fireRateTimer = 0;
        //barrelPos.LookAt(aim.aimPos);
        muzzleFalsh();

        for (int i = 0; i < bulletPerShot; i++)
        {
            //GameObject currentBullet = Instantiate(bullet, barrelPos.position, barrelPos.rotation);
            //Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
            //rb.AddForce(barrelPos.forward * bulletVelocity, ForceMode.Impulse);
            Vector3 aimDir = (aim.aimPos.position - barrelPos.position).normalized;
            Instantiate(bullet, barrelPos.position, Quaternion.LookRotation(aimDir, Vector3.up));
        }
    }

    void muzzleFalsh()
    {
        muzzleFlashParticles.Play();
        muzzleFlashLight.intensity = lightIntensity;
    }
}
