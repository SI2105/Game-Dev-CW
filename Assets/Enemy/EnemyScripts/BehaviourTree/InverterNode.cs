using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InverterNode : Node
{
    protected Node node;

    public InverterNode(Node node){
        this.node=node;
    }
    //implementation of abstract method Evaluate
    public override State Evaluate(){

        switch(node.Evaluate()){
            case State.RUNNING:
                node_state=State.RUNNING;
                break;
            case State.SUCCESS:
                node_state=State.FAILURE;
                break;
            case State.FAILURE:
                node_state=State.SUCCESS;
                break;
            default:
                break;
        }
        
        return node_state;

    }
}
