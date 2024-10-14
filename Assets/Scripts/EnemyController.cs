using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public Transform player;
    public float sightAngle = 45f; 
    public float patrolRadius = 10f; 
    public float patrolSpeed = 3.5f;
    public float attackSpeed = 6f;
    public float strikeSpeed = 8f;  
    public float sightCheckInterval = 5f; 
    public float strikeRange = 1.5f;  
    public float stepBackDistance = 1.0f; 
    public float chargeDelay = 1.0f;

    private NavMeshAgent agent;
    private Vector3 patrolCenter;
    private float patrolTimer;
    private float sightCheckTimer;
    private bool isStriking = false;

    private enum EnemyState
    {
        Patrol,
        Attack
    }
    private EnemyState currentState = EnemyState.Patrol;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolCenter = transform.position; 
        sightCheckTimer = sightCheckInterval; 
    }

    void Update()
    {
        if (currentState == EnemyState.Attack)
        {
            sightCheckTimer -= Time.deltaTime;
            if (sightCheckTimer <= 0)
            {
                sightCheckTimer = sightCheckInterval; 
                if (!IsPlayerInSight())
                {
                    patrolCenter = transform.position;
                    patrolTimer = 0f;
                    currentState = EnemyState.Patrol;
                }
            }
        }
        else
        {
            if (IsPlayerInSight())
            {
                currentState = EnemyState.Attack;
                sightCheckTimer = sightCheckInterval; 
            }
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Attack:
                Attack();
                break;
        }
    }

    private void Patrol()
    {
        agent.speed = patrolSpeed;

        patrolTimer += Time.deltaTime;
        Vector3 offset = new Vector3(Mathf.Sin(patrolTimer) * patrolRadius, 0, Mathf.Cos(patrolTimer) * patrolRadius);
        agent.SetDestination(patrolCenter + offset);
    }

    private void Attack()
    {
        if (isStriking) return;  

        agent.speed = attackSpeed;

        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            
            if (distanceToPlayer <= strikeRange)
            {
                StartCoroutine(StrikePlayer());
            }
            else
            {
                
                agent.SetDestination(player.position);
            }
        }
    }

    private IEnumerator StrikePlayer()
    {
        isStriking = true;
        
        Vector3 directionAwayFromPlayer = (transform.position - player.position).normalized;
        Vector3 stepBackPosition = transform.position + directionAwayFromPlayer * stepBackDistance;
        agent.SetDestination(stepBackPosition);
        yield return new WaitForSeconds(chargeDelay);  

        
        agent.speed = strikeSpeed;
        agent.SetDestination(player.position);
        yield return new WaitForSeconds(0.5f); 
        isStriking = false;
    }

    private bool IsPlayerInSight()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float hearingRange = 1.5f;
        if (distanceToPlayer <= hearingRange)
        {
            return true; 
        }

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > sightAngle / 2)
        {
            return false; 
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer, out hit))
        {
            if (hit.transform == player)
            {
                return true; 
            }
        }

        return false;
    }
}
