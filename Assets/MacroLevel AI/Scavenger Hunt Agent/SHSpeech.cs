using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHSpeech : MonoBehaviour {

    public string start_query;
    public string options_query;
    public string complete_query;
    public string finish_incomplete;
    public string finish_complete;
    public string give_hunt;
    public string hint;
    public string farewell;
    public string confusion;
    public string error;

    public WatsonTTS tts;

    public void Start_Query()
    {
        tts.Speak(start_query);
    }

    public void Give_Hunt()
    {
        tts.Speak(give_hunt);
    }

    public void Options_Query()
    {
        tts.Speak(options_query);
    }

    public void Complete_Query()
    {
        tts.Speak(complete_query);
    }

    public void Finish_Complete()
    {
        tts.Speak(finish_complete);
    }

    public void Finish_Incomplete()
    {
        tts.Speak(finish_incomplete);
    }

    public void Hint()
    {
        tts.Speak(hint);
    }

    public void Farewell()
    {
        tts.Speak(farewell);
    }

    public void Confusion()
    {
        tts.Speak(confusion);
    }

    public void Error()
    {
        tts.Speak(error);
    }

    public void Hello()
    {
        tts.Speak("hello there");
    }
}
