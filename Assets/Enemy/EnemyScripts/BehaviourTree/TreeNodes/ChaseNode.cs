using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChaseNode : Node
{
    private NavMeshAgent enemyAgent;
    private Transform player;

    public ChaseNode(NavMeshAgent enemyAgent, Transform player){
        this.enemyAgent = enemyAgent;
        this.player = player;
    }

    public override State Evaluate(){
        //calculate distance between enemy and player
        float distance = Vector3.Distance(enemyAgent.transform.position, player.position);

        //if the distancce of the enemy is more than 0.2f, continue chasing
        if(distance >0.2f){
            enemyAgent.isStopped=false;
            enemyAgent.SetDestination(player.position);
            node_state=State.RUNNING;
            return node_state;
        }
        //otherwise stop as now, the enemy will go to attack mode
        else{
            enemyAgent.isStopped=true;
            node_state=State.SUCCESS;
            return node_state;
        }
    }
}
