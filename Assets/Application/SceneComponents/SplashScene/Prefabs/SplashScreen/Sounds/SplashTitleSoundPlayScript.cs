using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class SplashTitleSoundPlayScript : MonoBehaviour {
	
	void PlaySound(string soundFileName)
	{
		AudioClip mAudioClip;
		mAudioClip = (AudioClip)Resources.Load (soundFileName);
//		audio.clip = mAudioClip;

//		audio.Play ();
	}
}
