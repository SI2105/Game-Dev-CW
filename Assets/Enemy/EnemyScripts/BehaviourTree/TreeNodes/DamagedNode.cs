using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DamagedNode : Node
{
    private EnemyAIController enemyAI;
    private Animator animator;
    private float previousEnemyHealth;
    private NavMeshAgent enemyAgent;
    private bool isDodging = false;
    private bool hitReactionTriggered = false;
    private Vector3 dodgeDestination;
    private float dodgeSpeed = 3f;
   
    public DamagedNode(EnemyAIController enemyAI, Animator animator, NavMeshAgent enemyAgent)
    {
        this.enemyAI = enemyAI;
        this.animator = animator;
        this.enemyAgent = enemyAgent;
        previousEnemyHealth = enemyAI.getHealth(); // Initialize with the current health
    }

    public override State Evaluate()
    {
         if (isDodging)
        {
            // Move manually towards the dodge destination
            Vector3 direction = (dodgeDestination - enemyAgent.transform.position).normalized;
            enemyAgent.transform.position += direction * dodgeSpeed * Time.deltaTime;
            // Check if the enemy has reached the dodge destination
            if (Vector3.Distance(enemyAgent.transform.position, dodgeDestination) < 0.1f)
            {
                // Stop dodging
                isDodging = false;
                hitReactionTriggered = false; // Reset for future reactions
                enemyAgent.ResetPath();
                node_state = State.FAILURE; // Dodge complete, return FAILURE
                return node_state;
            }
            else
            {
                node_state = State.SUCCESS; // Still dodging, return SUCCESS
                return node_state;
            }
        }


        if (hitReactionTriggered && !enemyAI.shouldDodge)
        {
            node_state = State.SUCCESS; // Continue returning SUCCESS until shouldDodge is true
            return node_state;
        }

        if (hitReactionTriggered && enemyAI.shouldDodge && !isDodging)
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
                enemyAgent.updateRotation = false;
                enemyAgent.SetDestination(dodgeDestination);
            }
          

            node_state = State.SUCCESS; // Start dodging, return SUCCESS
            return node_state;
        }

        
        if (!hitReactionTriggered && enemyAI.getHealth() < previousEnemyHealth)
        {
            // Update the previous health value
            previousEnemyHealth = enemyAI.getHealth();

            // Trigger the hit reaction
            animator.SetBool("enemyHit", true);
            animator.SetBool("IsPlayingAction", true);

            hitReactionTriggered = true;
            node_state = State.SUCCESS; // Indicate the hit reaction started
            return node_state;
        }

        node_state = State.FAILURE;
        return node_state;
    }
}
