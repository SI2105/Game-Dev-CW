using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [field: SerializeField] public PlayerMovementState CurrentPlayerMovementState { get; private set; } = PlayerMovementState.Idling;
    [field: SerializeField] public PlayerAttackState CurrentPlayerAttackState { get; private set; } = PlayerAttackState.Idling;
    [field: SerializeField] public PlayerAttackStatusState CurrentPlayerAttackStatusState { get; private set; } = PlayerAttackStatusState.Idling;

    public void SetPlayerMovementState(PlayerMovementState playerMovementState)
    {
        CurrentPlayerMovementState = playerMovementState;
    }

    public void SetPlayerAttackState(PlayerAttackState playerAttackState)
    {
        CurrentPlayerAttackState = playerAttackState;
    }

    public void SetPlayerAttackStatusState(PlayerAttackStatusState playerAttackStatusState)
    {
        CurrentPlayerAttackStatusState = playerAttackStatusState;
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

    public bool IsInAttackStatusState(PlayerAttackStatusState state){
        return CurrentPlayerAttackStatusState == state;
    }

    public bool attack1_progress = false;
    public bool attack2_progress = false;
    public bool attack3_progress = false;

    public bool Attack1_progress 
    {
        get { return attack1_progress; } 
        set { attack1_progress = value; }
    }
    public bool Attack2_progress 
    {
        get { return attack2_progress; } 
        set { attack2_progress = value; }
    }
    public bool Attack3_progress 
    {
        get { return attack3_progress; } 
        set { attack3_progress = value; }
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

public enum PlayerAttackStatusState{
    InProgress = 0,
    Idling = 1,
    Attack_progress_1 = 2,
    Attack_progress_2 = 3,
    Attack_progress_3 = 4,
}