using UnityEngine;
using System.Collections;

public class SplashSceneScript : MonoBehaviour {

	public string nextScene = "Title";

    void Start()
    {
    }

	public void LoadNextScene ()
	{
		Application.LoadLevel (nextScene);
	}
}
