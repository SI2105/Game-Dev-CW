using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class canSensePlayerNode : Node
{
    private EnemyAIController enemy;
    private Transform player;
    private float senseDistance; // Maximum distance to sense the player

    public canSensePlayerNode(EnemyAIController enemy, Transform player, float senseDistance)
    {
        this.enemy = enemy;
        this.player = player;
        this.senseDistance = senseDistance;
    }

    public override State Evaluate()
    {
        // Check if the player is detected by the sensor or within the sensing distance
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, player.position);
        if (enemy.player_sensor.objects.Count > 0)
        {
            node_state = State.SUCCESS;
            return State.SUCCESS;
        }
        
        node_state = State.FAILURE;
        return State.FAILURE;
    }
}
