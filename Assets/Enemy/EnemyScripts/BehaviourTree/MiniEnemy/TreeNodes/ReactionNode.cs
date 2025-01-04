using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ReactionNode : Node
{
    private Animator miniEnemyAnimator;
    private MiniEnemyAIController miniEnemyAI;
    private float previousHealth;

    public ReactionNode(Animator miniEnemyAnimator, MiniEnemyAIController miniEnemyAI)
    {
        this.miniEnemyAnimator = miniEnemyAnimator;
        this.miniEnemyAI = miniEnemyAI;
        this.previousHealth = miniEnemyAI.currentHealth; // Initialize with the current health
    }

    public override State Evaluate()
    {

        if(miniEnemyAI.hit){
            node_state = State.SUCCESS;
            return node_state;
        }

        // Check if the health has decreased
        float currentHealth = miniEnemyAI.currentHealth;
        if (currentHealth < previousHealth)
        {
            // Set the Reaction trigger
            miniEnemyAnimator.SetTrigger("Reaction");

            // Update previous health
            previousHealth = currentHealth;

            miniEnemyAI.hit=true;

            node_state = State.SUCCESS;
            return node_state;
        }

        node_state = State.FAILURE;
        return node_state;
    }
}
