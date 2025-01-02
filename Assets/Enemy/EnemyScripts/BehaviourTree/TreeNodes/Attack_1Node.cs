using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Attack_1Node : Node
{
    private NavMeshAgent enemyAgent;
    private EnemyAIController enemyAI;
    private Animator animator;

    private float lastAttackTime = 0f; // Tracks the time of the last attack
    private float attackCooldown = 1f; // Minimum time between attacks (in seconds)

    public Attack_1Node(NavMeshAgent enemyAgent, EnemyAIController enemyAI, Animator animator)
    {
        this.enemyAgent = enemyAgent;
        this.enemyAI = enemyAI;
        this.animator = animator;
    }

    public override State Evaluate()
    {
        Debug.LogError("Stuck in Attack_1Node");
        float currentTime = Time.time;

        if (currentTime - lastAttackTime >= attackCooldown)
        {
            if (!(enemyAI.attackSensor.objects.Count > 0)) // Hysteresis for >6f condition
            {
                Debug.LogError("Attacking and Surging");
                enemyAI.isAttacking = true;
                animator.SetBool("IsPlayingAction", true);
                // Calculate the direction vector from enemy to player
                Vector3 directionToPlayer = (enemyAI.playerTransform.position - enemyAgent.transform.position).normalized;

                // Offset the player's position by 2f in the direction away from the enemy
                Vector3 destination = enemyAI.playerTransform.position - directionToPlayer * 2f;

                // Set the speed for the surge
                float adjustedSpeed = 20f;
                enemyAgent.speed = adjustedSpeed;

                // Set the calculated destination
                enemyAgent.SetDestination(destination);
                // Set surge boolean
                animator.SetBool("Surge", true);

                // Trigger the surge attack
                animator.SetTrigger("AttackLeft");
            
                lastAttackTime = currentTime;

            }
            else
            {
                enemyAgent.ResetPath();
                // Debug.LogError("Here");
                animator.SetBool("IsPlayingAction", true);
                // Long-range surge attack
                animator.SetTrigger("AttackLeft");
                enemyAI.isAttacking = true;
                lastAttackTime = currentTime;
            }
        }

        node_state = State.RUNNING;
        return node_state;
    }

}
