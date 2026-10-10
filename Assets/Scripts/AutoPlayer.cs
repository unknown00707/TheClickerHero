using System.Collections;
using UnityEngine;

public class AutoPlayer : MonoBehaviour
{
    public Animator autoPlayerAnimator;
    Coroutine myCoroutine;
    private static readonly int SwordAttackHash = Animator.StringToHash("swordAttack");

    public void StartAutoAttack(float time)
    {
        myCoroutine = StartCoroutine(AutoAttackCoroutine(time));
    }

    IEnumerator AutoAttackCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        autoPlayerAnimator.SetTrigger(SwordAttackHash);
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
