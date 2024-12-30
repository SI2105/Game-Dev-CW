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
    private bool isIdle = false; // Tracks if the enemy is in idle state
    private bool isInAttack = true;
    private bool isTurning = false;
    private Vector3 originalIdlePosition; // Tracks original idle position
    private Quaternion originalIdleRotation; // Tracks original idle rotation
    private bool positionCaptured = false; // Ensures position is captured once during idle
    private bool positionEnemy=false;

    public Attack_1Node(NavMeshAgent enemyAgent, EnemyAIController enemyAI, Animator animator)
    {
        this.enemyAgent = enemyAgent;
        this.enemyAI = enemyAI;
        this.animator = animator;
    }

    public override State Evaluate()
    {

        float distance = Vector3.Distance(enemyAgent.transform.position, enemyAI.playerTransform.position);

        if(distance > 1.5f && !positionEnemy){
            // Step 1: Switch layer weights to Movement Layer
            animator.SetLayerWeight(0, 1.0f); // Movement Layer (index 0)
            animator.SetLayerWeight(1, 0.0f); // Attack Layer (index 1)
            animator.SetBool("isPatrolling", true);
            animator.SetBool("isChasing", false);
            enemyAgent.SetDestination(enemyAI.playerTransform.position);
            node_state=State.RUNNING;
            return node_state;
        }

        positionEnemy=true;

        if(distance>=2.5f){
            positionEnemy=false;
            positionCaptured=false;
            node_state=State.RUNNING;
            return node_state;
        }

        // Step 1: Switch layer weights to Attack Layer
        animator.SetLayerWeight(0, 0.0f); // Movement Layer (index 0)
        animator.SetLayerWeight(1, 1.0f); // Attack Layer (index 1)

        enemyAgent.ResetPath();
       

        // Capture original position and rotation during idle stance (only once)
        if (!positionCaptured)
        {
            // Calculate the direction to the player
            Vector3 toPlayer = (enemyAI.playerTransform.position - enemyAI.enemyTransform.position).normalized;

            // Set the rotation to face the player
            Quaternion lookRotation = Quaternion.LookRotation(toPlayer, Vector3.up);
            enemyAI.enemyTransform.rotation = lookRotation;

            // Capture the position and rotation
            originalIdlePosition = enemyAI.enemyTransform.position;
            originalIdleRotation = enemyAI.enemyTransform.rotation;
            positionCaptured = true; // Ensure position is captured only once
        }

        // Idle phase
        if (isIdle)
        {
            if(!animator.GetCurrentAnimatorStateInfo(1).IsName("Fight Idle")){
                animator.SetInteger("AttackPhase", 0); // Reset to idle animation
            }
            else{
                isInAttack=true;
            }
        }

        //Attack Phase
        if (isInAttack)
        {
            if(animator.GetCurrentAnimatorStateInfo(1).IsName("Fight Idle")){
                 // Gradually restore position and rotation
                enemyAI.enemyTransform.position = Vector3.Lerp(
                    enemyAI.enemyTransform.position,
                    originalIdlePosition,
                    Time.deltaTime * 2.0f
                );

                enemyAI.enemyTransform.rotation = Quaternion.Slerp(
                    enemyAI.enemyTransform.rotation,
                    originalIdleRotation,
                    Time.deltaTime * 2.0f
                );
                
                
                // Attack phase: Trigger the current attack animation
                animator.SetInteger("AttackPhase", currentAttackPhase);

                // Transitioning: Attack animation is complete
                isIdle = true;
                isInAttack = false;
                currentAttackPhase++; // Move to the next attack phase
                if (currentAttackPhase > 5) // Loop back to phase 1
                    {
                        currentAttackPhase = 1;
                    }
            }
        }

        node_state = State.RUNNING;
        return node_state;
    }

}
