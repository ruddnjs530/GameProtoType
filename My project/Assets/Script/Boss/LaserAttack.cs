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

        boss.Laser.enabled = true;

        if (Vector3.Angle(boss.transform.forward, directionToPlayer) < boss.AttackAngle / 2)
        {
            RaycastHit hit;
            Vector3 startPosition = boss.ShotPos.position;
            Vector3 direction = (laserEndPoint - startPosition).normalized;

            boss.Laser.SetPosition(0, startPosition);

            if (Physics.Raycast(startPosition, direction, out hit, laserRange))
            {
                boss.Laser.SetPosition(1, hit.point);

                if (hit.transform.CompareTag("Player"))
                {
                    Player player = hit.transform.GetComponent<Player>();
                    player?.TakeDamage(boss.AttackDamage);
                }
            }
            else
            {
                boss.Laser.SetPosition(1, boss.ShotPos.position + direction * laserRange);
            }
        }

        yield return new WaitForSeconds(laserDuration);
        boss.Laser.enabled = false;
        boss.ChangeToMoveToPlayerState();
    }

    public override bool CanExecute(BossEnemy boss)
    {
        return boss.CanLaserAttack && IsCoolDownComplete();
    }
}
