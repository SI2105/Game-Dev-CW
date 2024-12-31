using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Node : MonoBehaviour
{
    //variable describing state of Node
    protected State node_state {get;set;}

    //method for evaluating state of Node
    public abstract State Evaluate();

    public State GetNodeState()
    {
        return node_state;
    }
}


public enum State{
    SUCCESS, FAILURE, RUNNING
}