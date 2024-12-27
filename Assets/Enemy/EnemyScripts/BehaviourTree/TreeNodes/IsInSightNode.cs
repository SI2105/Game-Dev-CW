using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsInSightNode : Node
{
    private EnemyAIController enemy;
    private Transform player;

    public IsInSightNode(EnemyAIController enemy, Transform player)
    {
        this.enemy = enemy;
        this.player = player;
    }

    public override State Evaluate()
    {
        RaycastHit hit; // Use RaycastHit, not Raycast
        Vector3 direction = player.position - enemy.getPosition(); // Get direction vector

        // Perform raycast
        if (Physics.Raycast(enemy.getPosition(), direction, out hit))
        {
            //if the raycast collides with the player, then the enemy can see the player
            if (hit.collider.transform == player)
            {
                node_state = State.SUCCESS;
                return State.SUCCESS;
            }
        }

        //otherwise set the state of the node to failure
        node_state = State.FAILURE;
        return State.FAILURE;
    }
}
