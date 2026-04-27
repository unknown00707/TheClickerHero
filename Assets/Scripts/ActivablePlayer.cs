using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class ActivablePlayer : MonoBehaviour
{
    public PlayerStatsManager playerStatsManager; // 플레이어 스탯 매니저 참조
    public Transform playerTransform;
    public Vector2 inputVector;
    [Header("Animation")]
    public Animator playerAnim;
    public AnimatorOverrideController animatorOverrideController; // 애니메이터 오버라이드 컨트롤러 참조
    public Animator weaponAnim;
    public Animator weaponEffectAnim;
    [Header("Weapon Animation")]
    public WeaponScript weaponScript;
    public Renderer weaponRander;
    public WeaponDataSo currentWeapon; // 현재 장착 중인 무기 SO
    void Awake()
    {
        playerAnim.runtimeAnimatorController = animatorOverrideController;
        weaponRander.enabled = false;
    }
    void OnMove(InputValue value)
    {
        Vector2 rawInput = value.Get<Vector2>();

        // 대각선 입력 방지 로직 (그대로 유지!)
        if (Mathf.Abs(rawInput.x) > Mathf.Abs(rawInput.y))
            inputVector = new Vector2(rawInput.x > 0 ? 1 : -1, 0);
        else if (Mathf.Abs(rawInput.y) > Mathf.Abs(rawInput.x))
            inputVector = new Vector2(0, rawInput.y > 0 ? 1 : -1);
        else
            inputVector = Vector2.zero;

        // 🌟 여기가 변경된 핵심 마법의 코드입니다!
        if (inputVector != Vector2.zero)
        {
            // 움직일 때만 x, y 값을 전달합니다. 
            // 멈추면 마지막으로 누른 방향을 애니메이터가 '기억'하게 됩니다!
            playerAnim.SetFloat("x", inputVector.x);
            playerAnim.SetFloat("y", inputVector.y);
            weaponAnim.SetFloat("x", inputVector.x);
            weaponAnim.SetFloat("y", inputVector.y);
            weaponEffectAnim.SetFloat("x", inputVector.x);
            weaponEffectAnim.SetFloat("y", inputVector.y);
        }

        // Speed에 현재 움직임의 크기(움직이면 1, 멈추면 0)를 전달합니다.
        playerAnim.SetInteger("Speed", (int)inputVector.magnitude);
    }
    public void SetWeaponRanderFalse()
    {
        weaponRander.enabled = false;
    }
    void FixedUpdate()
    {
        Vector3 moveDir = new(inputVector.x, inputVector.y, 0);
        playerTransform.position += playerStatsManager.playerStats.Speed * Time.deltaTime * moveDir;
    }

    void OnUltimate()
    {
        
    }

    void OnNormalAttack()
    {
        playerAnim.SetTrigger("normalAttack");
    }

    void OnSwordAttack()
    {
        SetSameAnimeOverride();

        weaponRander.enabled = true;
        playerAnim.SetTrigger("swordAttack");
        weaponAnim.SetTrigger("sword");
        weaponEffectAnim.SetTrigger("swordEffect");
    }

    void SetSameAnimeOverride()
    {
        // 플레이어는 나중에
        weaponAnim.runtimeAnimatorController = currentWeapon.weaponOverrideController;
        weaponEffectAnim.runtimeAnimatorController = currentWeapon.weaponEffectOverrideController;
    }
}