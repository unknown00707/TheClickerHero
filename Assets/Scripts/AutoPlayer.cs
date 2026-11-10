using System.Collections;
using UnityEngine;

public class AutoPlayer : MonoBehaviour
{
    public Animator autoPlayerAnimator;
    public Animator autoPlayerWeaponAnimator;
    public Animator autoPlayerWeaponEffectAnimator;
    Coroutine myCoroutine;
    private static readonly int SwordAttackHash = Animator.StringToHash("swordAttack");

    public void StartAutoAttack(float time)
    {
        myCoroutine = StartCoroutine(AutoAttackCoroutine(time));
    }

    IEnumerator AutoAttackCoroutine(float time)
    {
        TestLogger.Log($"{ColorPalette.Cyan}[AutoPlayer]{ColorPalette.End} AutoAttackCoroutine called after {time} seconds.");
        
        yield return new WaitForSecondsRealtime(time);

        autoPlayerAnimator.SetTrigger(SwordAttackHash);
        autoPlayerWeaponAnimator.SetTrigger(SwordAttackHash);
        autoPlayerWeaponEffectAnimator.SetTrigger(SwordAttackHash);
    }

    public void StopAutoAttack()
    {
        if (myCoroutine != null)
        {
            StopCoroutine(myCoroutine);
            myCoroutine = null;
        }
    }
}
