using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsInRangeNode : Node
{
    private EnemyAIController enemy;
    private Transform player;
    private float range; // The range within which the check is successful

    public IsInRangeNode(EnemyAIController enemy, Transform player, float range){
        this.enemy = enemy;
        this.player = player;
        this.range = range;
    }
    
    public override State Evaluate(){
        if (Vector3.Distance(enemy.getPosition(), player.position) <= range)
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
