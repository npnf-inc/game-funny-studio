using UnityEngine;
using System.Collections;

public class ObjectSFXManager : MonoBehaviour {

	AudioSource aSource;
	void Awake ()
	{
		SetMute();
		SoundManager.Instance.OnSFXSettingChanged += SetMute;
	}
	
	void OnDestroy()
	{
		SoundManager.Instance.OnSFXSettingChanged -= SetMute;
	}

	void SetMute()
	{
		aSource = GetComponent<AudioSource>();
		if (aSource != null)
		{
//			aSource.mute = !CustomPlayerPrefs.SFXEnabled;
		}
	}
}
