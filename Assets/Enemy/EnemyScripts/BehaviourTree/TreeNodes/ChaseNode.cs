using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChaseNode : Node
{
    private NavMeshAgent enemyAgent;
    private Transform player;
    private Animator animator;
    private EnemyAIController enemyAI; ///
    private float velocity =1.0f;
    public ChaseNode(NavMeshAgent enemyAgent, Transform player, Animator animator, EnemyAIController enemyAI){
        this.enemyAgent = enemyAgent;
        this.player = player;
        this.animator = animator;
        this.enemyAI = enemyAI;
    }

    public override State Evaluate(){
        //increment the velocity to 2.0f to make player transition to running state
        if(enemyAI.getEnemyVelocity()<2.0f){
            // Debug.LogError(enemyAI.getEnemyVelocity());
            enemyAI.IncrementVelocity();
            animator.SetFloat("velocity", enemyAI.getEnemyVelocity());
        }

        //decrement the velocity to 2.0f to make player transition back to chasing state
        if(enemyAI.getEnemyVelocity()>2.0f){
            enemyAI.DecrementVelocity();
            animator.SetFloat("velocity", enemyAI.getEnemyVelocity());
        }

        //calculate distance between enemy and player
        float distance = Vector3.Distance(enemyAgent.transform.position, player.position);

        //if the distancce of the enemy is more than 0.2f, continue chasing
        if(distance>3f){
            enemyAgent.isStopped=false;
            enemyAgent.SetDestination(player.position);
            node_state=State.RUNNING;
            return node_state;
        }
        //otherwise stop as now, the enemy will go to attack mode
        else{
            enemyAgent.isStopped=true;
            node_state=State.FAILURE;
            return node_state;
        }
    }
}
