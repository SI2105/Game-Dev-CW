using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Attack_1Node : Node
{
    private NavMeshAgent enemyAgent;
    private EnemyAIController enemyAI;
    private Animator animator;

    public Attack_1Node(NavMeshAgent enemyAgent, EnemyAIController enemyAI, Animator animator)
    {
        this.enemyAgent = enemyAgent;
        this.enemyAI = enemyAI;
        this.animator = animator;
    }

    public override State Evaluate()
    {
        
        float distance = Vector3.Distance(enemyAgent.transform.position, enemyAI.playerTransform.position);
        if (enemyAI.attackSensor.objects.Count>0)
        {
            animator.SetBool("IsPlayingAction", true);
            enemyAgent.ResetPath();
            // Close-range attack
            enemyAgent.isStopped = true;
            // Randomly choose between AttackRight or AttackLeft
            animator.SetBool("AttackLeft", true);
            
        }
        else if (distance > 4f)
        {
            animator.SetBool("IsPlayingAction", true);
            // Long-range surge attack
            animator.SetBool("AttackLeft", true);

            // Set surge boolean
            animator.SetBool("Surge", true);

            enemyAgent.isStopped = false;

            // Calculate the direction vector from enemy to player
            Vector3 directionToPlayer = (enemyAI.playerTransform.position - enemyAgent.transform.position).normalized;

            // Offset the player's position by 2.5f in the direction away from the enemy
            Vector3 destination = enemyAI.playerTransform.position - directionToPlayer * 2f;

            // Calculate the distance to the player
            float distanceToPlayer = Vector3.Distance(enemyAI.playerTransform.position, enemyAgent.transform.position);

            // Set the speed as the difference in distance minus 2.5f (clamp to ensure positive speed)
            float adjustedSpeed = Mathf.Max(distanceToPlayer, 0f);
            enemyAgent.speed = adjustedSpeed;

            // Set the calculated destination
            enemyAgent.SetDestination(destination);
        }

        else{
          
            enemyAgent.isStopped = false;
            enemyAgent.speed=1.5f;
            animator.SetBool("IsPlayingAction", false);
            animator.SetFloat("velocityY", 0.5f);
            animator.SetFloat("velocityX", 0.0f);
            enemyAgent.SetDestination(enemyAI.playerTransform.position);
             
        }
       

        node_state = State.RUNNING;
        return node_state;
    }
}
