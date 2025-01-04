using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AttackPlayer : Node
{
    private Animator miniEnemyAnimator;
    private MiniEnemyAIController enemyController;

    public AttackPlayer(Animator miniEnemyAnimator, MiniEnemyAIController enemyController)
    {
        this.miniEnemyAnimator = miniEnemyAnimator;
        this.enemyController = enemyController;
    }

    public override State Evaluate()
    {
        // Set the attack trigger
        miniEnemyAnimator.SetTrigger("Attack");
        enemyController.attack=true;

        node_state = State.SUCCESS;
        return node_state;
    }
}
