using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DodgeNode : Node
{
    private EnemyAIController enemyAI;
    private float dodge_probability;

    public DodgeNode(EnemyAIController enemyAI, float dodge_probability){
        this.enemyAI = enemyAI;
        this.dodge_probability = dodge_probability;
    }

    public override State Evaluate(){
        //if a random value is greater than the dodge probability, then initiate block
        if(Random.value >=dodge_probability){
            enemyAI.dodge();
            node_state= State.SUCCESS;
            return node_state;
        }
        //else just set the state of node to failure
        else{
            node_state= State.FAILURE;
            return node_state;
        }
    }

    
}
