using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthNode : Node
{
    private EnemyAIController enemy;

    private float threshold; // The threshold for the health

    public HealthNode(EnemyAIController enemy, float threshold){
        this.enemy = enemy;
        this.threshold = threshold;
    }
    
    public override State Evaluate(){
        if (enemy.getHealth() <= threshold)
        {
            node_state=State.SUCCESS;
            return State.SUCCESS;
        }
        else
        {
            node_state=State.FAILURE;
            return State.FAILURE;
        }
    }
}
