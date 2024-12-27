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

    public PatrolNode(NavMeshAgent enemyAgent, EnemyAIController enemyAI, float patrolRadius, float patrolSpeed)
    {
        this.enemyAgent = enemyAgent;
        this.enemyAI = enemyAI;
        this.patrolCenter = enemyAgent.transform.position;
        this.patrolRadius = patrolRadius;
        this.patrolSpeed = patrolSpeed;

        patrolTimer = Random.Range(0f, Mathf.PI * 2); // Randomize the starting point for varied behavior
    }

    public override State Evaluate()
    {
        // Make the enemy patrol in a circular area around the patrol center
        enemyAgent.speed = patrolSpeed;

        patrolTimer += Time.deltaTime;
        Vector3 offset = new Vector3(Mathf.Sin(patrolTimer) * patrolRadius, 0, Mathf.Cos(patrolTimer) * patrolRadius);

        enemyAgent.SetDestination(patrolCenter + offset);

        // Patrol is always ongoing, so return RUNNING
        node_state = State.RUNNING;
        return node_state;
    }
}
