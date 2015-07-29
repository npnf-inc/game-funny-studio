using UnityEngine;
using UnityEditor;
using System.Collections;

public static class SplashEditor 
{
	[MenuItem ("PF Editor/Dev Tools/Start From Splash")]
	static void StartFromSplashScene ()
	{
		if (EditorApplication.isPlaying == true)
		{
			EditorApplication.isPlaying = false;
			return;
		}

		EditorApplication.SaveCurrentSceneIfUserWantsTo();
		EditorApplication.OpenScene("Assets/Application/SceneComponents/SplashScene/Splash.unity");
		EditorApplication.isPlaying = true;
	}
}
