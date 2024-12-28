using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Attack_1Node : Node
{
    private NavMeshAgent enemyAgent;
    private EnemyAIController enemyAI;
    private Animator animator;
    public Attack_1Node(NavMeshAgent enemyAgent, EnemyAIController enemyAI, Animator animator) {
        this.enemyAgent = enemyAgent;
        this.enemyAI = enemyAI;
        this.animator = animator;
    }

    public override State Evaluate(){
        //increment the velocity to 4.0 to make player transition to fighting idle state
        if(enemyAI.getEnemyVelocity()<4.0f){
            enemyAI.IncrementVelocity();
            animator.SetFloat("velocity", enemyAI.getEnemyVelocity());
        }

        //stop the agent from moving so that the attacks can initiate
        enemyAgent.isStopped = true;
        //set the node state to running and return it
        node_state=State.RUNNING;
        return node_state;
    }
}
