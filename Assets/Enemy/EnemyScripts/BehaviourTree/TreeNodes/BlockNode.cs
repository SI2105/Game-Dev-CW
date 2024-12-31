using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockNode : Node
{
    private EnemyAIController enemyAI;
    private float block_probability;

    public BlockNode(EnemyAIController enemyAI, float block_probability){
        this.enemyAI = enemyAI;
        this.block_probability = block_probability;
    }

    public override State Evaluate(){
        //if a random value is greater than the block probability, then initiate block
        if(Random.value >=block_probability){
            enemyAI.block();
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
