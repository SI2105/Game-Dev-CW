using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Attack_1Node : Node
{
    private NavMeshAgent enemyAgent;
    private EnemyAIController enemyAI;
    private Animator animator;
    private bool firstAttack=true;
    private float lastAttackTime = 0f; // Tracks the time of the last attack
    private float attackCooldown = 1f; // Minimum time between attacks (in seconds)
    float currentTime = Time.time;
    private bool hasAttacked=false;

    public Attack_1Node(NavMeshAgent enemyAgent, EnemyAIController enemyAI, Animator animator)
    {
        this.enemyAgent = enemyAgent;
        this.enemyAI = enemyAI;
        this.animator = animator;
    }

    public override State Evaluate()
    {

        if(enemyAI.isAttacking==false && hasAttacked){
            lastAttackTime=currentTime;
            hasAttacked = false;
        }

        currentTime = Time.time;

        if (currentTime - lastAttackTime >= attackCooldown)
        {
            if (!(enemyAI.attackSensor.objects.Count > 0))
            {
                // Calculate the direction vector from enemy to player
                Vector3 directionToPlayer = (enemyAI.playerTransform.position - enemyAgent.transform.position).normalized;

                // Gradually move towards the player
                float surgeSpeed = 10f;
                Vector3 movement = directionToPlayer * surgeSpeed * Time.deltaTime;

                // Update enemy position manually
                enemyAgent.transform.position += movement;
                
                if(enemyAI.isAttacking==false){
                    animator.SetTrigger("AttackLeft");
                    enemyAI.isAttacking = true;
                    hasAttacked=true;
                }
                  // Stop and trigger the attack
                enemyAgent.isStopped = true;
            }
            else{
                if(enemyAI.isAttacking==false){
                    animator.SetTrigger("AttackLeft");
                    enemyAI.isAttacking = true;
                    hasAttacked=true;
                }
                  // Stop and trigger the attack
                enemyAgent.isStopped = true;
            }
        }

        node_state = State.RUNNING;
        return node_state;
    }
}
