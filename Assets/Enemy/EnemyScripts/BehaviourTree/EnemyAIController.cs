using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class EnemyAIController : MonoBehaviour
{
    //serializable field for the enemy starting health, will be 100
    [SerializeField] private float enemyStartingHealth;

    private float velocity =0.0f;
    private float acceleration = 3f;
    private float decceleration = 10f;
    private float maxVelocity = 4.0f;

    public EnemyAISensor sensor;

    public EnemyPlayerSensor player_sensor;
    //field for the enemy animator component
    Animator animator;
    // public field for the enemy transform
    public Transform enemyTransform;

    //float variable for the current enemy health
    private float enemyCurrentHealth;

    //threshold for when enemy should start chasing player
    [SerializeField] private float chasingThreshold;

    //threshold for when player should start attacking
    [SerializeField] private float attackingThreshold;

    //threshold for when low player health threshold
    [SerializeField] private float lowHealthThreshold;

    private NavMeshAgent enemyAgent;
    

    //public field for player transform
    public Transform playerTransform;

    //variable for top node in the behaviour tree
    Node topNode;

    private void Awake(){
        enemyAgent= GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        sensor = GetComponent<EnemyAISensor>();
        player_sensor = GetComponent<EnemyPlayerSensor>();
    }

    void Start(){
        enemyCurrentHealth = enemyStartingHealth;
        AssembleBehaviourTree();
    }

    public float getEnemyVelocity(){
        return velocity;
    }

    public void IncrementVelocity(){
        velocity+=Time.deltaTime * acceleration;
        if(velocity>maxVelocity){
            velocity=maxVelocity;
        }
    }

    public void DecrementVelocity(){
        velocity-=Time.deltaTime * decceleration; 
        if(velocity<0.0f){
            velocity=0.0f;
        }
    }

    void Update(){
        topNode.Evaluate();
    }
    //method for assembling the behaviour tree from defined nodes
    private void AssembleBehaviourTree(){

        // Define Individual Nodes in BehaviourTree

        //range node for attacking range
        IsInRangeNode isInAttackingRange = new IsInRangeNode(this, playerTransform, attackingThreshold);

        //range node for chasing range
        canSensePlayerNode isInChasingRange = new canSensePlayerNode(this);

    
        //health node
        HealthNode healthNode = new HealthNode(this, lowHealthThreshold);

        //health node for death
        HealthNode deathHealthNode = new HealthNode(this, 0);

        //node for death initation
        DieNode deathNode = new DieNode(this, enemyAgent);

        //node for chasing player
        ChaseNode chaseNode = new ChaseNode(enemyAgent, playerTransform, animator, this);

        //node for blocking
        BlockNode blockNode = new BlockNode(this, 0.5f);

        //node for dodging
        DodgeNode dodgeNode = new DodgeNode(this, 0.5f);

        //node for first set of attacks
        Attack_1Node attackNode1 = new Attack_1Node(enemyAgent, this, animator);

        //node for second set of attacks
        Attack_2Node attackNode2 = new Attack_2Node(enemyAgent, this, animator);

        //node for patrolling
        PatrolNode patrolNode = new PatrolNode(enemyAgent, this, 1f, animator);

        //Define Sequence and Selector Nodes in Behaviour Tree

        //Sequence node for chasing
        SequenceNode chaseSequence = new SequenceNode(new List<Node> {isInChasingRange, chaseNode});

        //Invertor node for determing if enemy is in patrolling range
        InverterNode isInPatrollingRange = new InverterNode(isInChasingRange);

        //Sequence node for patrolling
        SequenceNode patrollingSequence = new SequenceNode(new List<Node> {isInPatrollingRange, patrolNode});

        //Inverter node for health node for attacks 1
        InverterNode hasAttacks1Health = new InverterNode(healthNode);

        //Sequence node for attacks 1
        SequenceNode attacks1 = new SequenceNode(new List<Node> {isInAttackingRange, hasAttacks1Health, attackNode1});

        //Sequence node for attacks 2
        SequenceNode attacks2 = new SequenceNode(new List<Node> {isInAttackingRange, healthNode, attackNode2});

        //Sequence node for death
        SequenceNode enemyDeath = new SequenceNode(new List<Node> {deathHealthNode, deathNode});

        //selector node for blocking/dodging
        SelectorNode block_dodge = new SelectorNode(new List<Node> {blockNode, dodgeNode});


        //selector node for root node of behaviour tree
        topNode= new SelectorNode(new List<Node> {attacks1, attacks2, chaseSequence,patrollingSequence, enemyDeath, block_dodge});

    }
    //getter for the current enemy health
    public float getHealth(){
        return enemyCurrentHealth;
    }

    //getter method for the current enemy position
    public Vector3 getPosition(){
        return enemyTransform.position;
    }

    //getter method for chasing threshold
    public float getChasingThreshold(){
        return chasingThreshold;
    }

    //getter method for attacking threshold
    public float getAttackingThreshold(){
        return attackingThreshold;
    }

    //method for taking damage to be used by player class
    public void takeDamage(float damage){
        enemyCurrentHealth-=damage;
        //check if health is below 0 and if so reset it
        if(enemyCurrentHealth<0.0f){
            enemyCurrentHealth=0.0f;
        }
    }

    //method for initiating the death of the enemy
    public bool Die(){
        return true;
    }

    //method for initiating dodge
    public void dodge(){
       
    }

    //method for initiating block
    public void block(){
      
    }
}
