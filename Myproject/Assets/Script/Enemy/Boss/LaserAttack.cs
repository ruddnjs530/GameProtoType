using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserAttack : DefaultBossSkill
{
    private float laserRange = 15f;
    private float laserDuration = 2.0f;
    private float warningTime = 2f;
    private float warningLineStartWidth = 0.5f;
    private float warningLineEndWidth = 0f;
    private float rotationSpeed = 5f;
    private float laserDamagePerSecond = 10f; // 레이저에 닿아 있는 동안 초당 피해(2초 동안 계속 맞으면 20)

    public LaserAttack() : base("LaserAttack", 8f) { }

    public override IEnumerator Execute(BossEnemy boss)
    {
        SetSkillUsedTime();
        boss.PrepareForAttack("LaserAttack");
        Debug.Log("laserAttack");

        boss.WarningLine.enabled = true;


        Vector3 directionToPlayer = (boss.Target.position - boss.transform.position).normalized;

        float elapsed = 0f;

        while (elapsed < warningTime)
        {
            elapsed += Time.deltaTime;

            float currentWidth = Mathf.Lerp(warningLineStartWidth, warningLineEndWidth, elapsed / warningTime);
            boss.WarningLine.startWidth = currentWidth;
            boss.WarningLine.endWidth = currentWidth;

            directionToPlayer = (boss.Target.position - boss.transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            boss.transform.rotation = Quaternion.Slerp(boss.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            Vector3 startPosition = boss.ShotPos.position;
            Vector3 direction = (boss.Target.position - startPosition).normalized;
            RaycastHit hit;

            if (Physics.Raycast(startPosition, direction, out hit, laserRange))
            {
                boss.WarningLine.SetPosition(1, hit.point);
            }
            else
            {
                boss.WarningLine.SetPosition(1, startPosition + directionToPlayer * laserRange);
            }

            boss.WarningLine.SetPosition(0, startPosition);

            yield return null;
        }

        boss.WarningLine.enabled = false;

        Vector3 laserEndPoint = boss.WarningLine.GetPosition(1);

        yield return new WaitForSeconds(0.3f);

        // 경고선이 끝난 방향으로 laserDuration 동안 레이저를 유지하고, 레이저에 닿아 있는 동안 초당 피해를 준다.
        // 예전에는 발사 순간 한 번만 판정해서 레이저 안에 서 있어도 한 번만 맞았다.
        Vector3 fireDirection = (laserEndPoint - boss.ShotPos.position).normalized;
        bool aimed = Vector3.Angle(boss.transform.forward, directionToPlayer) < boss.AttackAngle / 2;
        boss.Laser.enabled = aimed;

        int laserMask = ~LayerMask.GetMask("Bullet"); // 날아가는 플레이어 탄환에 레이저가 끊기지 않게 한다
        float fired = 0f;
        while (aimed && fired < laserDuration)
        {
            Vector3 startPosition = boss.ShotPos.position;
            boss.Laser.SetPosition(0, startPosition);

            if (Physics.Raycast(startPosition, fireDirection, out RaycastHit hit, laserRange, laserMask, QueryTriggerInteraction.Ignore))
            {
                boss.Laser.SetPosition(1, hit.point);

                if (hit.transform.CompareTag("Player"))
                {
                    Player player = hit.transform.GetComponent<Player>();
                    player?.TakeDamage(laserDamagePerSecond * Time.deltaTime);
                }
            }
            else
            {
                boss.Laser.SetPosition(1, startPosition + fireDirection * laserRange);
            }

            fired += Time.deltaTime;
            yield return null;
        }
        if (!aimed) yield return new WaitForSeconds(laserDuration);

        boss.Laser.enabled = false;
        boss.ChangeToMoveToPlayerState();
    }

    public override bool CanExecute(BossEnemy boss)
    {
        return boss.CanLaserAttack && IsCoolDownComplete();
    }
}
