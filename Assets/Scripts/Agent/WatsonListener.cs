﻿using UnityEngine;
using System.Collections;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.DataTypes;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

/*
 * TODO: Set microphoneID.
 * 
 */ 

public class WatsonListener : MonoBehaviour {

    [SerializeField] List<GameObject> _agents;
    
    #region PLEASE SET THESE VARIABLES IN THE INSPECTOR
    private string _username = "e9d8dc4d-bf1e-496f-89e7-24e051f46bb2";
    private string _password = "VkjsSxbChLDw";
    private string _url = "https://stream.watsonplatform.net/speech-to-text/api";
    #endregion
    
    public Text ResultsField;

    private int _recordingRoutine = 0;
    public string _microphoneID = null;
    private AudioClip _recording = null;
    private int _recordingBufferSize = 1;
    private int _recordingHZ = 22050;

    private WatsonHandler _current_handler;

    //Fix this, this is rude.
    private AgentRabbitMQServer _rabbit_mq_server;
    private SpeechToText _speechToText;
   

    [Tooltip("Disable Watson because it's expensive.")]
    [SerializeField] bool _disable_watson = false;

    [Tooltip("Initial id of the agent that the listener initially talks to")]
    public string initial_talking_to = "One";

    //idk if this is the best way but I'm gng to represent all the agents by dictionary.
    private Dictionary<string, AgentFSM> _dictionary_agents;

	// Use this for initialization
	void Awake() {
        _dictionary_agents = new Dictionary<string, AgentFSM>();
    }
    

    private void Start()
    {
       if(!_disable_watson)
        { 
        LogSystem.InstallDefaultReactors();

        //  Create credential and instantiate service
        Credentials credentials = new Credentials(_username, _password, _url);

        _speechToText = new SpeechToText(credentials);
        Active = true;

        //TODO : MAKE SURE THIS ISN'T HARD CODED
        //SetAgentTalkingTo("Demo_Agent");
        SetAgentTalkingTo(initial_talking_to);
        StartRecording();
        }
        else
        {
            print("NOTE : Watson Listener is disabled!");
        }

        _rabbit_mq_server = GetComponent<AgentRabbitMQServer>();

    }

    public void SetAgentTalkingTo(string name)
    {
        var tmp = from x in _agents
                  where x.GetComponent<AgentFSM>()._id == name
                  select x.GetComponent<WatsonHandler>();

        _current_handler = tmp.Single();

    }

    public void SendToAgent(string message)
    {
        _current_handler.ParseInput(message);
    }

    public bool Active
    {
        get { return _speechToText.IsListening; }
        set
        {
            if (value && !_speechToText.IsListening)
            {
                _speechToText.DetectSilence = true;
                _speechToText.EnableWordConfidence = true;
                _speechToText.EnableTimestamps = true;
                _speechToText.SilenceThreshold = 0.01f;
                _speechToText.MaxAlternatives = 0;
                _speechToText.EnableInterimResults = true;
                _speechToText.OnError = OnError;
                _speechToText.InactivityTimeout = -1;
                _speechToText.ProfanityFilter = false;
                _speechToText.SmartFormatting = true;
                _speechToText.SpeakerLabels = false;
                _speechToText.WordAlternativesThreshold = null;
                _speechToText.StartListening(OnRecognize, OnRecognizeSpeaker);
            }
            else if (!value && _speechToText.IsListening)
            {
                _speechToText.StopListening();
            }
        }
    }

    public void StartRecording()
    {
        if (_recordingRoutine == 0)
        {
            UnityObjectUtil.StartDestroyQueue();
            _recordingRoutine = Runnable.Run(RecordingHandler());
        }
    }

    public void StopRecording()
    {
        if (_recordingRoutine != 0)
        {
            Microphone.End(_microphoneID);
            Runnable.Stop(_recordingRoutine);
            _recordingRoutine = 0;
        }
    }

    private void OnError(string error)
    {
        Active = false;

        Log.Debug("ExampleStreaming.OnError()", "Error! {0}", error);
    }

