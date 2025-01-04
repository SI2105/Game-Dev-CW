using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolMap : Node
{
    private NavMeshAgent miniEnemyAgent;
    private Animator miniEnemyAnimator;
    private Vector3 patrolDestination;
    private float patrolRadius = 10f; // Radius around the zombie's current position to pick a random destination
    private bool isWalking = false;
    public PatrolMap(NavMeshAgent miniEnemyAgent, Animator miniEnemyAnimator)
    {
        this.miniEnemyAgent = miniEnemyAgent;
        this.miniEnemyAnimator = miniEnemyAnimator;
    }

    public override State Evaluate()
    {
        // If the agent has reached the destination or has no path
        if (!miniEnemyAgent.pathPending && miniEnemyAgent.remainingDistance <= miniEnemyAgent.stoppingDistance)
        {
            // Pick a new random destination within the patrol radius
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
            randomDirection += miniEnemyAgent.transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
            {
                patrolDestination = hit.position;
                miniEnemyAgent.SetDestination(patrolDestination);
            }
        }

       
        miniEnemyAnimator.SetBool("Walk", true);
       
       
        // Ensure the agent doesn't get stuck
        if (miniEnemyAgent.remainingDistance <=1f)
        {
            miniEnemyAgent.ResetPath();
        }

        node_state = State.RUNNING;
        return node_state;
    }
}
