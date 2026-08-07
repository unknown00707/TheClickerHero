using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class ActivablePlayer : MonoBehaviour
{
    private static readonly int NormalAttackHash = Animator.StringToHash("normalAttack");
    private static readonly int YHash = Animator.StringToHash("y");
    private static readonly int XHash = Animator.StringToHash("x");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int SwordAttackHash = Animator.StringToHash("swordAttack");
    
    public PlayerStatsManager playerStatsManager; // 플레이어 스탯 매니저 참조  
    public WeaponManager weaponManager; // 무기 매니저 참조
    public Rigidbody2D playerRigidbody;
    private Vector2 inputVector;
    private Vector2 dirPlayerVector;
    public Vector2 ReturnDirPlayerVec => dirPlayerVector;
    [Header("Animation")]
    public Animator playerAnim;
    public Animator weaponAnim;
    public Animator weaponEffectAnim;
    [Header("Weapon Animation")]
    public WeaponScript weaponScript;
    public Renderer weaponRander;
    
    void Awake()
    {
        SetWeaponRanderFalse(false);
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
            playerAnim.SetFloat(XHash, inputVector.x);
            playerAnim.SetFloat(YHash, inputVector.y);
            weaponAnim.SetFloat(XHash, inputVector.x);
            weaponAnim.SetFloat(YHash, inputVector.y);
            weaponEffectAnim.SetFloat(XHash, inputVector.x);
            weaponEffectAnim.SetFloat(YHash, inputVector.y);

            dirPlayerVector = inputVector;
        }

        // Speed에 현재 움직임의 크기(움직이면 1, 멈추면 0)를 전달합니다.
        playerAnim.SetInteger(SpeedHash, (int)inputVector.magnitude);
    }
    public void SetWeaponRanderFalse(bool isWeaponRander)
    {
        weaponRander.enabled = isWeaponRander;
    }
    void FixedUpdate()
    {
        Vector2 moveDir = new Vector2(inputVector.x, inputVector.y).normalized; // 입력 벡터를 정규화하여 방향만 유지
        Vector2 newPosition = playerRigidbody.position + moveDir * (playerStatsManager.playerStats.Speed * Time.fixedDeltaTime);
        playerRigidbody.MovePosition(newPosition);
    }
    // ----------------------- 공격 입력 메서드 -----------------------
    void OnUltimate()
    {
            
    }

    void OnNormalAttack()
    {
        playerAnim.SetTrigger(NormalAttackHash);
    }

    void OnSwordAttack()
    {
        SetSameAnimeOverride(weaponManager.GetCurrentWeaponData());
        SetWeaponRanderFalse(true);
        
        playerAnim.SetTrigger(SwordAttackHash);
        weaponAnim.SetTrigger(SwordAttackHash);
        weaponEffectAnim.SetTrigger(SwordAttackHash);
    }
    // ----------------------- 애니메이션 관련 메서드 -----------------------
    public void SetSameAnimeOverride(WeaponDataSo currentWeapon)
    {
        // 플레이어는 나중에
        weaponAnim.runtimeAnimatorController = currentWeapon.weaponOverrideController;
        weaponEffectAnim.runtimeAnimatorController = currentWeapon.weaponEffectOverrideController;
    }
    public Animator SetSkinAnimeOverride()
    {
        return playerAnim;
    }
}