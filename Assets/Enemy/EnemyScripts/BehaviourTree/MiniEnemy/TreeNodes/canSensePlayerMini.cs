using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class canSensePlayerMini : Node
{
    private MiniEnemyAIController enemy;

    public canSensePlayerMini(MiniEnemyAIController enemy)
    {
        this.enemy = enemy;
    }

    public override State Evaluate()
    {
        // Check if the player is detected by the sensor or within the sensing distance
        if (enemy.player_sensor.objects.Count > 0)
        {
            node_state = State.SUCCESS;
            return State.SUCCESS;
        }
        
        node_state = State.FAILURE;
        return State.FAILURE;
    }
}
