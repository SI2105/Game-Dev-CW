using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChasePlayer : Node
{
    private NavMeshAgent miniEnemyAgent;
    private Animator miniEnemyAnimator;
    private Transform playerTransform;
    private MiniEnemyAIController enemyController;
    private float chaseDistance = 2f; // Distance within which the chase is considered successful
  
    public ChasePlayer(NavMeshAgent miniEnemyAgent, Animator miniEnemyAnimator, Transform playerTransform, MiniEnemyAIController enemyController)
    {
        this.miniEnemyAgent = miniEnemyAgent;
        this.miniEnemyAnimator = miniEnemyAnimator;
        this.playerTransform = playerTransform;
        this.enemyController = enemyController;
    }

    public override State Evaluate()
    {
        
        if(enemyController.hit==true){
            node_state=State.FAILURE;
            return node_state;
        }
        
     
        if(enemyController.attack==true){
            node_state=State.FAILURE;
            return node_state;
        }

        if(miniEnemyAnimator.GetBool("Walk") !=true){
            miniEnemyAnimator.SetBool("Walk", true);
       }

        // Check the distance to the player
        float distanceToPlayer = Vector3.Distance(miniEnemyAgent.transform.position, playerTransform.position);

        if (distanceToPlayer <= chaseDistance)
        {
            // Stop chasing as the player is within range
            miniEnemyAgent.ResetPath();
            node_state = State.SUCCESS;
            return node_state;
        }

        // Set the destination to the player's position
        miniEnemyAgent.SetDestination(playerTransform.position);


        // If the path becomes invalid, consider this a failure
        if (miniEnemyAgent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            node_state = State.FAILURE;
            return node_state;
        }

        node_state = State.FAILURE;
        return node_state;
    }
}
