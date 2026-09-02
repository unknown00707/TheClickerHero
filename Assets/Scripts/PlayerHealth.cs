using System.Collections;
using UnityEngine;

public class PlayerHealth : Entity
{
    public GameObject playerObj;
    public DungeonManager dungeonManager;
    public PlayerStatsManager playerStatsManager;
    public BoxCollider2D playerBoxCollider;
    [SerializeField] private ActivablePlayer activablePlayer;
    [SerializeField] private Sprite diedSprite;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private readonly float deadTime = 1f; // 사망 후 딜레이 시간
    private void Start()
    {
        PlayerHealthInit();
        ChangePlayerActiveState(!isDead);
    }
    public override void Die()
    {
        isDead = true;
        playerBoxCollider.enabled = !isDead;
        activablePlayer.SetSkinAnimeOverride().enabled = !isDead;
        spriteRenderer.sprite = diedSprite;

        StartCoroutine(ExecuteRewardUIAfterDelay(deadTime));
    }
    IEnumerator ExecuteRewardUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ChangePlayerActiveState(isDead);
        dungeonManager.SetRewardUI(!isDead);
    }
    public void PlayerHealthInit()
    {
        isDead = false;
        HealthInit(playerStatsManager.playerStats.Health);
        activablePlayer.SetSkinAnimeOverride().enabled = !isDead;
        playerBoxCollider.enabled = !isDead;
    }
    /// <summary>
    /// "true -> false, false -> true"
    /// </summary>
    /// <param name="isDead"></param>
    public void ChangePlayerActiveState(bool isDead)
    {
        playerObj.SetActive(!isDead);
    }
}