    private IEnumerator RecordingHandler()
    {
        Log.Debug("ExampleStreaming.RecordingHandler()", "devices: {0}", Microphone.devices);
        _recording = Microphone.Start(_microphoneID, true, _recordingBufferSize, _recordingHZ);
        yield return null;      // let _recordingRoutine get set..

        if (_recording == null)
        {
            StopRecording();
            yield break;
        }

        bool bFirstBlock = true;
        int midPoint = _recording.samples / 2;
        float[] samples = null;

        while (_recordingRoutine != 0 && _recording != null)
        {
            int writePos = Microphone.GetPosition(_microphoneID);
            if (writePos > _recording.samples || !Microphone.IsRecording(_microphoneID))
            {
                Log.Error("ExampleStreaming.RecordingHandler()", "Microphone disconnected.");

                StopRecording();
                yield break;
            }

            if ((bFirstBlock && writePos >= midPoint)
              || (!bFirstBlock && writePos < midPoint))
            {
                // front block is recorded, make a RecordClip and pass it onto our callback.
                samples = new float[midPoint];
                _recording.GetData(samples, bFirstBlock ? 0 : midPoint);

                AudioData record = new AudioData();
                record.MaxLevel = Mathf.Max(Mathf.Abs(Mathf.Min(samples)), Mathf.Max(samples));
                record.Clip = AudioClip.Create("Recording", midPoint, _recording.channels, _recordingHZ, false);
                record.Clip.SetData(samples, 0);

                _speechToText.OnListen(record);

                bFirstBlock = !bFirstBlock;
            }
            else
            {
                // calculate the number of samples remaining until we ready for a block of audio, 
                // and wait that amount of time it will take to record.
                int remaining = bFirstBlock ? (midPoint - writePos) : (_recording.samples - writePos);
                float timeRemaining = (float)remaining / (float)_recordingHZ;

                yield return new WaitForSeconds(timeRemaining);
            }

        }

        yield break;
    }

    private void OnRecognize(SpeechRecognitionEvent result, Dictionary<string, object> customData)
    {
        if (result != null && result.results.Length > 0)
        {
            foreach (var res in result.results)
            {
                foreach (var alt in res.alternatives)
                {
                    string text = string.Format("{0} ({1}, {2:0.00})\n", alt.transcript, res.final ? "Final" : "Interim", alt.confidence);
                    string input = alt.transcript;
                    Log.Debug("ExampleStreaming.OnRecognize()", text);

                    ResultsField.text = input;

                    if(res.final)
                    {
                        _rabbit_mq_server.Send(input);
                        print(_current_handler);
                        StopRecording();
                    }
                    
                }

                if (res.keywords_result != null && res.keywords_result.keyword != null)
                {
                    foreach (var keyword in res.keywords_result.keyword)
                    {
                        Log.Debug("ExampleStreaming.OnRecognize()", "keyword: {0}, confidence: {1}, start time: {2}, end time: {3}", keyword.normalized_text, keyword.confidence, keyword.start_time, keyword.end_time);
                    }
                }

                if (res.word_alternatives != null)
                {
                    foreach (var wordAlternative in res.word_alternatives)
                    {
                        Log.Debug("ExampleStreaming.OnRecognize()", "Word alternatives found. Start time: {0} | EndTime: {1}", wordAlternative.start_time, wordAlternative.end_time);
                        foreach (var alternative in wordAlternative.alternatives)
                            Log.Debug("ExampleStreaming.OnRecognize()", "\t word: {0} | confidence: {1}", alternative.word, alternative.confidence);
                    }
                }
            }
        }
    }

    private void OnRecognizeSpeaker(SpeakerRecognitionEvent result, Dictionary<string, object> customData)
    {
        if (result != null)
        {
            foreach (SpeakerLabelsResult labelResult in result.speaker_labels)
            {
                Log.Debug("ExampleStreaming.OnRecognize()", string.Format("speaker result: {0} | confidence: {3} | from: {1} | to: {2}", labelResult.speaker, labelResult.from, labelResult.to, labelResult.confidence));
            }
        }
    }

}
