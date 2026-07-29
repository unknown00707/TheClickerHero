using UnityEngine;

public class PlayerHealth : Entity
{
    public PlayerStatsManager playerStatsManager;

    private void Start()
    {
        HealthInit(playerStatsManager.playerStats.Health);
    }
    public override void Die()
    {
        
    }
}
