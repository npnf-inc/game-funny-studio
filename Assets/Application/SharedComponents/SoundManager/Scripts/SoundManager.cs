using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class SoundManager : MonoBehaviour {
	
	static SoundManager _instance;
	public static SoundManager Instance
	{
		get
		{
			if (_instance == null)
			{
				Debug.LogError(GameObject.Find("SoundManager"));
				GameObject obj = GameObject.Find("SoundManager");
				if (obj == null)
				{
					obj = (GameObject)Instantiate(Resources.Load("SoundManager") as GameObject);
					obj.name = "SoundManager";
				}
				_instance = obj.GetComponent<SoundManager>();
			}
			return _instance;
		}
	}
	
	public enum SFXType
	{
		background_music = 0,
		battle_music = 1,
		hit_snd = 2,
		buff_snd = 3,
		debuff_snd = 4
	}
	
	public List<AudioClip> audioClipLst;
	public List<AudioSource> AllSources;
	
	public float Volume = 1f;
	public float Pitch = 1f;
	public bool Loop = false;
	public float MinDistance = 1f;
	public float MaxDistance = 500f;
	
	public Action OnSFXSettingChanged;
	public Action OnBGMSettingChanged;
	
	AudioSource mainSource;
	AudioSource musicSource;
	AudioSource musicSource1;
	
	public static bool GameOver;
	
	bool SoundEnabled = true;
	bool SFXEnabled = true;
	
	void Awake()
	{
		if (GameObjectRemoveHelper.FindAndRemoveGameObject(gameObject, "SoundManager"))
		{
			return;
		}
		
		if (_instance == null)
			_instance = this;
		
		AllSources = new List<AudioSource>();
		if (mainSource == null)
			mainSource = CreateNewSource(Loop);
		
		//        RegisterSource(mainSource);
		
		if (musicSource == null)
			musicSource = CreateNewSource(true);
		if (musicSource1 == null)
			musicSource1 = CreateNewSource(true);
		
		DontDestroyOnLoad(gameObject);
		
		//		Instance.SFXEnabled = CustomPlayerPrefs.SFXEnabled;
		//		Instance.SoundEnabled = CustomPlayerPrefs.BGMEnabled;
	}
	
	public static void StartSound(SFXType type)
	{
		AudioClip clip = Instance.GetAudioClip(type);
		StartSound(clip);
	}
	
	public static void StartSound(SFXType type, float _pitch)
	{
		AudioClip clip = Instance.GetAudioClip(type);
		StartSound(clip, _pitch);
	}
	
	public static void StartSound(AudioClip clip, float _pitch = 0.0f)
	{
		if(clip != null)
			Instance._StartSound(clip, false, _pitch);
	}
	
	public static void StopSound(AudioSource source)
	{
		source.Stop();
		Instance.UnRegisterSource(source);
	}
	
	public static void PauseSound(AudioSource source, bool isPause)
	{
		if (isPause)
			source.Pause();
		else
			source.Play();
	}
	
	public static void StartLoopingSound(AudioClip clip, Action<AudioSource> callback)
	{
		Instance._StartSound(clip, true, 0.0f, callback);
	}
	
	public static void StartMusic(SFXType type, bool shouldContinue = false)
	{
		AudioClip clip = Instance.GetAudioClip(type);
		if (clip != null)
		{
			float currentTime = 0.0f;
			if (shouldContinue)
			{
				currentTime = Instance.musicSource.time;
			}
			StopMusic();
			StopMusic(Instance.musicSource1);
			
			Instance.PlayMusic(Instance.musicSource, clip, true, currentTime);
		}
	}
	
	public static void ContinueMusic (SFXType type)
	{
		AudioClip clip = Instance.GetAudioClip(type);
		if (clip != null)
		{
			float currentTime = 0.0f;
			if (!Instance.musicSource.isPlaying)
			{
				if (Instance.musicSource1.clip != null)
				{
					currentTime = Instance.musicSource1.time;
					Instance.musicSource1.volume = 1.0f;
					Instance.FadeMusic(Instance.musicSource1, true);
					
					Instance.PlayMusic(Instance.musicSource, clip, true, currentTime);
					Instance.musicSource.volume = 0.0f;
					Instance.FadeMusic(Instance.musicSource, false);
				}
			}
			else
			{
				if (Instance.musicSource.clip != null)
				{
					currentTime = Instance.musicSource.time;
					Instance.musicSource.volume = 1.0f;
					Instance.FadeMusic(Instance.musicSource, true);
					
					Instance.PlayMusic(Instance.musicSource1, clip, true, currentTime);
					Instance.musicSource1.volume = 0.0f;
					Instance.FadeMusic(Instance.musicSource1, false);
				}
			}
		}
	}
	
	
	
	
	public static void StopMusic()
	{
		Instance.musicSource.Stop();
	}
	
	public static void StopMusic(AudioSource source)
	{
		if (source.clip != null && source.isPlaying)
		{
			source.volume = 1.0f;
			source.Stop();
		}
	}
	
	//	public static void DisableSFX()
	//	{
	//		Instance.SFXEnabled = false;
	//	}
	
	public static void EnableSFX(bool enabled)
	{
		//	CustomPlayerPrefs.SFXEnabled = enabled;
		Instance.SFXEnabled = enabled;
		MuteSFX(!enabled);
		if(Instance.OnSFXSettingChanged != null)
		{
			Instance.OnSFXSettingChanged();
		}
	}
	
	//    public static void DisableSound()
	//    {
	//		CustomPlayerPrefs.BGMEnabled = !CustomPlayerPrefs.BGMEnabled;
	//        Instance.SoundEnabled = false;
	//    }
	
	public static void EnableSound(bool enabled)
	{
		//	CustomPlayerPrefs.BGMEnabled = enabled;
		Instance.SoundEnabled = enabled;
		MuteMusic(!enabled);
		if(Instance.OnBGMSettingChanged != null)
		{
			Instance.OnBGMSettingChanged();
		}
	}
	
	public static void StopMusicAndSound()
	{
		Instance.musicSource.Stop();
		Instance.musicSource.mute = true;
		
		Instance.mainSource.Stop();
		Instance.mainSource.mute = true;
		
		for (int i = 0; i < Instance.AllSources.Count; i++)
		{
			Instance.AllSources[i].Stop();
			Instance.AllSources[i].mute = true;
		}
	}
	
	public static void MuteMusic(bool mute)
	{
		if (Instance.musicSource != null)
			Instance.musicSource.mute = mute;
		
		if (Instance.musicSource1 != null)
			Instance.musicSource1.mute = mute;
		
		//		CustomPlayerPrefs.BGMEnabled = !mute;
	}
	
	public static void MuteSFX(bool mute)
	{
		//        if (Instance.mainSource != null)
		//            Instance.mainSource.mute = mute;
		
		for (int i = 0; i < Instance.AllSources.Count; i++)
		{
			Instance.AllSources[i].mute = true;
		}
		//		CustomPlayerPrefs.SFXEnabled = !mute;
	}
	
	public void FadeMusic (AudioSource source, bool isFadeOut)
	{
		StartCoroutine(FadeMusicRoutine(source, isFadeOut));
	}
	public IEnumerator FadeMusicRoutine(AudioSource source, bool isFadeOut)
	{
		if (isFadeOut)
		{
			while (source.volume > 0.1f)
			{
				source.volume = Mathf.Lerp(source.volume, 0.0f, Time.deltaTime);
				yield return 0;
			}
			
			source.volume = 0.0f;
			StopMusic(source);
		}
		else
		{
			while (source.volume < 0.9f)
			{
				source.volume = Mathf.Lerp(source.volume, 1.0f, Time.deltaTime);
				yield return 0;
			}
			
			source.volume = 1.0f;
		}
	}
	
	public void _StartSound(AudioClip clip, bool isLoop, float _pitch = 0.0f, Action<AudioSource> callback = null)
	{
		if (mainSource == null)
			mainSource = CreateNewSource(Loop);
		if (mainSource.isPlaying)
		{
			StartCoroutine(_StartOnOtherSource(clip, isLoop, callback, _pitch));
		}
		else
		{
			mainSource.loop = isLoop;
			
			mainSource.pitch = _pitch;
			if (_pitch == 0.0f)
			{
				mainSource.pitch = Pitch;
			}
			
			PlaySound(mainSource, clip, isLoop);
			
			if(callback != null)
			{
				callback(mainSource);
			}
		}
	}
	
	IEnumerator _StartOnOtherSource(AudioClip clip, bool isLoop, Action<AudioSource> callback, float _pitch = 0.0f)
	{
		AudioSource newSource = gameObject.AddComponent<AudioSource>();
		newSource.volume = Volume;
		newSource.pitch = _pitch;
		if (_pitch == 0.0f)
		{
			newSource.pitch = Pitch;
		}
		newSource.loop = isLoop;
		newSource.minDistance = MinDistance;
		newSource.maxDistance = MaxDistance;
		
		RegisterSource(newSource);
		PlaySound(newSource, clip, isLoop);
		
		while (newSource.isPlaying && !isLoop)
			yield return null;
		
		if (!isLoop)
		{
			GameObject.Destroy(newSource);
			UnRegisterSource(newSource);
		}
		else
		{
			if (callback != null)
				callback(newSource);
		}
	}
	
	AudioClip GetAudioClip(SFXType type)
	{
		return audioClipLst.Count > (int)type ? audioClipLst[(int)type] : null;
	}
	
	void OnDestroy()
	{
		StopMusicAndSound();
	}
	
	private void RegisterSource(AudioSource Source)
	{
		if (Source && !AllSources.Contains(Source))
		{
			AllSources.Add(Source);
		}
	}
	
	private void UnRegisterSource(AudioSource Source)
	{
		if (Source)
		{
			AllSources.Remove(Source);
		}
	}
	
	void PlaySound(AudioSource source, AudioClip clip, bool isLoop)
	{
		if (SFXEnabled)
		{
			source.loop = isLoop;
			source.clip = clip;
			source.Play();
		}
	}
	
	void PlayMusic(AudioSource source, AudioClip clip, bool isLoop, float continueTime = 0.0f)
	{
		//		if (SoundEnabled)
		//        {
		source.loop = isLoop;
		source.clip = clip;
		
		source.time = continueTime;
		source.Play();
		MuteMusic(!SoundEnabled);
		//        }
	}
	
	AudioSource CreateNewSource(bool isLoop)
	{
		AudioSource source = gameObject.AddComponent<AudioSource>();
		source.volume = Volume;
		source.pitch = Pitch;
		source.loop = isLoop;
		source.minDistance = MinDistance;
		source.maxDistance = MaxDistance;
		
		return source;
	}
}
