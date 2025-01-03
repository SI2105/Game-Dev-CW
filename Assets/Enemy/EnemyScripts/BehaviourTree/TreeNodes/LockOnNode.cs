using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LockOnNode : Node
{
    private NavMeshAgent enemyAgent;
    private EnemyAIController enemyAI;
    private Animator animator;
    private Transform player;

    // Animator velocities
    private float velocityX;
    private float velocityY;

    // Dodging
    private bool isDodging = false;
    private Vector3 dodgeDestination;
    private float dodgeSpeed = 10f;

    enum MovementState
    {
        Forward,
        StrafeRight,
        StrafeLeft
    }

    MovementState currentState = MovementState.Forward;

    public LockOnNode(NavMeshAgent enemyAgent, EnemyAIController enemyAI, Animator animator, Transform player)
    {
        this.enemyAgent = enemyAgent;
        this.enemyAI = enemyAI;
        this.animator = animator;
        this.player = player;
    }

    public override State Evaluate()
    {
        
        // 4) If not dodging, always rotate to face the player
        Vector3 toPlayer = (player.position - enemyAgent.transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(toPlayer);
        enemyAgent.transform.rotation = Quaternion.Slerp(
            enemyAgent.transform.rotation,
            targetRotation,
            Time.deltaTime * 2f
        );

        if(enemyAI.isComboAttacking){
            // Update animator smoothly
            animator.SetFloat("velocityX", 0.0f);
            animator.SetFloat("velocityY", 0.5f);
            node_state = State.SUCCESS;
            return node_state;
        }


        // 2) If currently attacking, do nothing else
        if (enemyAI.isAttacking)
        {
            // Update animator smoothly
            animator.SetFloat("velocityX", 0.0f);
            animator.SetFloat("velocityY", 0.5f);
            node_state = State.SUCCESS;
            return node_state;
        }


        float distance = Vector3.Distance(enemyAgent.transform.position, player.position);

        if (enemyAI.lockOnSensor.objects.Count > 0) // Lock on if sensor detects the player
        {
            enemyAgent.isStopped = true;
            enemyAgent.ResetPath();
            node_state = State.SUCCESS;
            return node_state;
        }
        
        enemyAgent.ResetPath();

        // 5) Determine front vs. left vs. right via dot products
        float dotForward = Vector3.Dot(enemyAgent.transform.forward, toPlayer);
        float dotRight   = Vector3.Dot(enemyAgent.transform.right,   toPlayer);

        // Decide which way to move
        //   Front if dotForward is fairly high (say > 0.5)
        //   Right if dotRight   > 0
        //   Left  otherwise
        float moveSpeed = 2f; // Adjust strafe/walk speed as desired

        // Apply movement based on state
        switch (currentState)
        {
            case MovementState.Forward:
                velocityY = Mathf.Lerp(velocityY, 0.5f, Time.deltaTime);
                velocityX = Mathf.Lerp(velocityX, 0.0f, Time.deltaTime);
                if(enemyAI.attackSensor.objects.Count==0){
                    enemyAgent.transform.position += enemyAgent.transform.forward * moveSpeed * Time.deltaTime;
                }
                break;

            case MovementState.StrafeRight:
                velocityX = Mathf.Lerp(velocityX, 0.5f, Time.deltaTime);
                velocityY = Mathf.Lerp(velocityY, 0.0f, Time.deltaTime);
                if(enemyAI.attackSensor.objects.Count==0){
                    enemyAgent.transform.position += enemyAgent.transform.right * moveSpeed * Time.deltaTime;
                 }
                break;

            case MovementState.StrafeLeft:
                velocityX = Mathf.Lerp(velocityX, -0.5f, Time.deltaTime);
                velocityY = Mathf.Lerp(velocityY, 0.0f, Time.deltaTime);
                if(enemyAI.attackSensor.objects.Count==0){
                    enemyAgent.transform.position -= enemyAgent.transform.right * moveSpeed * Time.deltaTime;
                }
                break;
        }

       
        if (dotForward > 0.95f)
        {
            if (currentState != MovementState.Forward)
            {
                currentState = MovementState.Forward;
                
            }
        }
        else if (dotRight > 0f)
        {
            if (currentState != MovementState.StrafeRight)
            {
                currentState = MovementState.StrafeRight;
                
            }
        }
        else
        {
            if (currentState != MovementState.StrafeLeft)
            {
                currentState = MovementState.StrafeLeft;
                
            }
        }
   
        // Update animator smoothly
        animator.SetFloat("velocityX", velocityX);
        animator.SetFloat("velocityY", velocityY);

        // 8) Default
        node_state = State.FAILURE;
        return node_state;
    }
}
