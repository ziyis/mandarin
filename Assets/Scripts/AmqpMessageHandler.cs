using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is custom!
//We prob. want this to be a singleton so it can hold variables.
public sealed class AmqpMessageHandler : MonoBehaviour{

    private static readonly AmqpMessageHandler instance = new AmqpMessageHandler();

    static AmqpMessageHandler()
    {
    }
    private AmqpMessageHandler()
    {
    }

    public static AmqpMessageHandler Instance()
    {        
        return instance;
    }

	public static void PrintJson(string json)
    {
        JSON_TEST jsn = JsonUtility.FromJson<JSON_TEST>(json);
        print ("int_test : " + jsn.int_test + "\nstring_test : " + jsn.string_test +
            "\nstring_test_two : " + jsn.string_test_two +
            "\nstring_test_three : " + jsn.string_test_three);
    }

}
