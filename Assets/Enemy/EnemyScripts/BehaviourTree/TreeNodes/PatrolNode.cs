using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolNode : Node
{
    private NavMeshAgent enemyAgent;
    private EnemyAIController enemyAI;
    private Vector3 patrolCenter;
    private float patrolRadius;
    private float patrolSpeed;
    private float patrolTimer;
    
    private Animator animator;

    public PatrolNode(NavMeshAgent enemyAgent, EnemyAIController enemyAI, float patrolRadius, float patrolSpeed, Animator animator)
    {
        this.enemyAgent = enemyAgent;
        this.enemyAI = enemyAI;
        this.patrolCenter = enemyAgent.transform.position;
        this.patrolRadius = patrolRadius;
        this.patrolSpeed = patrolSpeed;
        this.animator = animator;
        patrolTimer = Random.Range(0f, Mathf.PI * 2); // Randomize the starting point for varied behavior
    }

    public override State Evaluate()
    {
        // Set the Movement Layer weight to 1 (fully active)
        animator.SetLayerWeight(0, 1.0f); // Movement Layer (index 0)
        animator.SetLayerWeight(1, 0.0f); // Attack Layer (index 1)

        // Increment the velocity to smoothly transition to walking state (0.5)
        if (animator.GetFloat("velocity") < 0.5f)
        {
            float newVelocity = animator.GetFloat("velocity") + Time.deltaTime;
            animator.SetFloat("velocity", Mathf.Clamp(newVelocity, 0.0f, 0.5f));
        }

        // Decrement the velocity to smoothly transition to walking state (0.5)
        if (animator.GetFloat("velocity") > 0.5f)
        {
            float newVelocity = animator.GetFloat("velocity") - Time.deltaTime;
            animator.SetFloat("velocity", Mathf.Clamp(newVelocity, 0.5f, 1.0f));
        }

        // Set the patrol speed for the NavMeshAgent
        enemyAgent.speed = patrolSpeed;

        // Make the enemy patrol in a circular area around the patrol center
        patrolTimer += Time.deltaTime;
        Vector3 offset = new Vector3(Mathf.Sin(patrolTimer) * patrolRadius, 0, Mathf.Cos(patrolTimer) * patrolRadius);
        enemyAgent.SetDestination(patrolCenter + offset);

        // Patrol is always ongoing, so return RUNNING
        node_state = State.RUNNING;
        return node_state;
    }


}
