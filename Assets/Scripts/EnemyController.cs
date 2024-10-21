using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    public Transform player;
    public float sightAngle = 45f; 
    public float patrolRadius = 10f; 
    public float patrolSpeed = 3.5f;
    public float attackSpeed = 6f;
    public float strikeSpeed = 8f;  
    public float sightCheckInterval = 10f; 
    public float strikeRange = 1.5f;  
    public float stepBackDistance = 1.0f; 
    public float chargeDelay = 1.0f;
    private int HP = 100;
    public Slider HealthBar;
    private NavMeshAgent agent;
    private Vector3 patrolCenter;
    private float patrolTimer;
    private float sightCheckTimer;
    private bool isStriking = false;

    private Renderer enemyRenderer; 
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
        enemyRenderer = GetComponent<Renderer>(); 
        SetColor(Color.blue); 
    }

    void Update()
    {
        HealthBar.value = HP;


        if (HP <= 0)
        {
            SetColor(new Color(0.8f, 0.6f, 1.0f)); // Light purple when dead
            Die();
            return; // Skip all other updates if dead
        }

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
                    SetColor(Color.blue); // Dark blue for patrol mode
                }
            }
        }
        else
        {
            if (IsPlayerInSight())
            {
                currentState = EnemyState.Attack;
                sightCheckTimer = sightCheckInterval; 
                SetColor(Color.red); // Red for attack mode
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


    public void TakeDamage(int damage)
        {
            
                HP -= damage;
                HealthBar.value = HP;

                if (HP <= 0)
                {
                    Die();
                }
            
        }

    private void Die()
    {

    
        // Stop the enemy's movement by disabling the NavMeshAgent
        agent.isStopped = true;
        agent.enabled = false;
        agent.velocity = Vector3.zero;
       
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Enable gravity and physics interactions
        rb.isKinematic = false; // Ensure the Rigidbody isn't kinematic
        rb.useGravity = true;    // Enable gravity so the cube falls naturally

        
        rb.AddForce(Vector3.back * 2f, ForceMode.Impulse); // Pushes the cube backward
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
        SetColor(new Color(0.5f, 0.0f, 0.0f));
        Vector3 directionAwayFromPlayer = (transform.position - player.position).normalized;
        Vector3 stepBackPosition = transform.position + directionAwayFromPlayer * stepBackDistance;
        agent.SetDestination(stepBackPosition);
        yield return new WaitForSeconds(chargeDelay);  

        
        agent.speed = strikeSpeed;
        agent.SetDestination(player.position);
        yield return new WaitForSeconds(0.5f); 
        isStriking = false;
    }

    private void SetColor(Color color)
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = color; // Change the color of the enemy's material
        }
    }
    private bool IsPlayerInSight()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float hearingRange = 4;
        
        Debug.Log(distanceToPlayer);
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
