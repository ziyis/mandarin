using UnityEngine;

[System.Serializable]
public class JsToCSharpMessage {
	public string type;
	public string body;

	public static string ConvertToJson(JsToCSharpMessage msg) {
		return JsonUtility.ToJson(msg);
	}

	public static JsToCSharpMessage ConvertToObject(string json) {
		return JsonUtility.FromJson<JsToCSharpMessage>(json);
	}
}