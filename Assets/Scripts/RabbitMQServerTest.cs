using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.IO;
using System.Text;

//Note: AmqpClient has been edited (703)!

namespace CymaticLabs.Unity3D.Amqp.UI
{ 
    public class RabbitMQServerTest : MonoBehaviour {

        [SerializeField] private string connectionName;
        [SerializeField] private Button but_con, but_sen, but_sus;
        [SerializeField] public static AmqpClient client;

        public Text sndtxt, recvtxt, txtexcname, txtroutkey;

        const string CONFIG_FILE = "Config.json";

        public void Start()
        {
            // Load the config file from StreamingAssets.
            string configText = "";
            bool configError = false;

            try
            {
                configText = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, CONFIG_FILE));
                print("yeet");
            }
            catch
            {
                configError = true;
                print("ohno");
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
                amqpConnection.VirtualHost = "Testing";//config.virtual_host;
                amqpConnection.WebPort = 15674;
                amqpConnection.ReconnectInterval = 5;
                amqpConnection.RequestedHeartBeat = 30;
            }

            AmqpClient.AddConnection(amqpConnection);

            client = GetComponent<AmqpClient>();
            client.enabled = true;
            client.Connection = amqpConnection.Name;
            client.ConnectToHost();

            txtexcname.text = "Testing_Json";
        }

        public void Update()
        {
        }

        public void OnMessageReceived(AmqpExchangeSubscription subscription, IAmqpReceivedMessage message)
        {
            var origMsgText = Encoding.UTF8.GetString(message.Body);
            var origMsg = JsToCSharpMessage.ConvertToObject(origMsgText);

            var origMsgEscaped = origMsgText.Replace("\\", "\\\\").Replace("\"", "\\\"");
            OnMessageReceived("{\"type\":\"" + origMsg.type + "\",\"body\":\"" + origMsgEscaped + "\"}");

        }

        public void OnMessageReceived(string json)
        {
            //AmqpMessageHandler.PrintJson(json);
            JsToCSharpMessage msg = JsToCSharpMessage.ConvertToObject(json);
            
            Debug.Log("Received unrecognized message " + msg.type + " from the server: " + msg.body);
            
        }

        public void Send(string message)
        {
            AmqpClient.Publish(txtexcname.text, "" , sndtxt.text);
        }

        public void Subscribe()
        {
            var subscription = new UnityAmqpExchangeSubscription(
                txtexcname.text, AmqpExchangeTypes.Topic, txtroutkey.text, 
                null, AmqpClient.Instance.UnityEventDebugExhangeMessageHandler);

            // Subscribe on the client
            AmqpClient.Subscribe(subscription);
        }

    }
}
