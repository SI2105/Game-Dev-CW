using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Attack_2Node : Node
{
    private NavMeshAgent enemyAgent;
    private EnemyAIController enemyAI;

    public Attack_2Node(NavMeshAgent enemyAgent, EnemyAIController enemyAI) {
        this.enemyAgent = enemyAgent;
        this.enemyAI = enemyAI;
    }

    public override State Evaluate(){
        //stop the agent from moving so that the attacks can initiate
        enemyAgent.isStopped = true;
        //set the node state to running and return it
        node_state=State.RUNNING;
        return node_state;
    }
}
