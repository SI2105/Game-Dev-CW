using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Attack_2Node : Node
{
    private NavMeshAgent enemyAgent;
    private EnemyAIController enemyAI;
    private Animator animator;

    public Attack_2Node(NavMeshAgent enemyAgent, EnemyAIController enemyAI, Animator animator)
    {
        this.enemyAgent = enemyAgent;
        this.enemyAI = enemyAI;
        this.animator = animator;
    }

    public override State Evaluate()
    {
        if (!enemyAI.isComboAttacking)
        {
            // Activate the Combo attack sequence
            animator.SetTrigger("Combo");
            enemyAI.isComboAttacking = true;
        }

        if (enemyAI.attackSensor.objects.Count == 0)
        {
            // Calculate the direction vector from enemy to player
            Vector3 directionToPlayer = (enemyAI.playerTransform.position - enemyAgent.transform.position).normalized;

            // Gradually move towards the player
            float surgeSpeed = 10f;
            Vector3 movement = directionToPlayer * surgeSpeed * Time.deltaTime;

            // Update enemy position manually
            enemyAgent.transform.position += movement;
        }
        
        return State.RUNNING;
    }
}
