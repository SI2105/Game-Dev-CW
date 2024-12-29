using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChaseNode : Node
{
    private NavMeshAgent enemyAgent;
    private Transform player;
    private Animator animator;
    private EnemyAIController enemyAI;

    private bool hasRoared = false;
    private bool roaringStarted = false;
    private bool isTurning = false;

    private float sightLostTime = 0f; // Tracks the time when the player went out of sight
    private float sightCooldown = 2f; // Cooldown time to allow chasing to continue

    public ChaseNode(NavMeshAgent enemyAgent, Transform player, Animator animator, EnemyAIController enemyAI)
    {
        this.enemyAgent = enemyAgent;
        this.player = player;
        this.animator = animator;
        this.enemyAI = enemyAI;
    }

    public override State Evaluate()
    {
        // Roaring phase
        if (!hasRoared)
        {
            Debug.LogWarning("Here1");
            HandleRoaring();
            node_state = State.RUNNING;
            return node_state;
        }

        // Turning phase
        if (isTurning)
        {
            HandleTurning();
            node_state = State.RUNNING;
            return node_state;
        }

        // Chasing phase
        animator.SetBool("isChasing", true);
        animator.SetBool("isPatrolling", false);
        animator.SetBool("isIdle", false);

        float distance = Vector3.Distance(enemyAgent.transform.position, player.position);

        // Check if the player is in sight
        if (enemyAI.player_sensor.objects.Count == 0)
        {
            if (sightLostTime == 0f)
            {
                // Record the time when sight was lost
                sightLostTime = Time.time;
            }

            // Check if the cooldown has expired
            if (Time.time - sightLostTime > sightCooldown)
            {
                // Player has been out of sight for too long; turn toward them
                Vector3 toPlayer = (player.position - enemyAgent.transform.position).normalized;
                StartTurning(toPlayer);
                node_state = State.RUNNING;
                return node_state;
            }
        }
        else
        {
            // Player is in sight; reset sightLostTime
            sightLostTime = 0f;
        }

        // Chase the player if in sight or during the cooldown
        if (distance > 1.5f)
        {
            enemyAgent.speed = 2f;
            enemyAgent.isStopped = false;
            enemyAgent.SetDestination(player.position);

            node_state = State.RUNNING;
            return node_state;
        }
        else
        {
            // Stop the NavMeshAgent and transition to attack mode
            enemyAgent.isStopped = true;
            node_state = State.FAILURE; // Transition to AttackNode
            return node_state;
        }
    }

    private void StartTurning(Vector3 directionToPlayer)
    {
        isTurning = true;
        enemyAgent.ResetPath(); // Stop movement during turn

        // Calculate the angle to the player
        Vector3 forward = enemyAgent.transform.forward;
        float angleToPlayer = Vector3.SignedAngle(forward, directionToPlayer, Vector3.up);

        // Determine the optimal direction
        if (angleToPlayer > 0)
        {
            // Right turn required
            animator.SetBool("IsRightTurning", true);
            animator.SetBool("IsLeftTurning", false);
        }
        else
        {
            // Left turn required
            animator.SetBool("IsRightTurning", false);
            animator.SetBool("IsLeftTurning", true);
        }
    }

    private void HandleTurning()
    {
        // Adjust layer weights for turning
        animator.SetLayerWeight(0, 0.0f); // Movement Layer
        animator.SetLayerWeight(2, 1.0f); // Turning Layer

        // Check if the player is visible
        if (enemyAI.player_sensor.objects.Count > 0) // Player is now in sight
        {
            // Stop turning and reset animations
            animator.SetBool("IsRightTurning", false);
            animator.SetBool("IsLeftTurning", false);

            if (animator.IsInTransition(2)) // Assuming Turning Layer is index 2
            {
                isTurning = false;
                animator.SetLayerWeight(0, 1.0f); // Movement Layer
                animator.SetLayerWeight(2, 0.0f); // Turning Layer
            }
        }
        else
        {
            // Continue turning dynamically
            Vector3 toPlayer = (player.position - enemyAgent.transform.position).normalized;
            StartTurning(toPlayer);
        }
    }

    private void HandleRoaring()
    {
        enemyAgent.isStopped = true;
        animator.SetLayerWeight(0, 0.0f);
        animator.SetLayerWeight(3, 1.0f);
        animator.SetBool("IsRoaring", true);
        enemyAI.audio_controller.playRoar();
        if (animator.GetCurrentAnimatorStateInfo(3).IsName("Mutant Roaring"))
        {
            roaringStarted = true;
            animator.SetBool("IsRoaring", false);
        }

        if (animator.GetCurrentAnimatorStateInfo(3).IsName("Not Roaring") && roaringStarted)
        {
            hasRoared = true;
            roaringStarted = false;
            animator.SetLayerWeight(0, 1.0f);
            animator.SetLayerWeight(3, 0.0f);
            enemyAI.audio_controller.stopSound();
        }
    }
}
