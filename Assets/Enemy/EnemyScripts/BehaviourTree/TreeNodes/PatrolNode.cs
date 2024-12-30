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
        if (isRoaring)
        {
            HandleRoaring();
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

        // Adjust layer weights for patrolling
        animator.SetLayerWeight(0, 1.0f); // Movement Layer

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

            foreach (Vector3 direction in directions)
            {
                Vector3 potentialPosition = enemyAgent.transform.position + direction * 5f;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(potentialPosition, out hit, 10f, NavMesh.AllAreas))
                {
                    enemyAgent.SetDestination(hit.position);
                    break;
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
        enemyAgent.ResetPath();
        animator.SetLayerWeight(0, 0.0f); // Movement Layer
        animator.SetLayerWeight(1, 1.0f); // Roaring Layer
        animator.SetBool("IsRoaring", true);
        isRoaring = true;
        audio_controller.playRoar();
    }

    private void HandleRoaring()
    {
        if (animator.GetCurrentAnimatorStateInfo(1).IsName("Mutant Roaring"))
        {
            animator.SetBool("IsRoaring", false);
        }

        if (animator.GetBool("IsRoaring")==false && animator.GetCurrentAnimatorStateInfo(1).IsName("Not Roaring"))
        {
            isRoaring=false;
            audio_controller.stopSound();
            animator.SetLayerWeight(0, 1.0f); // Movement Layer
            animator.SetLayerWeight(1, 0.0f); // Roaring Layer
        }
    }
}
