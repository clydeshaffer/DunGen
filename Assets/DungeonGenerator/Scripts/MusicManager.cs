//MusicManager.cs
//@author: Clyde H. Shaffer III (Clydeicus)
//
//This script can manage multiple tracks of audio.
//It can fade them in, out, crossfade them, pause them
//and it does this all automatically, controlled by
//its functions.
//Simply assign AudioClips to its tracks array in the inspector,
//and set names for them in the trackNames array, so that they
//can be called upon by messages defined as strings.
//
//All yours for 0 easy payments of $19.99!

using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour {

	public string[] trackNames;
	
	public AudioClip[] tracks;
	
	public AudioSource[] musicChannels;
	public int leadChannel = 0;
	
	public float fadeRate = 0.07f;
	public float musicVolume = 0.2f;
	
	private string currentTrack;
	
	private bool paused = false;
	
	public static MusicManager mm;
	
	void Awake () {
		mm = this;
	}
	
	void Start () {
		musicChannels = new AudioSource[tracks.Length];
		for(int i = 0; i < tracks.Length; i++)
		{
			GameObject newChannel = new GameObject("Channel " + i, typeof(AudioSource));
			musicChannels[i] = newChannel.audio;
		}
	}
	
	void Update () {
		if(paused) return;
		for(int i = 0; i < musicChannels.Length; i ++) {
			if(i == leadChannel) {
				if(!musicChannels[i].isPlaying) musicChannels[i].Play();
				musicChannels[i].volume = Mathf.MoveTowards(musicChannels[i].volume, musicVolume, fadeRate * Time.deltaTime);
			}
			else {
				musicChannels[i].volume = Mathf.MoveTowards(musicChannels[i].volume, 0.0f, fadeRate * Time.deltaTime);
				if(musicChannels[i].isPlaying && musicChannels[i].volume == 0) musicChannels[i].Pause();
			}
		}
	}
	
	
	
	public void PlayTrack(string name) {
		if(musicChannels.Length == 0) return;
		if(name == "") return;
		if(currentTrack == name) return;
		leadChannel = (leadChannel + 1) % musicChannels.Length;
		musicChannels[leadChannel].clip = trackByName(name);
		musicChannels[leadChannel].loop = true;
		musicChannels[leadChannel].Play();
		currentTrack = name;
	}
	
	public void PlayTrackNow(string name) {
		if(currentTrack == name) return;
		leadChannel = (leadChannel + 1) % musicChannels.Length;
		musicChannels[leadChannel].clip = trackByName(name);
		musicChannels[leadChannel].loop = true;
		musicChannels[leadChannel].volume = musicVolume;
		musicChannels[leadChannel].Play();
		currentTrack = name;
	}
	
	public void StopTrackIfPlaying(string name)
	{
		if(currentTrack != name) return;
		StopAll();
	}
	
	public void StopAll() {
		leadChannel = -1;
	}
	
	public void CutAll() {
		for(int i = 0; i < musicChannels.Length; i ++) {
			musicChannels[i].volume = 0;
			musicChannels[i].Stop();
		}
	}
	
	public void Pause() {
		for(int i = 0; i < musicChannels.Length; i ++) {
			musicChannels[i].Pause();
		}
		paused = true;
	}
	
	public void UnPause() {
		for(int i = 0; i < musicChannels.Length; i ++) {
			if(musicChannels[i].volume != 0) musicChannels[i].Play();
		}
		paused = false;
	}
	
	private AudioClip trackByName(string name) {
		int len = Mathf.Min(trackNames.Length,tracks.Length);
		for(int i = 0; i < len; i++) {
			if(name == trackNames[i]) return tracks[i];
		}
		
		return null;
	}
}
