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
    public bool isAttacking{get;set;}
    public bool isWalkingToPlayer{get;set;}
    public EnemyPlayerSensor player_sensor;
    public EnemyWallSensor wall_sensor;
    public EnemyLockOnSensor lockOnSensor;
    public EnemyAttackSensor attackSensor;
    public bool isComboAttacking{get;set;}
    public bool shouldRoar{get;set;}
    public bool isDodging{get;set;}
    public bool shouldDodge{get;set;}
    public bool isDead{get;set;}

    public bool isRoaring;

    public EnemyAudioController audio_controller;
    //field for the enemy animator component
    Animator animator;
    // public field for the enemy transform
    public Transform enemyTransform;

    //float variable for the current enemy health
    [SerializeField] private float enemyCurrentHealth;

    //threshold for when low player health threshold
    [SerializeField] private float lowHealthThreshold;

    public float senseDistance;

    private NavMeshAgent enemyAgent;
    
    public PlayerState playerState;
    //public field for player transform
    public Transform playerTransform;

    //variable for top node in the behaviour tree
    Node topNode;

    private void Awake(){
        enemyAgent= GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player_sensor = GetComponent<EnemyPlayerSensor>();
        wall_sensor=GetComponent<EnemyWallSensor>();
        lockOnSensor=GetComponent<EnemyLockOnSensor>();
        attackSensor=GetComponent<EnemyAttackSensor>();
        audio_controller= GetComponent<EnemyAudioController>();
        playerState = playerTransform.GetComponent<PlayerState>();
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
        //range node for chasing range
        canSensePlayerNode isInChasingRange = new canSensePlayerNode(this, playerTransform, senseDistance);

        //health node
        HealthNode healthNode = new HealthNode(this, lowHealthThreshold);

        //health node for death
        HealthNode deathHealthNode = new HealthNode(this, 0);

        //node for death initation
        DieNode deathNode = new DieNode(this, enemyAgent, animator);

        //node for dodging
        DodgeNode dodgeNode = new DodgeNode(this, animator, enemyAgent);

        //node for first set of attacks
        Attack_1Node attackNode1 = new Attack_1Node(enemyAgent, this, animator);

        //node for second set of attacks
        Attack_2Node attackNode2 = new Attack_2Node(enemyAgent, this, animator);

        //node for patrolling
        PatrolNode patrolNode = new PatrolNode(enemyAgent, this, animator, audio_controller);

        //node for locking on to player
        LockOnNode playerLockNode = new LockOnNode(enemyAgent, this, animator, playerTransform);

        //node for getting a reaction
        DamagedNode damagedNode = new DamagedNode(this, animator, enemyAgent);

        //Define Sequence and Selector Nodes in Behaviour Tree
        
        //Invertor node for determing if enemy is in patrolling range
        InverterNode isInPatrollingRange = new InverterNode(isInChasingRange);

        //Sequence node for patrolling
        SequenceNode patrollingSequence = new SequenceNode(new List<Node> {isInPatrollingRange, patrolNode});

        //Inverter node for health node for attacks 1
        InverterNode hasAttacks1Health = new InverterNode(healthNode);

        //Sequence node for attacks 1
        SequenceNode attacks1 = new SequenceNode(new List<Node> {isInChasingRange, hasAttacks1Health, playerLockNode, attackNode1});

        //Sequence node for attacks 1
        SequenceNode attacks2 = new SequenceNode(new List<Node> {isInChasingRange, healthNode, playerLockNode, attackNode2});

        //Sequence node for death
        SequenceNode enemyDeath = new SequenceNode(new List<Node> {deathHealthNode, deathNode});

        //selector node for blocking/dodging
        SequenceNode dodge = new SequenceNode(new List<Node> {isInChasingRange, dodgeNode});

        //selector node for root node of behaviour tree
        topNode= new SelectorNode(new List<Node> {enemyDeath, damagedNode,dodge, patrollingSequence, attacks1, attacks2});
    }

    //getter for the current enemy health
    public float getHealth(){
        return enemyCurrentHealth;
    }

    //getter method for the current enemy position
    public Vector3 getPosition(){
        return enemyTransform.position;
    }

  
    //method for taking damage to be used by player class
    public void takeDamage(float damage){
        enemyCurrentHealth-=damage;
        //check if health is below 0 and if so reset it
        if(enemyCurrentHealth<0.0f){
            enemyCurrentHealth=0.0f;
        }
    }

    private void stopRoaring(){
        animator.SetBool("IsRoaring", false);
        audio_controller.stopSound();
        isRoaring=false;
        shouldRoar=false;
    }

    private void stopReaction(){
        animator.SetBool("enemyHit", false);
        shouldDodge=true;
    }

    private void stopSurge(){
        animator.SetBool("Surge", false);
    }

    private void stopAttack(){
        isAttacking=false;
    }

    private void stopCombo(){
        isComboAttacking=false;
    }

    private void stopDodge(){
        animator.SetInteger("DodgeIndex", -1);
        shouldDodge=false;
        isDodging=false;
    }

}
