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

    public LockOnNode(NavMeshAgent enemyAgent, EnemyAIController enemyAI, Animator animator, Transform player)
    {
        this.enemyAgent = enemyAgent;
        this.enemyAI = enemyAI;
        this.animator = animator;
        this.player = player;
    }

    public override State Evaluate()
    {
        if(animator.GetBool("enemyHit")==true){
            enemyAgent.isStopped = true;
            return State.FAILURE;
        }
        
        if(enemyAI.isAttacking==true){
            return State.FAILURE;
        }

        // Check if locked on
        if (enemyAI.lockOnSensor.objects.Count > 0)
        {       
                if(!enemyAI.isWalkingToPlayer){
                    enemyAgent.isStopped=true;
                }
                // Gradually adjust animation parameters to idle
                float velocityX = animator.GetFloat("velocityX");
                float velocityY = animator.GetFloat("velocityY");

                velocityX = Mathf.Lerp(velocityX, 0.0f, Time.deltaTime * 2f);
                velocityY = Mathf.Lerp(velocityY, 0.0f, Time.deltaTime * 2f);

                animator.SetFloat("velocityX", velocityX);
                animator.SetFloat("velocityY", velocityY);

                if(animator.GetFloat("velocityX") < 0.01f && animator.GetFloat("velocityY") < 0.01f){
                    node_state = State.SUCCESS;
                    return node_state;
                }
                else{
                    node_state=State.FAILURE;
                    return node_state;
                }
                
        }

        animator.SetBool("IsPlayingAction", false);
        float distanceToPlayer = Vector3.Distance(enemyAgent.transform.position, player.position);

        if (distanceToPlayer > 4f)
        {
            enemyAgent.isStopped=false;
            // Move towards the player
            enemyAgent.speed=1.5f;
            enemyAgent.SetDestination(player.position);

            // Adjust animator parameters for walking
            float velocityY = animator.GetFloat("velocityY");
            float velocityX = animator.GetFloat("velocityX");

            velocityY = Mathf.Lerp(velocityY, 0.5f, Time.deltaTime * 2f);
            velocityX = Mathf.Lerp(velocityX, 0.0f, Time.deltaTime * 2f);

            animator.SetFloat("velocityY", velocityY);
            animator.SetFloat("velocityX", velocityX);

            node_state = State.FAILURE;
        }
        else
        {
            enemyAgent.isStopped = true;

            // Lock on logic
            Vector3 directionToPlayer = (player.position - enemyAgent.transform.position).normalized;

            // Directions to check for obstacles
            Vector3 leftDirection = -enemyAgent.transform.right;
            Vector3 rightDirection = enemyAgent.transform.right;

            bool isObjectOnLeft = IsObjectOnSide(leftDirection);
            bool isObjectOnRight = IsObjectOnSide(rightDirection);

            RotateTowardsPlayer(directionToPlayer);

            // Determine if the player is on the left or right (relative to the enemy)
            if (player.position.x > enemyAgent.transform.position.x) // Player is on the left
            {
                if (isObjectOnLeft) // Obstacle on the left
                {
                    Debug.LogError("Obstacle on the left, strafing right and moving forward");

                    float velocityX = animator.GetFloat("velocityX");
                    float velocityY = animator.GetFloat("velocityY");

                    // Strafe right and move forward (along z-axis)
                    velocityX = Mathf.Lerp(velocityX, 0.5f, Time.deltaTime * 2f);
                    velocityY = Mathf.Lerp(velocityY, 0.5f, Time.deltaTime * 2f);

                    animator.SetFloat("velocityX", velocityX);
                    animator.SetFloat("velocityY", velocityY);

                    // Move right and forward
                    Vector3 moveDirection = rightDirection + enemyAgent.transform.forward;
                    enemyAgent.transform.position += moveDirection.normalized * Time.deltaTime * 2f;
                }
                else // No obstacle on the left, strafe left
                {
                    Debug.LogError("Strafing left");

                    float velocityX = animator.GetFloat("velocityX");
                    float velocityY = animator.GetFloat("velocityY");

                    // Gradually adjust animation parameters
                    velocityX = Mathf.Lerp(velocityX, -0.5f, Time.deltaTime * 2f);
                    velocityY = Mathf.Lerp(velocityY, 0.0f, Time.deltaTime * 2f);

                    animator.SetFloat("velocityX", velocityX);
                    animator.SetFloat("velocityY", velocityY);

                   enemyAgent.transform.position = new Vector3(enemyAgent.transform.position.x +(Time.deltaTime * 1f),enemyAgent.transform.position.y, enemyAgent.transform.position.z); 
                }
            }
            else // Player is on the right
            {
                if (isObjectOnRight) // Obstacle on the right
                {
                    Debug.LogError("Obstacle on the right, strafing left and moving forward");

                    float velocityX = animator.GetFloat("velocityX");
                    float velocityY = animator.GetFloat("velocityY");

                    // Strafe left and move forward (along z-axis)
                    velocityX = Mathf.Lerp(velocityX, -0.5f, Time.deltaTime * 2f);
                    velocityY = Mathf.Lerp(velocityY, 0.5f, Time.deltaTime * 2f);

                    animator.SetFloat("velocityX", velocityX);
                    animator.SetFloat("velocityY", velocityY);

                    // Move left and forward
                    Vector3 moveDirection = leftDirection + enemyAgent.transform.forward;
                    enemyAgent.transform.position += moveDirection.normalized * Time.deltaTime * 2f;
                }
                else // No obstacle on the right, strafe right
                {
                    Debug.LogError("Strafing right");

                    float velocityX = animator.GetFloat("velocityX");
                    float velocityY = animator.GetFloat("velocityY");

                    // Gradually adjust animation parameters
                    velocityX = Mathf.Lerp(velocityX, 0.5f, Time.deltaTime * 2f);
                    velocityY = Mathf.Lerp(velocityY, 0.0f, Time.deltaTime * 2f);

                    animator.SetFloat("velocityX", velocityX);
                    animator.SetFloat("velocityY", velocityY);

                    // Move right
                    enemyAgent.transform.position = new Vector3(enemyAgent.transform.position.x -(Time.deltaTime * 1f),enemyAgent.transform.position.y, enemyAgent.transform.position.z); 
                }
            }
        }

        
        node_state = State.FAILURE;

        return node_state;
    }

    private void RotateTowardsPlayer(Vector3 directionToPlayer)
    {
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        enemyAgent.transform.rotation = Quaternion.Lerp(enemyAgent.transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    // Function to detect if there's an obstacle on the left or right
    bool IsObjectOnSide(Vector3 direction)
    {
        RaycastHit hit;
        float sideCheckDistance = 1.5f; // Adjust as needed for the distance to check
        Vector3 origin = enemyAgent.transform.position + Vector3.up * 0.5f; // Offset origin to avoid ground collision

        // Perform the raycast in the given direction
        if (Physics.Raycast(origin, direction, out hit, sideCheckDistance))
        {
            Debug.Log($"Object detected on side: {hit.collider.name}");
            return true; // Object detected
        }
        return false; // No object detected
    }

}
