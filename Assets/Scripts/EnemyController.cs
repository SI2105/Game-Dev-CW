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
    private bool isDying = false; // Track if the enemy is in the dying process
    private float disintegrationSpeed = 0.5f; // Speed at which the enemy scales down
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


        if(!agent.enabled){
            return;
        }
        
        if (HP <= 0)
        {
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
                SetColor(Color.blue);
                Patrol();
                break;
            case EnemyState.Attack:
                SetColor(Color.red);
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
    if (!isDying)
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
        rb.isKinematic = false;
        rb.useGravity = true;

        // Apply a push force to make enemy fall
        rb.AddForce(Vector3.back * 10f, ForceMode.Impulse);
       

        // Start the disintegration (scaling down) process
        isDying = true;
    }

    // Disintegration process by decreasing the scale
    if (isDying)
    {
         // If the scale reaches zero or below, destroy the enemy object
        if (transform.localScale.x <= 0)
        {
            Destroy(gameObject);
            return;
        }
        // Decrease the scale of the enemy until it disappears
        if (transform.localScale.x > 0)
        {
            transform.localScale -= Vector3.one * disintegrationSpeed * Time.deltaTime; // Reduce the scale uniformly
        }

       
    }
}



    private void Attack()
    {

    
        if (isStriking){
            SetColor(new Color(0.5f, 0.0f, 0.0f));
            return;
        }

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
        SetColor(new Color(0.5f, 0.0f, 0.0f));
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

private void SetColor(Color baseColor)
{
    if (enemyRenderer != null)
    {
        // Calculate how many 20-HP increments are left
        int hpLevel = Mathf.FloorToInt(HP / 20); // Determine the range of HP in increments of 20

        // Map the hpLevel to different shades, turning whiter as HP decreases
        Color adjustedColor;

        switch (hpLevel)
        {
            case 5: // 100 to 81 HP
                adjustedColor = baseColor; // Full color at max HP
                break;
            case 4: // 80 to 61 HP
                adjustedColor = Color.Lerp(baseColor, Color.white, 0.2f); // Slightly whiter
                break;
            case 3: // 60 to 41 HP
                adjustedColor = Color.Lerp(baseColor, Color.white, 0.4f); // More whiter
                break;
            case 2: // 40 to 21 HP
                adjustedColor = Color.Lerp(baseColor, Color.white, 0.6f); // Even whiter
                break;
            case 1: // 20 to 1 HP
                adjustedColor = Color.Lerp(baseColor, Color.white, 0.8f); // Nearly white
                break;
            default: // 0 HP
                adjustedColor = Color.white; // Turn completely white when dead
                break;
        }

        // Apply the adjusted color to the enemy's material
        enemyRenderer.material.color = adjustedColor;
    }
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
