using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public event Action<Vector2> OnMoveAction;
    public event Action OnUltimateAction;
    public event Action OnNormalAttackAction;
    public event Action OnSwordAttackAction;
    public event Action OnPauseAction;
    public void OnMove(InputValue value)
    {
        OnMoveAction?.Invoke(value.Get<Vector2>());
    }
    public void OnUltimate()
    {
        OnUltimateAction?.Invoke();     
    }
    public void OnNormalAttack()
    {
        OnNormalAttackAction?.Invoke();
    }
    public void OnSwordAttack()
    {
        OnSwordAttackAction?.Invoke();
    }
    public void OnEsc()
    {
        OnPauseAction?.Invoke();
    }
}
