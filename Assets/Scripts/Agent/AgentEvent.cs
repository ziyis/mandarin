using System;
using UnityEngine.Events;
using UnityEngine;

/// <summary>
/// Unity Event that gets called when a state is transitioned from.
/// </summary>
[Serializable]
public class AgentEvent : UnityEvent {

    [Tooltip("The name of the state that this function will transition to.")]
    [SerializeField] public string _state;
}
