using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AudioSource))]
public class SplashScreenSoundPlayer : MonoBehaviour
{
	bool play = false;

    public float delaySeconds = 0.0f;


	void Start()
	{
		StartCoroutine(enableTimer(delaySeconds));
    }

    public IEnumerator enableTimer(float delaySeconds)
    {
        play = false;
        yield return new WaitForSeconds(delaySeconds);
        play = true;
    }
	
	void Update()
	{
		if (play)
		{
//			audio.Play();
			play = false;
		}
	}
}