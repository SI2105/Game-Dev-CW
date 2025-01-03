using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DodgeNode : Node
{
    private EnemyAIController enemyAI;
    private float dodgeProbability = 0.7f; // Set the dodge probability here (e.g., 50%)
    private Animator animator;
    private NavMeshAgent enemyAgent;
    private bool isDodging = false;
    private Vector3 dodgeDestination;
    private float dodgeSpeed = 3f;

    public DodgeNode(EnemyAIController enemyAI, Animator animator, NavMeshAgent enemyAgent)
    {
        this.enemyAI = enemyAI;
        this.animator = animator;
        this.enemyAgent = enemyAgent;
    }

    public override State Evaluate()
    {
        if(enemyAI.isAttacking){
            node_state=State.FAILURE;
            return node_state;
        }

         // Continue dodging
        if (isDodging)
        {
            Vector3 direction = (dodgeDestination - enemyAgent.transform.position).normalized;
            Rigidbody rb = enemyAgent.GetComponent<Rigidbody>();
            
            Vector3 toPlayer = (enemyAI.playerTransform.position - enemyAgent.transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(toPlayer);
            enemyAgent.transform.rotation = Quaternion.Slerp(
                enemyAgent.transform.rotation,
                targetRotation,
                Time.deltaTime * 10f
            );

            // Check if dodge is complete
            if (Vector3.Distance(enemyAgent.transform.position, dodgeDestination) < 0.1f)
            {
                Debug.LogError("dodging completed");
                isDodging = false;
                node_state=State.FAILURE;
                return node_state; // Dodge complete
            }

            node_state=State.SUCCESS;
            return node_state;
        }

    
        // Check if the player is attacking
        if (enemyAI.playerState.IsInState(PlayerAttackState.Attacking) && enemyAI.attackSensor.objects.Count>0)
        {
            // Generate a random value and compare it to dodge probability
            float randomValue = Random.value;
            if (randomValue < dodgeProbability)
            {
                // Start dodging if not already dodging
                if (!isDodging)
                {
                    Vector3 backwards = -enemyAgent.transform.forward;
                    Vector3 left = -enemyAgent.transform.right;
                    Vector3 right = enemyAgent.transform.right;

                    NavMeshHit hit;
                    Vector3 selectedDirection = Vector3.zero;
                    float maxDistance = 0f;
                    int dodgeIndex = -1;

                    // Check each direction and find the farthest valid position
                    if (NavMesh.SamplePosition(enemyAgent.transform.position + backwards * 8f, out hit, 10f, NavMesh.AllAreas))
                    {
                        float distance = Vector3.Distance(enemyAgent.transform.position, hit.position);
                        if (distance > maxDistance)
                        {
                            maxDistance = distance;
                            selectedDirection = hit.position;
                            dodgeIndex = 1; // Backward
                        }
                    }
                    if (NavMesh.SamplePosition(enemyAgent.transform.position + left * 8f, out hit, 10f, NavMesh.AllAreas))
                    {
                        float distance = Vector3.Distance(enemyAgent.transform.position, hit.position);
                        if (distance > maxDistance)
                        {
                            maxDistance = distance;
                            selectedDirection = hit.position;
                            dodgeIndex = 2; // Left
                        }
                    }
                    if (NavMesh.SamplePosition(enemyAgent.transform.position + right * 8f, out hit, 10f, NavMesh.AllAreas))
                    {
                        float distance = Vector3.Distance(enemyAgent.transform.position, hit.position);
                        if (distance > maxDistance)
                        {
                            maxDistance = distance;
                            selectedDirection = hit.position;
                            dodgeIndex = 0; // Right
                        }
                    }

                    // If a valid direction is found, set up dodging
                    if (maxDistance > 0f && dodgeIndex != -1)
                    {
                        dodgeDestination = selectedDirection;
                        animator.SetInteger("DodgeIndex", dodgeIndex); // Set dodge animation
                        isDodging = true;
                        enemyAI.isDodging = true; // Inform AI that dodging is active
                        enemyAgent.updateRotation = false;
                        enemyAgent.SetDestination(dodgeDestination);
                    }

                    Debug.LogError(maxDistance);
                }

                node_state = State.SUCCESS;
                return node_state;
            }
        }
    
        node_state = State.FAILURE;
        return node_state;

    }
}
