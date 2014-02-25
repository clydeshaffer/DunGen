using UnityEngine;
using System.Collections;

public class ReloadLevelOnKey : MonoBehaviour {
	
	public KeyCode resetKey = KeyCode.F2;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(resetKey))
		{
			Application.LoadLevel(Application.loadedLevel);
		}
	}
	
	void OnGUI () {
		GUILayout.BeginVertical();
		GUILayout.Label("Press " + resetKey + " to generate a new level.");
		GUILayout.Label("Use WASD to move, mouse to look.");
		GUILayout.Label("Hold space to fly upward.");
		GUILayout.EndVertical();
	}
}
