using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ActivablePlayer : MonoBehaviour
{
    [Header("References")]
    private static readonly int AttackSpeedHash = Animator.StringToHash("attackSpeed");
    private static readonly int NormalAttackHash = Animator.StringToHash("normalAttack");
    private static readonly int YHash = Animator.StringToHash("y");
    private static readonly int XHash = Animator.StringToHash("x");
    private static readonly int SpeedHash = Animator.StringToHash("moveSpeed");
    private static readonly int SwordAttackHash = Animator.StringToHash("swordAttack");
    [Header("Manager References")]
    public PlayerStatsManager playerStatsManager; // 플레이어 스탯 매니저 참조  
    public WeaponManager weaponManager; // 무기 매니저 참조
    public PlayerInput playerInput; // 플레이어 입력 참조
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
    void OnEnable()
    {
        playerInput.OnMoveAction += Move;
        playerInput.OnUltimateAction += Ultimate;
        playerInput.OnNormalAttackAction += NormalAttack;
        playerInput.OnSwordAttackAction += SwordAttack;
    }
    void OnDisable()
    {
        playerInput.OnMoveAction -= Move;
        playerInput.OnUltimateAction -= Ultimate;
        playerInput.OnNormalAttackAction -= NormalAttack;
        playerInput.OnSwordAttackAction -= SwordAttack;
    }
    void Move(Vector2 direction)
    {
        Vector2 rawInput = direction;

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
    void Ultimate()
    {
            
    }

    void NormalAttack()
    {
        playerAnim.SetTrigger(NormalAttackHash);
    }

    void SwordAttack()
    {
        SetSameAnimeOverride(weaponManager.GetCurrentWeaponData());
        SetWeaponRanderFalse(true);
        SetSameAttackSpeed(playerStatsManager.playerStats.AttackSpeed);

        playerAnim.SetTrigger(SwordAttackHash);
        weaponAnim.SetTrigger(SwordAttackHash);
        weaponEffectAnim.SetTrigger(SwordAttackHash);
    }

    void SetSameAttackSpeed(float speed)
    {
        playerAnim.SetFloat(AttackSpeedHash, speed);
        weaponAnim.SetFloat(AttackSpeedHash, speed);
        weaponEffectAnim.SetFloat(AttackSpeedHash, speed);
    }
    // private IEnumerator Co_SpawnMonsterWithSync()
    // {
    //     // ⚠️ [중요] 트리거가 실행된 후, 애니메이터가 다음 애니메이션 정보로 
    //     // 갱신될 때까지 딱 1프레임 대기합니다.
    //     yield return null; 

    //     // 3. 현재 레이어(0번 베이스 레이어)에서 재생 중인 애니메이션 클립 정보 획득
    //     AnimatorClipInfo[] clipInfo = playerAnimator.GetCurrentAnimatorClipInfo(0);
        
    //     if (clipInfo.Length > 0)
    //     {
    //         AnimationClip currentClip = clipInfo[0].clip; // 현재 재생중인 클립
    //         float originalEventTime = 0f;
    //         bool eventFound = false;

    //         // 4. 해당 애니메이션 파일에 들어있는 모든 이벤트를 검색
    //         foreach (AnimationEvent animEvent in currentClip.events)
    //         {
    //             // 우리가 연출 타이밍으로 잡고 싶은 애니메이션 이벤트 함수 이름
    //             if (animEvent.functionName == "OnPlayerHitEvent") 
    //             {
    //                 originalEventTime = animEvent.time; // 원본 세팅 시간(초) 추출
    //                 eventFound = true;
    //                 break;
    //             }
    //         }

    //         // 5. 이벤트를 정상적으로 찾았다면 역산 공식 적용 후 몬스터 출발!
    //         if (eventFound)
    //         {
    //             // 실제 걸릴 시간 = 원본 시간 / 현재 공격 속도 배속
    //             float actualTimeUntilHit = originalEventTime / attackSpeedMultiplier;

    //             // 큐 풀에서 꺼내어 출발시키기
    //             Monster enemy = monsterPool.Get();
    //             enemy.SetTrajectory(spawnPoint.position, hitPoint.position, actualTimeUntilHit);
                
    //             Debug.Log($"🎬 [{currentClip.name}] 발견! 원본이벤트:{originalEventTime}초 -> 공속반영실제시간:{actualTimeUntilHit:F2}초로 몬스터 속도 자동 제어");
    //         }
    //         else
    //         {
    //             Debug.LogWarning($"⚠️ 현재 애니메이션 [{currentClip.name}]에 'OnPlayerHitEvent' 이벤트가 찍혀있지 않습니다!");
    //         }
    //     }
    // }
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