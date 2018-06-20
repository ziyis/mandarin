/**
* Copyright 2015 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

using UnityEngine;
using IBM.Watson.DeveloperCloud.Services.TextToSpeech.v1;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Utilities;
using System.Collections;
using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Connection;
using UnityEngine.UI;

public class WatsonTTS : MonoBehaviour
{
    #region PLEASE SET THESE VARIABLES IN THE INSPECTOR
    private string _username = "d8c60d5c-eb3b-45fe-9fea-7717e1f99d26";
    private string _password = "FBijzEbqdIrp";
    private string _url = "https://stream.watsonplatform.net/text-to-speech/api";
    #endregion

    [SerializeField] WatsonListener _watson_listener;

    //TODO: Refactor this becuase these should be for each? So we need to know which Agent is speaking, which willb e itneresting
    //but there also should be a central script that contains all the agents. Basically, refactoring.
    public Image _agent_output;

    [Tooltip("If true, WatsonListener will be listen when this is done speaking.")]
    public bool _reenable_listener = true;

    [Tooltip("Disable Watson because it's expensive.")]
    [SerializeField] bool _disable_watson = false;

    TextToSpeech _textToSpeech;
    //Use this to set the agent's transcript.
    private AgentUI _agent_ui;
    private bool _synthesizeTested = false;

    void Start()
    {
        if(!_disable_watson)
        { 
            LogSystem.InstallDefaultReactors();
            Credentials credentials = new Credentials(_username, _password, _url);
            _textToSpeech = new TextToSpeech(credentials);
        }
        else
        {
            print("NOTE : Watson TTS is disabled!");
        }
    }

    private IEnumerator Examples()
    {
        //  Synthesize
        Log.Debug("ExampleTextToSpeech.Examples()", "Attempting synthesize.");
        while (!_synthesizeTested)
            yield return null;
    }

    public void Speak(string text) {
        if (!_disable_watson)
            Runnable.Run(RunnableSpeak(text));
        else
        {
            print("WatsonTTS : " + text);

            _agent_output.GetComponentInChildren<Text>().text = text;
            ReactivateListener();
        }
    }

    

    private IEnumerator RunnableSpeak(string text)
    {
        _textToSpeech.Voice = VoiceType.en_US_Allison;
        _textToSpeech.ToSpeech(HandleToSpeechCallback, OnFail, text, true);

        while (!_synthesizeTested)
            yield return null;
    }

    private void HandleToSpeechCallback(AudioClip clip, Dictionary<string, object> customData = null)
    {
        PlayClip(clip);
    }

    private void PlayClip(AudioClip clip)
    {
        if (Application.isPlaying && clip != null)
        {
            GameObject audioObject = new GameObject("AudioObject");
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.spatialBlend = 0.0f;
            source.loop = false;
            source.clip = clip;
            source.Play();

            if(_reenable_listener)
                Invoke("ReactivateListener", clip.length);
            Destroy(audioObject, clip.length);

            _synthesizeTested = true;
        }
    }

    public void ReactivateListener()
    {
        _watson_listener.StartRecording();
    }

    private void OnFail(RESTConnector.Error error, Dictionary<string, object> customData)
    {
        Log.Error("ExampleTextToSpeech.OnFail()", "Error received: {0}", error.ToString());
    }
}
