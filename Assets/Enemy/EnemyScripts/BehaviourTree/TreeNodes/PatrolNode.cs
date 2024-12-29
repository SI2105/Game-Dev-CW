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
    private float lastRoarTime = 0f; // Time of the last roar
    private float roarCooldown = 30f; // Minimum time between roars (in seconds)
    private bool isTurning;
    private bool isRoaring;
    private bool roaringStarted;
    private EnemyAudioController audio_controller;
    public PatrolNode(NavMeshAgent enemyAgent, EnemyAIController enemyAI, float patrolSpeed, Animator animator, EnemyAudioController audio_controller)
    {
        this.enemyAgent = enemyAgent;
        this.enemyAI = enemyAI;
        this.patrolSpeed = patrolSpeed;
        this.animator = animator;
        this.audio_controller = audio_controller;
        isTurning = false;
        isRoaring = false;
        roaringStarted = false;
    }

    public override State Evaluate()
    {
        // Roaring phase
        if (isRoaring)
        {
            HandleRoaring();
            node_state = State.RUNNING;
            return node_state;
        }

        // Check if AI should turn
        if (isTurning)
        {
            HandleTurning();
            node_state = State.RUNNING;
            return node_state;
        }
    

        // Randomly decide to roar
        if (Time.time - lastRoarTime >= roarCooldown && Random.Range(0, 100) < 2) // 2% chance to roar if cooldown has passed
        {
            lastRoarTime = Time.time; // Update the last roar time
            StartRoaring();
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

    private void StartRoaring()
    {
        isRoaring = true;
        enemyAgent.ResetPath();
        animator.SetLayerWeight(0, 0.0f); // Movement Layer
        animator.SetLayerWeight(3, 1.0f); // Roaring Layer
        animator.SetBool("IsRoaring", true);
        audio_controller.playRoar();
    }

    private void HandleRoaring()
    {
        if (animator.GetCurrentAnimatorStateInfo(3).IsName("Mutant Roaring"))
        {
            roaringStarted = true;
            animator.SetBool("IsRoaring", false);
        }

        if (animator.GetCurrentAnimatorStateInfo(3).IsName("Not Roaring") && roaringStarted)
        {
            audio_controller.stopSound();
            isRoaring = false;
            roaringStarted = false;
            animator.SetLayerWeight(0, 1.0f); // Movement Layer
            animator.SetLayerWeight(3, 0.0f); // Roaring Layer
            animator.SetBool("isPatrolling", true);
        }
        
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

        // Check 45 degrees to the left and right
        float leftCheck = CheckDirection(Quaternion.Euler(0, -45, 0) * enemyAgent.transform.forward);
        float rightCheck = CheckDirection(Quaternion.Euler(0, 45, 0) * enemyAgent.transform.forward);

        if (rightCheck > leftCheck)
        {
            // Turn right
            animator.SetBool("IsRightTurning", true);
            animator.SetBool("IsLeftTurning", false);
        }
        else
        {
            // Turn left
            animator.SetBool("IsRightTurning", false);
            animator.SetBool("IsLeftTurning", true);
        }
    }

    private float CheckDirection(Vector3 direction)
    {
        // Perform a raycast to check the distance to the nearest obstacle in the given direction
        RaycastHit hit;
        Vector3 origin = enemyAgent.transform.position + Vector3.up * 0.5f; // Offset the origin slightly upwards
        if (Physics.Raycast(origin, direction, out hit, 5.0f)) // Adjust the raycast length as needed
        {
            Debug.DrawRay(origin, direction * hit.distance, Color.red); // Visualize the raycast in the Scene view
            return hit.distance; // Return the distance to the obstacle
        }
        else
        {
            Debug.DrawRay(origin, direction * 5.0f, Color.green); // Visualize the raycast in the Scene view
            return Mathf.Infinity; // No obstacle detected
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
        else{
            StartTurning();
        }
    }
}
