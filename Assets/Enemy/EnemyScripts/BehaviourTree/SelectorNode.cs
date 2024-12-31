using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorNode : Node
{
    protected List<Node> children= new List<Node>();

    public SelectorNode(List<Node> children){
        this.children = children;
    }
    //implementation of abstract method Evaluate
    public override State Evaluate(){
        foreach(Node child in children){

            switch(child.Evaluate()){
                case State.RUNNING:
                    node_state = State.RUNNING;
                    return node_state;
                    break;
                case State.SUCCESS:
                    node_state = State.SUCCESS;
                    return node_state;
                    break;
                case State.FAILURE:
                    break;
                default:
                    break;
            }
        }

        node_state = State.FAILURE;
        return node_state;
    }
}
