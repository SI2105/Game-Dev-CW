using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class EnemyAIController : MonoBehaviour
{
    //serializable field for the enemy starting health, will be 100
    [SerializeField] private float enemyStartingHealth;

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
    }

    void Start(){
        enemyCurrentHealth = enemyStartingHealth;
        AssembleBehaviourTree();
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
        IsInRangeNode isInChasingRange = new IsInRangeNode(this, playerTransform, chasingThreshold);

    
        //health node
        HealthNode healthNode = new HealthNode(this, lowHealthThreshold);

        //health node for death
        HealthNode deathHealthNode = new HealthNode(this, 0);

        //node for death initation
        DieNode deathNode = new DieNode(this, enemyAgent);

        //node for chasing player
        ChaseNode chaseNode = new ChaseNode(enemyAgent, playerTransform);

        //node for blocking
        BlockNode blockNode = new BlockNode(this, 0.5f);

        //node for dodging
        DodgeNode dodgeNode = new DodgeNode(this, 0.5f);

        //node for checking if player is in sight
        IsInSightNode isInSight = new IsInSightNode(this, playerTransform);

        //node for first set of attacks
        Attack_1Node attackNode1 = new Attack_1Node(enemyAgent, this);

        //node for second set of attacks
        Attack_2Node attackNode2 = new Attack_2Node(enemyAgent, this);

        //node for patrolling
        PatrolNode patrolNode = new PatrolNode(enemyAgent, this, 10, 3.5f);

        //Define Sequence and Selector Nodes in Behaviour Tree


        //Selector node for sensing the player based on sight and hearing range
        SelectorNode canSensePlayer = new SelectorNode(new List<Node> {isInSight, isInChasingRange});

        //Sequence node for chasing
        SequenceNode chaseSequence = new SequenceNode(new List<Node> {canSensePlayer, chaseNode});

        //Invertor node for determing if enemy is in patrolling range
        InverterNode isInPatrollingRange = new InverterNode(canSensePlayer);

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
        topNode= new SelectorNode(new List<Node> {patrollingSequence, chaseSequence, attacks1, attacks2, enemyDeath, block_dodge});

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
