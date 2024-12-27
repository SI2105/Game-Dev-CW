using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceNode : Node
{
    protected List<Node> children= new List<Node>();

    public SequenceNode(List<Node> children){
        this.children = children;
    }
    //implementation of abstract method Evaluate
    public override State Evaluate(){
        bool isAnyNodeRunning = false;
        foreach(Node child in children){

            switch(child.Evaluate()){
                case State.RUNNING:
                    isAnyNodeRunning = true;
                    break;
                case State.SUCCESS:
                    break;
                case State.FAILURE:
                    node_state = State.FAILURE;
                    return node_state;
                    break;
                default:
                    break;
            }
        }

        node_state = isAnyNodeRunning ? State.RUNNING : State.SUCCESS;
        return node_state;
    }
}
