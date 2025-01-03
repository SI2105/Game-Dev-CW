using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class DieNode : Node
{
    private EnemyAIController enemyAI;
    private NavMeshAgent enemyAgent;
    private Animator animator;
    public DieNode(EnemyAIController enemyAI, NavMeshAgent enemyAgent, Animator animator){
        this.enemyAI = enemyAI;
        this.enemyAgent = enemyAgent;
        this.animator = animator;
    }

    public override State Evaluate(){
        if(!enemyAI.isDead){
            //stop the nav mesh agent and the call the Die method in enemy controller
            enemyAgent.isStopped=true;
            enemyAI.isDead=true;
            animator.SetTrigger("Death");
        }
        node_state=State.SUCCESS;
        return node_state;
    }
}
