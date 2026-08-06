using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth;
    public float currentHealth;
    public bool isDead;

    public virtual void ChangeDieState()
    {
        isDead = !isDead;
    }

    public virtual void HealthInit(float hp)
    {
        currentHealth = maxHealth = hp;
    }

    public virtual void TakeDamage(DamageInfo info)
    {
        if (isDead) return; // 이미 사망한 경우에는 데미지를 받지 않음

        currentHealth -= info.damage;

        if (currentHealth <= 0f && !isDead) 
        {
            isDead = true;
            Die();
        }
    }

    public abstract void Die();
}
