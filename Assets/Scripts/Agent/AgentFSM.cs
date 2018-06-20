using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

// Should allow for Agents to be more easily created.

[System.Serializable,RequireComponent(typeof(WatsonHandler))]
public class AgentFSM : MonoBehaviour {

    [HideInInspector]
    public List<string> _states = new List<string> { "one", "two", "three" };
    [HideInInspector, SerializeField]
    public List<AgentEvent> _events = new List<AgentEvent>{new AgentEvent(), new AgentEvent(), new AgentEvent() };
    
    private string _current_state = "In Editor";
    public AgentEvent _error_event;
    public string _id;

    //Error state name; this doesn't really matter as long as its not a state.
    public static string _error_state = "_ERROR";

    /*
     * Call this function to transition to another state. If it contains the given state, it returns true,
     * sets the _current_state to the new state, and calls all functions related to that state. 
     * Else, it returns false.
     */

    private void Awake()
    {
        _current_state = _states[0];
    }

    public bool Transition(string next)
    {
        if(_states.Contains(next))
        {
            int y = _states.FindIndex(lambda => lambda == next);
            _events[y].Invoke();
            _current_state = next;
            return true;
        }
        else
        { 
            _error_event.Invoke();
        }
        return false;
    }
    

    //Returns the current state of the AgentFSM.
    public string GetState()
    {
        return _current_state;
    }
    
}



