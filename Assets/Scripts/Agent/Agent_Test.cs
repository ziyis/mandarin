using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Script used to test the functions of AgentFSM / WatsonHandler.
 */ 
public class Agent_Test : MonoBehaviour {

    public string greeting;
    public string tablefull;
    public WatsonTTS tts;

    public void Greeting()
    {
        tts.Speak(greeting);
    }

    public void TableFull()
    {
        tts.Speak(tablefull);
    }

    public void Error()
    {
        tts.Speak("I have no idea what you're saying.");
    }

    public void Stack()
    {
        tts.Speak("This is proof of two events being invoked!");
    }

    

}
