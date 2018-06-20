using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.Events;
using System.Collections.Generic;
/*
 * Agent Editor Script. Put inside Assets/Editors.
 * Notes: You can have duplicate state names right now...
 * Freaks out when you have this open + click play. Don't, uh, do that.
 * When you edit the functions, it resets the name of the state.
 */

//Script Menu. Adds a "Modify Agent" button to the script, that calls upon a seperate window.
[CustomEditor(typeof(AgentFSM))]
public class AgentEditor : Editor
{
    public override void OnInspectorGUI()
    {

        if (GUILayout.Button("Open Agent Editor"))
        {
            AgentEditorWindow.ShowWindow();
        }   
    }
}

/*
 * The seperate window that is called when you click the button in AgentEditor.
 * 
 * Current issues:
 * If you squish it too small, it gets weird. Don't do that.
 */
public class AgentEditorWindow : EditorWindow{

    AgentFSM _agent_fsm;
    SerializedObject _agent_fsm_serialized;
    Vector2 scrollPos;
    bool _loaded = false;

    public static void ShowWindow()
    {
        EditorWindow.GetWindow<AgentEditorWindow>("Agent Editor");
    }
   
    public void Instantiate()
    {
        _agent_fsm_serialized = new SerializedObject(_agent_fsm);
        _loaded = true;
    }

    void OnGUI()
    {
        if(!_loaded)
        {
            GUILayout.Label("Select an AgentFSM script attached to a gameobject.");
            _agent_fsm = (AgentFSM)EditorGUILayout.ObjectField(_agent_fsm, typeof(AgentFSM), true);
            if(GUILayout.Button("Load Script"))
            {
                if(_agent_fsm == null)
                {
                    ShowNotification(new GUIContent("朋友对不起, you should snag an AgentFSM script first!"));
                }
                else
                {
                    Instantiate();
                }
            }
        }
        else
        { 
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUILayout.Label(new GUIContent("ID : ", "The agent's ID. This is used in WatsonListener."));
            _agent_fsm._id = GUILayout.TextField(_agent_fsm._id);

            GUILayout.Label(new GUIContent("Currrent state : " + _agent_fsm.GetState(),
                 "The AgentFSM's current state. Upon starting the game, the state will default to the first state."));

            EditorGUILayout.PropertyField(_agent_fsm_serialized.FindProperty("_error_event"), new GUIContent("Error function",
                "Functions that are called when the agent is given an invalid input."));

            GUILayout.Label("States: ");
            GUILayout.Space(10);
            for (int i = 0; i < _agent_fsm._states.Count; i++)
            {
                GUILayout.Label(new GUIContent("Name: ", "The name of the state."));

                _agent_fsm._states[i] = GUILayout.TextField(_agent_fsm._states[i]);

                GUILayout.Label(new GUIContent("Events",
                    "The functions that will be called when this state is transitioned to."));

                EditorGUILayout.PropertyField(_agent_fsm_serialized.FindProperty("_events").GetArrayElementAtIndex(i), new GUIContent("Functions",
                    "The functions that will be called when this state is transitioned to."));

                GUILayout.BeginHorizontal();
                GUILayout.Space(Screen.width - 100);
                if (GUILayout.Button("Remove"))
                {
                    _agent_fsm._states.RemoveAt(i);
                    _agent_fsm._events.RemoveAt(i);
                    _agent_fsm_serialized = new SerializedObject(_agent_fsm);
                }
                GUILayout.EndHorizontal();
            }

            //When the player has set all the values for the agent, click on the button to create it!
            if (GUILayout.Button(new GUIContent("Add State", "Adds a new agent state.")))
            {
                _agent_fsm._events.Add(new AgentEvent());
                _agent_fsm._states.Add("New");
                _agent_fsm_serialized = new SerializedObject(_agent_fsm);
            }

            if (GUILayout.Button(new GUIContent("Apply Changes", "Confirms things made in the editor.")))
            {
                _agent_fsm_serialized.ApplyModifiedProperties();
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
