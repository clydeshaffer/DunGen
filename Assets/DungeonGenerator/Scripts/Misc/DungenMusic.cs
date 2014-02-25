using UnityEngine;
using System.Collections;

public class DungenMusic : MonoBehaviour {
	
	RoomGenerator rg;
	bool containsTarget = false;
	
	// Use this for initialization
	void Start () {
		rg = GetComponent<RoomGenerator>();
	
	}
	
	// Update is called once per frame
	void Update () {
		bool containsNow = rg.totalBounds.Contains(Camera.mainCamera.transform.position);
		
		if(containsNow && !containsTarget)
		{
			MusicManager.mm.PlayTrack(rg.roomStyle.musicName);
		}
		else if(containsTarget && !containsNow)
		{
			MusicManager.mm.StopTrackIfPlaying(rg.roomStyle.musicName);
		}
		
		containsTarget = containsNow;
	}
}
