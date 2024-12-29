using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolNode : Node
{
    private NavMeshAgent enemyAgent;
    private EnemyAIController enemyAI;
    private Animator animator;
    private float patrolSpeed;

    private bool isTurning;

    public PatrolNode(NavMeshAgent enemyAgent, EnemyAIController enemyAI, float patrolSpeed, Animator animator)
    {
        this.enemyAgent = enemyAgent;
        this.enemyAI = enemyAI;
        this.patrolSpeed = patrolSpeed;
        this.animator = animator;
        isTurning = false;
    }

    public override State Evaluate()
    {
        // Check if AI should turn
        if (isTurning)
        {
            Debug.LogError("Turning");
            HandleTurning();
            node_state = State.RUNNING;
            return node_state;
        }

        // Adjust layer weights and animator booleans for patrolling
        animator.SetLayerWeight(0, 1.0f); // Movement Layer
        animator.SetLayerWeight(1, 0.0f); // Attack Layer
        animator.SetLayerWeight(2, 0.0f); // Turning Layer

        animator.SetBool("isPatrolling", true);
        animator.SetBool("isChasing", false);
        animator.SetBool("isIdle", false);
        animator.SetBool("IsRightTurning", false);
        animator.SetBool("IsLeftTurning", false);

        // Set patrol speed
        enemyAgent.speed = patrolSpeed;

        // Check if the AI detects a wall
        if (DetectWall())
        {
            Debug.LogError("Here");
            StartTurning();
        }
        else
        {
            // Move forward
            Vector3 forwardPosition = enemyAgent.transform.position + enemyAgent.transform.forward * 1.0f;
            enemyAgent.SetDestination(forwardPosition);
        }

        node_state = State.RUNNING;
        return node_state;
    }

    private bool DetectWall()
    {
        // Check if the sensor detects any obstacles (walls)
        return enemyAI.sensor.objects.Count > 0;
    }

    private void StartTurning()
    {
        isTurning = true;
        enemyAgent.ResetPath(); // Stop movement during turn

        if (enemyAI.sensor.objects.Count > 0)
        {
            GameObject obstacle = enemyAI.sensor.objects[0];
            Vector3 toObstacle = obstacle.transform.position - enemyAgent.transform.position;

            // Determine if the obstacle is to the right or left
            float dotProduct = Vector3.Dot(enemyAgent.transform.right, toObstacle.normalized);

            if (dotProduct > 0)
            {
                // Obstacle is on the right
                animator.SetBool("IsRightTurning", true);
                animator.SetBool("IsLeftTurning", false);
            }
            else
            {
                // Obstacle is on the left
                animator.SetBool("IsRightTurning", false);
                animator.SetBool("IsLeftTurning", true);
            }
        }
    }

    private void HandleTurning()
    {
        // Adjust layer weights for turning
        animator.SetLayerWeight(0, 0.0f); // Movement Layer
        animator.SetLayerWeight(2, 1.0f); // Turning Layer

        // Check if obstacles are cleared to stop turning
        if (enemyAI.sensor.objects.Count == 0)
        {
            animator.SetBool("IsRightTurning", false);
            animator.SetBool("IsLeftTurning", false);

            // Wait for the turning animation to finish before resuming forward movement
            if (animator.IsInTransition(2)) // Assuming Turning Layer is index 2
            {
                isTurning = false;
                animator.SetLayerWeight(0, 1.0f); // Movement Layer
                animator.SetLayerWeight(2, 0.0f); // Turning Layer
            }
        }
    }
}
