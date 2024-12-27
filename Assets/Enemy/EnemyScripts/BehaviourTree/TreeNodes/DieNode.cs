using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class DieNode : Node
{
    private EnemyAIController enemyAI;
    private NavMeshAgent enemyAgent;

    public DieNode(EnemyAIController enemyAI, NavMeshAgent enemyAgent){
        this.enemyAI = enemyAI;
        this.enemyAgent = enemyAgent;
    }

    public override State Evaluate(){
        //stop the nav mesh agent and the call the Die method in enemy controller
        enemyAgent.isStopped=true;
        if(enemyAI.Die()){
            node_state=State.SUCCESS;
            return node_state;
        }
        else{
            node_state=State.FAILURE;
            return node_state;
        }
    }
}
