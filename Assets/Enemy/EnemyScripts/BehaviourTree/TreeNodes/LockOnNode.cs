using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LockOnNode : Node
{
    private NavMeshAgent enemyAgent;
    private EnemyAIController enemyAI;
    private Animator animator;
    private Transform player;

    // Animator velocities
    private float velocityX;
    private float velocityY;

    // Dodging
    private bool isDodging = false;
    private Vector3 dodgeDestination;
    private float dodgeSpeed = 10f;

    public LockOnNode(NavMeshAgent enemyAgent, EnemyAIController enemyAI, Animator animator, Transform player)
    {
        this.enemyAgent = enemyAgent;
        this.enemyAI = enemyAI;
        this.animator = animator;
        this.player = player;
    }

    public override State Evaluate()
    {

        // 4) If not dodging, always rotate to face the player
        Vector3 toPlayer = (player.position - enemyAgent.transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(toPlayer);
        enemyAgent.transform.rotation = Quaternion.Slerp(
            enemyAgent.transform.rotation,
            targetRotation,
            Time.deltaTime * 10f
        );

        if(enemyAI.isComboAttacking){
            // Gradually adjust velocity to idle
            velocityX = animator.GetFloat("velocityX");
            velocityY = animator.GetFloat("velocityY");

            velocityX = Mathf.Lerp(velocityX, 0.0f, Time.deltaTime * 2f);
            velocityY = Mathf.Lerp(velocityY, 0.0f, Time.deltaTime * 2f);

            animator.SetFloat("velocityX", velocityX);
            animator.SetFloat("velocityY", velocityY);
            node_state = State.SUCCESS;
            return node_state;
        }


        // 2) If currently attacking, do nothing else
        if (enemyAI.isAttacking)
        {
            // Gradually adjust velocity to idle
            velocityX = animator.GetFloat("velocityX");
            velocityY = animator.GetFloat("velocityY");

            velocityX = Mathf.Lerp(velocityX, 0.0f, Time.deltaTime * 2f);
            velocityY = Mathf.Lerp(velocityY, 0.0f, Time.deltaTime * 2f);

            animator.SetFloat("velocityX", velocityX);
            animator.SetFloat("velocityY", velocityY);
            node_state = State.FAILURE;
            return node_state;
        }

        // 6) If distance >= 6f, check for lock-on
        float distance = Vector3.Distance(enemyAgent.transform.position, player.position);

        if (enemyAI.lockOnSensor.objects.Count > 0) // Lock on if sensor detects the player
        {
            enemyAgent.isStopped = true;
            enemyAgent.ResetPath();

            // Gradually adjust velocity to idle
            velocityX = animator.GetFloat("velocityX");
            velocityY = animator.GetFloat("velocityY");

            velocityX = Mathf.Lerp(velocityX, 0.0f, Time.deltaTime * 2f);
            velocityY = Mathf.Lerp(velocityY, 0.0f, Time.deltaTime * 2f);

            animator.SetFloat("velocityX", velocityX);
            animator.SetFloat("velocityY", velocityY);

            node_state = State.SUCCESS;
            return node_state;
        }
        


        enemyAgent.isStopped=true;
        enemyAgent.ResetPath();

        // 5) Determine front vs. left vs. right via dot products
        float dotForward = Vector3.Dot(enemyAgent.transform.forward, toPlayer);
        float dotRight   = Vector3.Dot(enemyAgent.transform.right,   toPlayer);

        // Decide which way to move
        //   Front if dotForward is fairly high (say > 0.5)
        //   Right if dotRight   > 0
        //   Left  otherwise
        float moveSpeed = 2f; // Adjust strafe/walk speed as desired

        if (dotForward > 0.7f && !(enemyAI.attackSensor.objects.Count > 0))
        {
            // Gradually adjust velocityY to move forward
            velocityY = Mathf.Lerp(velocityY, 0.5f, Time.deltaTime * 10f);
            velocityX = Mathf.Lerp(velocityX, 0.0f, Time.deltaTime * 10f);

            // Move forward in local space
            enemyAgent.transform.position += enemyAgent.transform.forward * moveSpeed * Time.deltaTime;
        }
        else if (dotRight > 0f)
        {
            // Gradually adjust velocityX for strafing right
            velocityX = Mathf.Lerp(velocityX, 0.5f, Time.deltaTime * 10f);
            velocityY = Mathf.Lerp(velocityY, 0.0f, Time.deltaTime * 10f);

            // Strafe right
            enemyAgent.transform.position += enemyAgent.transform.right * moveSpeed * Time.deltaTime;
        }
        else
        {
            // Gradually adjust velocityX for strafing left
            velocityX = Mathf.Lerp(velocityX, -0.5f, Time.deltaTime * 10f);
            velocityY = Mathf.Lerp(velocityY, 0.0f, Time.deltaTime * 10f);

            // Strafe left
            enemyAgent.transform.position -= enemyAgent.transform.right * moveSpeed * Time.deltaTime;
        }

        // Update animator smoothly
        animator.SetFloat("velocityX", velocityX);
        animator.SetFloat("velocityY", velocityY);

        // 8) Default
        node_state = State.FAILURE;
        return node_state;
    }
}
