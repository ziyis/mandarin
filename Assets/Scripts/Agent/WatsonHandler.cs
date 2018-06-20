using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Handles Watson's input. It gets intentions from Watson and sends them to the AgentFSM.
 * Bug: If you try to parse input as soon as Awake is finished, it doesn't work.
 */
 
public class WatsonHandler : MonoBehaviour {
    
    AgentFSM _agent_fsm;

    private void Awake()
    {
        _agent_fsm = GetComponent<AgentFSM>();
    }

    //Give us Watson's output.
    public void ParseInput(string NewValue)
    {
        print("Output is " + NewValue);
        _agent_fsm.Transition(NewValue);
    }


}
