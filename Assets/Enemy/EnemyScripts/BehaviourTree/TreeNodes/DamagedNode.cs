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
    private float dodgeSpeed = 10f;
   
    public DamagedNode(EnemyAIController enemyAI, Animator animator, NavMeshAgent enemyAgent)
    {
        this.enemyAI = enemyAI;
        this.animator = animator;
        this.enemyAgent = enemyAgent;
        previousEnemyHealth = enemyAI.getHealth(); // Initialize with the current health
    }

    public override State Evaluate()
    {
        
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

        if (hitReactionTriggered && !enemyAI.shouldDodge)
        {
            node_state = State.SUCCESS; // Continue returning SUCCESS until shouldDodge is true
            return node_state;
        }

        if (hitReactionTriggered && enemyAI.shouldDodge && !isDodging)
        {
            // Determine the free direction for dodging
            Vector3 backwards = -enemyAgent.transform.forward;
            Vector3 left = -enemyAgent.transform.right;
            Vector3 right = enemyAgent.transform.right;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(enemyAgent.transform.position + backwards * 5f, out hit, 10f, NavMesh.AllAreas))
            {
                // Move backwards as far as possible (up to 5f)
                dodgeDestination = hit.position;
                animator.SetInteger("DodgeIndex", 1); // Dodge backwards
                isDodging = true;

                // Rotate the enemy by 90 degrees on the Y-axis
                enemyAgent.transform.Rotate(0f, 90f, 0f);
            }
            else if (NavMesh.SamplePosition(enemyAgent.transform.position + left * 5f, out hit, 10f, NavMesh.AllAreas))
            {
                // Move left as far as possible (up to 5f)
                dodgeDestination = hit.position;
                animator.SetInteger("DodgeIndex", 2); // Dodge left
                isDodging = true;
            }
            else if (NavMesh.SamplePosition(enemyAgent.transform.position + right * 5f, out hit, 10f, NavMesh.AllAreas))
            {
                // Move right as far as possible (up to 5f)
                dodgeDestination = hit.position;
                animator.SetInteger("DodgeIndex", 0); // Dodge right
                isDodging = true;
            }

            node_state = State.SUCCESS; // Start dodging, return SUCCESS
            return node_state;
        }

        if (isDodging)
        {
            // Move manually towards the dodge destination
            Vector3 direction = (dodgeDestination - enemyAgent.transform.position).normalized;
            enemyAgent.transform.position += direction * dodgeSpeed * Time.deltaTime;

            // Check if the enemy has reached the dodge destination
            if (Vector3.Distance(enemyAgent.transform.position, dodgeDestination) <= 0.1f)
            {
                // Stop dodging
                isDodging = false;
                hitReactionTriggered = false; // Reset for future reactions
                node_state = State.FAILURE; // Dodge complete, return FAILURE
                return node_state;
            }
            else
            {
                node_state = State.SUCCESS; // Still dodging, return SUCCESS
                return node_state;
            }
        }

        node_state = State.FAILURE;
        return node_state;
    }
}
