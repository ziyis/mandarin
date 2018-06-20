using UnityEngine;

[System.Serializable]
public class StompConfig {
	public string game_object;
	public string ip;
	public string port;
	public string username;
	public string password;
	public string virtual_host;

	public static StompConfig ConvertToObject(string json) {
		return JsonUtility.FromJson<StompConfig>(json);
	}

	public static string ConvertToJSON(StompConfig obj) {
		return JsonUtility.ToJson(obj);
	}
}