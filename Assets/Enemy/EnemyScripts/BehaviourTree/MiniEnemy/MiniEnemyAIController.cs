using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class MiniEnemyAIController : MonoBehaviour
{

    private NavMeshAgent miniEnemyAgent;
    private Animator miniEnemyAnimator;
    public Transform player;
    public MiniEnemyPlayerSensor player_sensor;
    public bool attack{get;set;}
    public float startingHealth;
    public float currentHealth;
    public bool hit{get;set;}

    public MiniEnemyAudioController audio_controller;
    //variable for top node in the behaviour tree
    Node topNode;
    
    void Awake(){
        miniEnemyAgent= GetComponent<NavMeshAgent>();
        miniEnemyAnimator = GetComponent<Animator>();
        player_sensor = GetComponent<MiniEnemyPlayerSensor>();
        audio_controller = GetComponent<MiniEnemyAudioController>();
    }

    void Start(){
        AssembleBehaviourTreeMini();
    }

    private void AssembleBehaviourTreeMini(){
        canSensePlayerMini isInRangeChasing = new canSensePlayerMini(this);

        InverterNode isInRangePatrolling = new InverterNode(isInRangeChasing);

        ChasePlayer chasePlayer = new ChasePlayer(miniEnemyAgent, miniEnemyAnimator, player, this);

        MiniDieNode miniDieNode = new MiniDieNode(miniEnemyAnimator, this, miniEnemyAgent);

        ReactionNode reactionNode = new ReactionNode(miniEnemyAnimator, this);

        PatrolMap patrolMap = new PatrolMap(miniEnemyAgent, miniEnemyAnimator);

        AttackPlayer attackPlayer = new AttackPlayer(miniEnemyAnimator, this);


         //Sequence node for patrolling
        SequenceNode patrol = new SequenceNode(new List<Node> {isInRangePatrolling, patrolMap});

        //Sequence node for attack
        SequenceNode attack = new SequenceNode(new List<Node> {isInRangeChasing, chasePlayer, attackPlayer});

    
        //selector node for root node of behaviour tree
        topNode= new SelectorNode(new List<Node> {miniDieNode, reactionNode, patrol, attack});

    }

    void Update(){
        topNode.Evaluate();
    }

    private void endAttack(){
        attack=false;
    }

    private void endHit(){
        hit=false;
    }

    private void playWalk(){
        audio_controller.playWalk();
    }

   private void Die()
    {
        Destroy(this.gameObject);
    }
    
    private void playAttack(){
        audio_controller.playAttack();
    }
}
