using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevitationAttack : DefaultBossSkill
{
    public LevitationAttack() : base("LevitationAttack", 14f) { }

    public override IEnumerator Execute(BossEnemy boss)
    {
        SetSkillUsedTime();
        boss.PrepareForAttack("LevitationAttack");

        Debug.Log("levitationAttack");

        Vector3 direction = (boss.Target.transform.position - boss.transform.position).normalized;
        float angleBetween = Vector3.Angle(boss.transform.forward, direction);
        boss.Projector.enabled = true;
        if (angleBetween < boss.AttackAngle / 2)
        {
            CharacterController playerController = boss.Target.GetComponent<CharacterController>();
            if (playerController != null)
            {
                yield return new WaitForSeconds(2);

                direction = (boss.Target.transform.position - boss.transform.position).normalized;
                angleBetween = Vector3.Angle(boss.transform.forward, direction);
                if (angleBetween < boss.AttackAngle / 2)
                {
                    GameManager.Instance.canPlayerMove = false;
                    playerController.transform.gameObject.GetComponent<Player>().TakeDamage(boss.AttackDamage);
                    playerController.transform.GetComponent<Player>().anim.SetTrigger("levitation");
                    boss.StartCoroutine(ApplyKnockback(playerController));
                }
            }
        }
        yield return new WaitForSeconds(2);
        boss.Projector.enabled = false;
        GameManager.Instance.canPlayerMove = true;
        boss.ChangeToMoveToPlayerState();
    }

    private IEnumerator ApplyKnockback(CharacterController controller)
    {
        Vector3 knockbackForce = new Vector3(0, 10, 0);
        float timer = 0;
        while (timer < 0.5f)
        {
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
