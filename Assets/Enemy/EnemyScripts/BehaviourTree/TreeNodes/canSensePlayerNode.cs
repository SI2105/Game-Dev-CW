using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class canSensePlayerNode : Node
{
    private EnemyAIController enemy;
   

    public canSensePlayerNode(EnemyAIController enemy){
        this.enemy = enemy;
    
    }
    
    public override State Evaluate(){
        if (enemy.player_sensor.objects.Count>0)
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
