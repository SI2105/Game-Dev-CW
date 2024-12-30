using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LockOnNode : Node
{
    private NavMeshAgent enemyAgent;
    private EnemyAIController enemyAI;
    private Animator animator;
    private Transform player;

    public LockOnNode(NavMeshAgent enemyAgent, EnemyAIController enemyAI, Animator animator, Transform player)
    {
        this.enemyAgent = enemyAgent;
        this.enemyAI = enemyAI;
        this.animator = animator;
        this.player = player;
    }

    public override State Evaluate()
    {

        // Check if locked on
        if (enemyAI.lockOnSensor.objects.Count > 0)
        {
                enemyAgent.isStopped=true;
                Debug.LogError("Locked On");
                node_state = State.SUCCESS;
                return node_state;
        }


        float distanceToPlayer = Vector3.Distance(enemyAgent.transform.position, player.position);

        if (distanceToPlayer > 4f)
        {
            enemyAgent.isStopped=false;
            // Move towards the player
            enemyAgent.speed=1.5f;
            enemyAgent.SetDestination(player.position);

            // Adjust animator parameters for walking
            float velocityY = animator.GetFloat("velocityY");
            float velocityX = animator.GetFloat("velocityX");

            velocityY = Mathf.Lerp(velocityY, 0.5f, Time.deltaTime * 2f);
            velocityX = Mathf.Lerp(velocityX, 0.0f, Time.deltaTime * 2f);

            animator.SetFloat("velocityY", velocityY);
            animator.SetFloat("velocityX", velocityX);

            node_state = State.RUNNING;
        }
        else
        {
            enemyAgent.isStopped = true;
            // Lock on logic
            Vector3 directionToPlayer = (player.position - enemyAgent.transform.position).normalized;
            float dotProduct = Vector3.Dot(enemyAgent.transform.right, directionToPlayer);

            if (dotProduct < -0.1f) // Player is on the left
            {
                Debug.LogError("Player is on the left");
                // Strafe left
                float velocityX = animator.GetFloat("velocityX");
                float velocityY = animator.GetFloat("velocityY");

                velocityX = Mathf.Lerp(velocityX, -0.5f, Time.deltaTime * 2f);
                velocityY = Mathf.Lerp(velocityY, 0.0f, Time.deltaTime * 2f);

                animator.SetFloat("velocityX", velocityX);
                animator.SetFloat("velocityY", velocityY);

                RotateTowardsPlayer(directionToPlayer);
            }
            else if (dotProduct > 0.1f) // Player is on the right
            {
                Debug.LogError("Player is on the right");
                // Strafe right
                float velocityX = animator.GetFloat("velocityX");
                float velocityY = animator.GetFloat("velocityY");

                velocityX = Mathf.Lerp(velocityX, 0.5f, Time.deltaTime * 2f);
                velocityY = Mathf.Lerp(velocityY, 0.0f, Time.deltaTime * 2f);

                animator.SetFloat("velocityX", velocityX);
                animator.SetFloat("velocityY", velocityY);

                RotateTowardsPlayer(directionToPlayer);
            }
            else // Player is in front but possibly obscured
            {
                Debug.LogError("Player is hiding behind wall");
                // Stop and become idle
                enemyAgent.isStopped = true;

                float velocityX = animator.GetFloat("velocityX");
                float velocityY = animator.GetFloat("velocityY");

                velocityX = Mathf.Lerp(velocityX, 0.0f, Time.deltaTime * 2f);
                velocityY = Mathf.Lerp(velocityY, 0.0f, Time.deltaTime * 2f);

                animator.SetFloat("velocityX", velocityX);
                animator.SetFloat("velocityY", velocityY);
            }
            node_state = State.RUNNING;
        }

        return node_state;
    }

    private void RotateTowardsPlayer(Vector3 directionToPlayer)
    {
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        enemyAgent.transform.rotation = Quaternion.Lerp(enemyAgent.transform.rotation, targetRotation, Time.deltaTime * 5f);
    }
}
