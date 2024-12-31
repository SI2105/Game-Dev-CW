using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [field: SerializeField] public PlayerMovementState CurrentPlayerMovementState { get; private set; } = PlayerMovementState.Idling;
    [field: SerializeField] public PlayerAttackState CurrentPlayerAttackState { get; private set; } = PlayerAttackState.Idling;

    public void SetPlayerMovementState(PlayerMovementState playerMovementState)
    {
        CurrentPlayerMovementState = playerMovementState;
    }

    public void SetPlayerAttackState(PlayerAttackState playerAttackState)
    {
        CurrentPlayerAttackState = playerAttackState;
    }

    public bool InGroundedState()
    {
        return CurrentPlayerMovementState == PlayerMovementState.Idling ||
               CurrentPlayerMovementState == PlayerMovementState.Walking ||
               CurrentPlayerMovementState == PlayerMovementState.Running ||
               CurrentPlayerMovementState == PlayerMovementState.Strafing;
    }

    public bool IsInState(PlayerAttackState state)
    {
        // Use the correct property name
        return CurrentPlayerAttackState == state;
    }
}





public enum PlayerMovementState{
    Idling = 0,
    Walking = 1,
    Running = 2,
    Jumping = 5,
    Strafing = 6,
    Attacking = 7,
}

public enum PlayerAttackState{
    Idling = 0,
    Attacking = 1
}