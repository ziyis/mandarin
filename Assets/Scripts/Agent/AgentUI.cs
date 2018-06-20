using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

//Does AgentFSM require this component?
public class AgentUI : MonoBehaviour {

    public Image _agent_output, _user_output;
    private Vector3 _agent_output_dist, _user_output_dist;

    //Change this later to use math isntead of eyeballs.
    private Vector3 _agent_offset = new Vector3(-150, 110, 0);
    private Vector3 _user_offset = new Vector3(150, 110, 0);

	// Use this for initialization
	void Start () {
        //todo: fix so ont hard code
        _agent_output_dist = _agent_output.rectTransform.position;
        _user_output_dist = _user_output.rectTransform.position;

    }
	
	// Update is called once per frame
	void Update () {

        RelocateOutputs();
	}

    public void UpdateAgentText(string newText)
    {
        _agent_output.GetComponentInChildren<Text>().text = newText;
    }

    public void UpdateUserText(string newText)
    {
        _user_output.GetComponentInChildren<Text>().text = newText;
    }

    private void RelocateOutputs()
    {
        _agent_output.rectTransform.position =
             Camera.main.WorldToScreenPoint(gameObject.transform.position) + _agent_offset;

        _user_output.rectTransform.position =
             Camera.main.WorldToScreenPoint(gameObject.transform.position) + _user_offset;
    }
}
