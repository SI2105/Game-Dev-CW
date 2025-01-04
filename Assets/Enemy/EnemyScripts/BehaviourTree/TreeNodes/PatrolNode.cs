using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolNode : Node
{
    private NavMeshAgent enemyAgent;
    private EnemyAIController enemyAI;
    private Animator animator;
    private float lastRoarTime = 0f; // Time of the last roar
    private float roarCooldown = 30f; // Minimum time between roars (in seconds)
    private bool isRoaring=false;
    private EnemyAudioController audio_controller;

    public PatrolNode(NavMeshAgent enemyAgent, EnemyAIController enemyAI, Animator animator, EnemyAudioController audio_controller)
    {
        this.enemyAgent = enemyAgent;
        this.enemyAI = enemyAI;
        this.animator = animator;
        this.audio_controller = audio_controller;
    }

    public override State Evaluate()
    {
        // Roaring phase
        if(enemyAI.isRoaring){
            node_state = State.RUNNING;
            return node_state;
        }

        animator.SetBool("IsPlayingAction", false);

        // Randomly decide to roar
        if (Time.time - lastRoarTime >= roarCooldown && Random.Range(0, 100) < 2) // 2% chance to roar if cooldown has passed
        {
            lastRoarTime = Time.time; // Update the last roar time
            StartRoaring();
            node_state = State.RUNNING;
            return node_state;
        }

        // Gradually adjust velocityX and velocityY
        float velocityX = animator.GetFloat("velocityX");
        float velocityY = animator.GetFloat("velocityY");

        if(velocityY<0.5){
            velocityY +=Time.deltaTime * 0.1f;
        }
        
        if(velocityY>0.5){
            velocityY=0.5f;
        }
    
        animator.SetFloat("velocityX", 0.0f);
        animator.SetFloat("velocityY", velocityY);

        // Set patrol speed
        enemyAgent.speed = Random.Range(1.0f, 1.5f);

        // Check for walls and adjust destination if necessary
        if (enemyAI.wall_sensor.objects.Count > 0)
        {
            Vector3[] directions = {
                enemyAgent.transform.forward,
                enemyAgent.transform.right,
                -enemyAgent.transform.right,
                -enemyAgent.transform.forward
            };

            Vector3 bestDirection = Vector3.zero;
            float maxDistance = float.MinValue;

            foreach (Vector3 direction in directions)
            {
                RaycastHit hit;
                float distance;

                // Cast a ray to measure distance to obstacle in the direction
                if (Physics.Raycast(enemyAgent.transform.position, direction, out hit, Mathf.Infinity))
                {
                    distance = hit.distance;
                }
                else
                {
                    // If no obstacle is detected, consider it as infinite distance
                    distance = Mathf.Infinity;
                }

                // Update the best direction if this direction has the maximum distance
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    bestDirection = direction;
                }
            }

            // Set destination in the direction with maximum distance
            if (bestDirection != Vector3.zero)
            {
                Vector3 potentialPosition = enemyAgent.transform.position + bestDirection * 5f;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(potentialPosition, out hit, 10f, NavMesh.AllAreas))
                {
                    enemyAgent.SetDestination(hit.position);
                }
            }
        }

        else if (!enemyAgent.hasPath || enemyAgent.remainingDistance <= enemyAgent.stoppingDistance)
        {
            Vector3 randomDirection = Random.insideUnitSphere * 10f;
            randomDirection += enemyAgent.transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas))
            {
                enemyAgent.SetDestination(hit.position);
            }
        }

        node_state = State.RUNNING;
        return node_state;
    }

    private void StartRoaring()
    {
        animator.SetBool("IsPlayingAction", true);
        enemyAgent.ResetPath();
        animator.SetBool("IsRoaring", true);
        enemyAI.isRoaring = true;
    }
}
