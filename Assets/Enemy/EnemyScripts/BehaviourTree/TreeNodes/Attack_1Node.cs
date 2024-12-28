using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Attack_1Node : Node
{
    private NavMeshAgent enemyAgent;
    private EnemyAIController enemyAI;
    private Animator animator;

    private float attackTimer = 0.0f; // Tracks time between attacks
    private float attackCooldown = 1.5f; // Time between attack phases
    private int currentAttackPhase = 1; // Tracks current attack animation phase
    private float transitionSpeed = 3.0f; // Speed for reducing velocity
    private bool isInCooldown= false;
    private bool isIdle = false; // Tracks if the enemy is in idle state
    private bool isInAttack=true;

    private Vector3 originalIdlePosition; // Tracks original idle position
    private Quaternion originalIdleRotation; // Tracks original idle rotation
    private bool positionCaptured = false; // Ensures position is captured once during idle

    public Attack_1Node(NavMeshAgent enemyAgent, EnemyAIController enemyAI, Animator animator)
    {
        this.enemyAgent = enemyAgent;
        this.enemyAI = enemyAI;
        this.animator = animator;
    }

    public override State Evaluate()
    {
        // Stop the agent from moving to initiate attacks
        enemyAgent.isStopped = true;

        // Step 1: Ensure velocity is reduced to 0 for the fighting idle stance
        float currentVelocity = animator.GetFloat("velocity");
        if (currentVelocity > 0.0f)
        {
            currentVelocity -= Time.deltaTime * transitionSpeed; // Gradually decrease velocity
            animator.SetFloat("velocity", Mathf.Max(currentVelocity, 0.0f)); // Clamp to 0
            node_state = State.RUNNING; // Wait until velocity is fully reduced
            return node_state;
        }

        // Step 2: Switch layer weights (Attack Layer: 1.0, Movement Layer: 0.0)
        animator.SetLayerWeight(0, 0.0f); // Movement Layer (index 0)
        animator.SetLayerWeight(1, 1.0f); // Attack Layer (index 1)

        if (isIdle)
        {
            // Idle phase: Set AttackPhase to 0 and wait for the cooldown to complete
            if (attackTimer == 0.0f)
            {
                animator.SetInteger("AttackPhase", 0); // Enter idle state exactly once
            }

            // Capture original position and rotation during idle stance (only once)
            if (!positionCaptured)
            {
                originalIdlePosition = enemyAI.enemyTransform.position;
                originalIdleRotation = enemyAI.enemyTransform.rotation;
                positionCaptured = true; // Ensure position is captured only once
            }

    
              // Check if the Animator is transitioning to another state
            if (animator.IsInTransition(1))
            {
                currentAttackPhase++; // Increment attack phase
                if (currentAttackPhase > 5) // Loop back to phase 1
                    {
                        currentAttackPhase = 1;
                    }
                isIdle = false;
                attackTimer += Time.deltaTime;
                isInCooldown=true;
            }
        }

        if(isInCooldown){
            
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(1);
            if (stateInfo.IsName("Fight Idle")){
                // Restore original position and rotation
                enemyAI.enemyTransform.position = originalIdlePosition;
                enemyAI.enemyTransform.rotation = originalIdleRotation;
            }
            attackTimer+= Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                // Cooldown complete, prepare for next attack
                attackTimer = 0.0f; // Reset timer
                isInAttack=true;
                isInCooldown=false;
            }
        }

        if(isInAttack)
        {
            // Attack phase: Trigger the current attack animation
            animator.SetInteger("AttackPhase", currentAttackPhase);

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(1);
            if (!stateInfo.IsName("Fight Idle"))
            {
                // Transitioning: Attack animation is complete
                isIdle = true;
                isInAttack = false;
            }
          
        }

        // Set the node state to running and return it
        node_state = State.RUNNING;
        return node_state;
    }

}
