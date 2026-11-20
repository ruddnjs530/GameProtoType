using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevitationAttack : DefaultBossSkill
{
    // 경고 범위를 보여 주는 시간. 이 시간이 끝나는 순간 한 번 판정한다.
    // LevitationAttack 클립(2.67초)은 0~1.25초에 몸을 숙이고 1.25~2.08초에 머리를 들어올린다.
    // 머리를 들기 시작할 때 맞도록 1.35초로 맞춘다(예전 2초는 들어올리기가 거의 끝날 때라 늦어 보였다).
    private const float TelegraphTime = 1.35f;
    private const float StunDuration = 2f;  // 피격 시 플레이어가 이동·사격·대시를 할 수 없는 시간
    private const float RecoveryTime = 2f;  // 판정 후 보스가 다음 행동으로 넘어가기까지의 시간

    public LevitationAttack() : base("LevitationAttack", 14f) { }

    public override IEnumerator Execute(BossEnemy boss)
    {
        SetSkillUsedTime();
        boss.PrepareForAttack("LevitationAttack");

        Debug.Log("levitationAttack");

        // 시작 시점의 위치와 무관하게 항상 경고를 보여 주고, 경고가 끝나는 순간 표시된 범위 안에 있으면 맞는다.
        // 예전에는 시작 시 각도 검사 + 2초 뒤 각도만 재검사(거리 무제한)라 표시 밖에서도 맞았다.
        boss.Projector.enabled = true;
        yield return new WaitForSeconds(TelegraphTime);
        boss.Projector.enabled = false;

        Player player = boss.Target != null ? boss.Target.GetComponent<Player>() : null;
        if (player != null && LevitationArea.Contains(boss.transform.position, boss.transform.forward,
                player.transform.position, boss.LevitationAttackRange, boss.AttackAngle))
        {
            player.TakeDamage(boss.AttackDamage);
            player.anim.SetTrigger("levitation");
            player.Stun(StunDuration);

            CharacterController playerController = player.GetComponent<CharacterController>();
            if (playerController != null) boss.StartCoroutine(ApplyKnockback(playerController));
        }

        yield return new WaitForSeconds(RecoveryTime);
        boss.ChangeToMoveToPlayerState();
    }

    private IEnumerator ApplyKnockback(CharacterController controller)
    {
        Vector3 knockbackForce = new Vector3(0, 10, 0);
        float timer = 0;
        while (timer < 0.5f)
        {
            if (controller == null) yield break;
            controller.Move(knockbackForce * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    public override bool CanExecute(BossEnemy boss)
    {
        return boss.CanLevitationAttack() && IsCoolDownComplete();
    }
}
