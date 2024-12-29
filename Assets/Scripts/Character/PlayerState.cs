using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour{
    [field: SerializeField] public PlayerMovementState CurrentPlayerMovementState { get; private set; } = PlayerMovementState.Idling;
    public void SetPlayerMovementState(PlayerMovementState playerMovementState){
        CurrentPlayerMovementState = playerMovementState;
    }

    public bool InGroundedState(){
        return CurrentPlayerMovementState == PlayerMovementState.Idling ||
                CurrentPlayerMovementState == PlayerMovementState.Walking ||
                CurrentPlayerMovementState == PlayerMovementState.Running ||
                CurrentPlayerMovementState == PlayerMovementState.Strafing;
    }
}

public enum PlayerMovementState{
    Idling = 0,
    Walking = 1,
    Running = 2,
    Jumping = 5,
    Strafing = 6,
}