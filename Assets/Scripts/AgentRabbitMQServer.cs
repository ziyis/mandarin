using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.IO;
using System.Text;
using CymaticLabs.Unity3D.Amqp;

[RequireComponent(typeof(WatsonListener))]
public class AgentRabbitMQServer : MonoBehaviour
{
    [SerializeField] public static AmqpClient client;

    private static string send_to_backend_exchange = "send_to_backend";
    private static string vhost = "mandarin_project";
    private WatsonListener _watson_listener;

    const string CONFIG_FILE = "Config.json";

    public void Start()
    {
        // Load the config file from StreamingAssets.
        string configText = "";
        bool configError = false;

        try
        {
            configText = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, CONFIG_FILE));
        }
        catch
        {
            configError = true;
        }


        if (string.IsNullOrEmpty(configText))
            configError = true;

        var amqpConnection = new AmqpConnection();
        if (!configError)
        {
            var config = StompConfig.ConvertToObject(configText);
            amqpConnection.Name = "StompConfig";
            amqpConnection.Host = "localhost";
            amqpConnection.AmqpPort = 5672;// int.Parse(config.port);
            amqpConnection.Username = "guest"; //config.username;
            amqpConnection.Password = "guest"; //config.password;
            amqpConnection.VirtualHost = vhost;//config.virtual_host;
            amqpConnection.WebPort = 15674;
            amqpConnection.ReconnectInterval = 5;
            amqpConnection.RequestedHeartBeat = 30;
        }
        else
        {
            print("ERROR : Config file invalid!!!!!!!!");
        }

        AmqpClient.AddConnection(amqpConnection);

        client = GetComponent<AmqpClient>();
        client.enabled = true;
        client.Connection = amqpConnection.Name;
        client.ConnectToHost();

        _watson_listener = GetComponent<WatsonListener>();

    }

    public void Update()
    {
    }

    //This assumes we get input as a JSON file. 
    public void OnMessageReceived(AmqpExchangeSubscription subscription, IAmqpReceivedMessage message)
    {
        /*
        var origMsgText = Encoding.UTF8.GetString(message.Body);
        var origMsg = JsToCSharpMessage.ConvertToObject(origMsgText);

        var origMsgEscaped = origMsgText.Replace("\\", "\\\\").Replace("\"", "\\\"");
        OnMessageReceived("{\"type\":\"" + origMsg.type + "\",\"body\":\"" + origMsgEscaped + "\"}");
        */

        OnMessageReceived(Encoding.UTF8.GetString(message.Body));
    }

    public void OnMessageReceived(string output)
    {
        //AmqpMessageHandler.PrintJson(json);
        //JsToCSharpMessage msg = JsToCSharpMessage.ConvertToObject(json);

        //JSON_INTENT json_intent = JsonUtility.FromJson<JSON_INTENT>(msg.body);

        Debug.Log("Received output " + output);
        _watson_listener.SendToAgent(output.Substring(2,output.Length-4));
    }

    public void Send(string send)
    {
        print("Sent " + send + " to " + send_to_backend_exchange);
        AmqpClient.Publish("send_to_backend", "", send);
    }

}

