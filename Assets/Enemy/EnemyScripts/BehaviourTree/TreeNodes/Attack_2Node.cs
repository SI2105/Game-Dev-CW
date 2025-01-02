using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Attack_2Node : Node
{
    private NavMeshAgent enemyAgent;
    private EnemyAIController enemyAI;
    private Animator animator;
    // Distance thresholds
    private float surgeDistanceThreshold = 4f; // Surge if player is further than this

    public Attack_2Node(NavMeshAgent enemyAgent, EnemyAIController enemyAI, Animator animator)
    {
        this.enemyAgent = enemyAgent;
        this.enemyAI = enemyAI;
        this.animator = animator;
    }

    public override State Evaluate()
    {
        if(!enemyAI.isComboAttacking){
            // Activate the Combo attack sequence
            animator.SetTrigger("Combo");
            enemyAI.isComboAttacking=true;
        }
    
        // Follow the player while attacking
        Transform playerTransform = enemyAI.playerTransform;
        float distanceToPlayer = Vector3.Distance(enemyAgent.transform.position, playerTransform.position);

        if (distanceToPlayer > surgeDistanceThreshold)
        {
            // Player is far away: Surge toward the player
            Debug.LogError("Player far away: Surge toward the player");

            // Enable Surge behavior
            animator.SetBool("Surge", true);

            // Set high speed and target destination
            enemyAgent.speed = 10f; // Surge speed
            enemyAgent.ResetPath(); // Reset
            enemyAgent.isStopped = false;
            // Calculate the direction vector from enemy to player
            Vector3 directionToPlayer = (enemyAI.playerTransform.position - enemyAgent.transform.position).normalized;

            // Offset the player's position by 2f in the direction away from the enemy
            Vector3 destination = enemyAI.playerTransform.position - directionToPlayer * 2f;
            enemyAgent.SetDestination(destination);
        }
        else
        {

            // Player is close: Move toward the player without surging
            Debug.LogError("Player close: Move toward the player");

            // Set normal speed and target destination
            enemyAgent.speed = 3.5f; // Normal movement speed
            enemyAgent.ResetPath(); // Reset
            enemyAgent.isStopped = false;
             // Calculate the direction vector from enemy to player
            Vector3 directionToPlayer = (enemyAI.playerTransform.position - enemyAgent.transform.position).normalized;

            // Offset the player's position by 2f in the direction away from the enemy
            Vector3 destination = enemyAI.playerTransform.position - directionToPlayer * 2f;
            enemyAgent.SetDestination(destination);
        }

        // // Ensure the enemy rotates to face the player
        // Vector3 directionToPlayer = (playerTransform.position - enemyAgent.transform.position).normalized;
        // Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        // enemyAgent.transform.rotation = Quaternion.Slerp(
        //     enemyAgent.transform.rotation,
        //     targetRotation,
        //     Time.deltaTime * 10f
        // );

        // Node is still running as long as the Combo animation is playing and the enemy is pursuing the player
        return State.RUNNING;
    }
}
