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
        if (animator.GetBool("enemyHit") == true)
        {
            enemyAgent.isStopped = true;
            node_state = State.FAILURE;
            return State.FAILURE;
        }

        if (enemyAI.isAttacking == true && animator.GetBool("Surge") == false)
        {
            enemyAgent.isStopped = true;
            node_state = State.RUNNING;
            return State.RUNNING;
        }

        float distance = Vector3.Distance(enemyAgent.transform.position, enemyAI.playerTransform.position);

        Debug.Log(distance);

        // Hysteresis for <4f condition
        if (distance < 3.5f && !enemyAI.isAttacking)
        {
            enemyAgent.isStopped = true;
            
            enemyAI.isWalkingToPlayer=false;
            animator.SetBool("IsPlayingAction", false);

            // Gradually adjust animation parameters
            float velocityX = animator.GetFloat("velocityX");
            float velocityY = animator.GetFloat("velocityY");

            velocityX = Mathf.Lerp(velocityX, 0.0f, Time.deltaTime * 20f);
            velocityY = Mathf.Lerp(velocityY, -0.5f, Time.deltaTime * 20f);

            animator.SetFloat("velocityX", velocityX);
            animator.SetFloat("velocityY", velocityY);

            // Move the enemy backwards
            Vector3 backwardDirection = -enemyAgent.transform.forward; // Backward direction
            float backwardSpeed = 0.4f; // Adjust speed as needed
            enemyAgent.transform.position += backwardDirection * backwardSpeed * Time.deltaTime;

            node_state = State.RUNNING;
            return State.RUNNING;
        }

        if (enemyAI.attackSensor.objects.Count > 0)
        {
            enemyAI.isWalkingToPlayer=false;
            if (!enemyAI.isAttacking)
            {
                animator.SetBool("IsPlayingAction", false);
                // Gradually adjust animation parameters to idle
                float velocityX = animator.GetFloat("velocityX");
                float velocityY = animator.GetFloat("velocityY");

                velocityX = Mathf.Lerp(velocityX, 0.0f, Time.deltaTime * 2f);
                velocityY = Mathf.Lerp(velocityY, 0.0f, Time.deltaTime * 2f);

                animator.SetFloat("velocityX", velocityX);
                animator.SetFloat("velocityY", velocityY);
            }

            // Check if velocities have reached idle position
            if (animator.GetFloat("velocityX") < 0.01f && animator.GetFloat("velocityY") < 0.01f)
            {
                // Close-range attack
                animator.SetBool("IsPlayingAction", true);
                enemyAgent.isStopped = true;

                // Randomly choose between AttackRight or AttackLeft
                animator.SetTrigger("AttackLeft");
                enemyAI.isAttacking = true;
            }
            
        }
        else if (distance > 6f) // Hysteresis for >6f condition
        {
            enemyAI.isWalkingToPlayer=false;
            animator.SetBool("IsPlayingAction", true);
            // Long-range surge attack
            animator.SetTrigger("AttackLeft");
            enemyAI.isAttacking = true;

            // Set surge boolean
            animator.SetBool("Surge", true);

            enemyAgent.isStopped = false;

            // Calculate the direction vector from enemy to player
            Vector3 directionToPlayer = (enemyAI.playerTransform.position - enemyAgent.transform.position).normalized;

            // Offset the player's position by 2.5f in the direction away from the enemy
            Vector3 destination = enemyAI.playerTransform.position - directionToPlayer * 3f;

            // Calculate the distance to the player
            float distanceToPlayer = Vector3.Distance(enemyAI.playerTransform.position, enemyAgent.transform.position);

            // Set the speed as the difference in distance minus 2.5f (clamp to ensure positive speed)
            float adjustedSpeed = Mathf.Max(distanceToPlayer, 0f);
            enemyAgent.speed = adjustedSpeed;

            // Set the calculated destination
            enemyAgent.SetDestination(destination);
        }
        else if (distance >= 3.6f && distance <= 6f) // Hysteresis for 4f to 6f condition
        {
            Debug.LogError("walking to player");
            enemyAI.isWalkingToPlayer=true;
            enemyAgent.isStopped = false;
            enemyAgent.speed = 1.5f;
            animator.SetBool("IsPlayingAction", false);

            // Gradually adjust animation parameters
            float velocityX = animator.GetFloat("velocityX");
            float velocityY = animator.GetFloat("velocityY");

            velocityX = Mathf.Lerp(velocityX, 0.0f, Time.deltaTime * 4f);
            velocityY = Mathf.Lerp(velocityY, 0.5f, Time.deltaTime * 4f);

            animator.SetFloat("velocityX", velocityX);
            animator.SetFloat("velocityY", velocityY);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(enemyAI.playerTransform.position, out hit, 6.0f, NavMesh.AllAreas))
            {
                enemyAgent.SetDestination(hit.position);
                Debug.Log("Valid destination set");
            }
            else
            {
                Debug.LogError("Player position is not on NavMesh");
            }

        }

        node_state = State.RUNNING;
        return node_state;
    }
}
