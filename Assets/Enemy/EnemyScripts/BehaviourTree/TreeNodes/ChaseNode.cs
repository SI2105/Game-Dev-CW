using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChaseNode : Node
{
    private NavMeshAgent enemyAgent;
    private Transform player;
    private Animator animator;
    private EnemyAIController enemyAI; ///
    private float velocity =1.0f;
    public ChaseNode(NavMeshAgent enemyAgent, Transform player, Animator animator, EnemyAIController enemyAI){
        this.enemyAgent = enemyAgent;
        this.player = player;
        this.animator = animator;
        this.enemyAI = enemyAI;
    }

    public override State Evaluate()
    {
        // Set the Movement Layer weight to 1 (fully active)
        animator.SetLayerWeight(0, 1.0f); // Movement Layer (index 0)
        animator.SetLayerWeight(1, 0.0f); // Attack Layer (index 1)

        // Increment the velocity to smoothly transition to running state (1.0)
        float currentVelocity = animator.GetFloat("velocity");
        if (currentVelocity < 1.0f)
        {
            float newVelocity = currentVelocity + Time.deltaTime;
            animator.SetFloat("velocity", Mathf.Clamp(newVelocity, 0.0f, 1.0f));
        }

        // Calculate the distance between the enemy and the player
        float distance = Vector3.Distance(enemyAgent.transform.position, player.position);

        // If the distance is more than 3.0f, continue chasing
        if (distance > 2.5f)
        {
            enemyAgent.isStopped = false;
            enemyAgent.SetDestination(player.position);
            node_state = State.RUNNING;
            return node_state;
        }
        // Otherwise, stop as the enemy transitions to attack mode
        else
        {
            // Stop the NavMeshAgent to prepare for attacking
            enemyAgent.isStopped = true;

            // Set the node state to FAILURE to indicate transition to AttackNode
            node_state = State.FAILURE;
            return node_state;
        }
    }


}
