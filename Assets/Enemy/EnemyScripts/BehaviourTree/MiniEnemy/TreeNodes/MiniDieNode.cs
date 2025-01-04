using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MiniDieNode : Node
{
    private Animator miniEnemyAnimator;
    private MiniEnemyAIController miniEnemyAI;
    private NavMeshAgent miniEnemyAgent;
    private bool isDead=false;
    public MiniDieNode(Animator miniEnemyAnimator, MiniEnemyAIController miniEnemyAI, NavMeshAgent miniEnemyAgent)
    {
        this.miniEnemyAnimator = miniEnemyAnimator;
        this.miniEnemyAI = miniEnemyAI;
        this.miniEnemyAgent = miniEnemyAgent;
    }

    public override State Evaluate()
    {
        if(isDead){
            node_state = State.SUCCESS;
            return node_state;
        }
        
        // Check if the enemy's health is 0
        if (miniEnemyAI.currentHealth<= 0)
        {
            // Set the Die trigger
            miniEnemyAnimator.SetTrigger("Die");
            miniEnemyAgent.isStopped = true;
            miniEnemyAgent.ResetPath();
            node_state = State.SUCCESS;
            isDead = true;
            return node_state;
        }

        node_state = State.FAILURE;
        return node_state;
    }
}
