using UnityEngine;
using System.Collections;

public class DownloadObbExample : MonoBehaviour
{
	public string PUBLIC_KEY = "";
	public string nextScene = "Title";

	private string logtxt;
	private bool alreadyLogged = false;
	private bool downloadStarted = false;
	private string expPath;

	
	void log( string t )
	{
		logtxt += t + "\n";
		print("MYLOG " + t);
	}
//	void OnGUI()
//	{
//		GUI.skin = mySkin;
//		GUI.DrawTexture(new Rect(0,0,background.width,background.height),background);
//		
//		if (!GooglePlayDownloader.RunningOnAndroid())
//		{
//			GUI.Label(new Rect(10, 10, Screen.width-10, 20), "Use GooglePlayDownloader only on Android device!");
//			return;
//		}
//		
//		expPath = GooglePlayDownloader.GetExpansionFilePath();
//		if (expPath == null)
//		{
//			GUI.Label(new Rect(10, 10, Screen.width-10, 20), "External storage is not available!");
//		}
//		else
//		{
//			string mainPath = GooglePlayDownloader.GetMainOBBPath(expPath);
//			string patchPath = GooglePlayDownloader.GetPatchOBBPath(expPath);
//			if( alreadyLogged == false )
//			{
//				alreadyLogged = true;
//				log( "expPath = "  + expPath );
//				log( "Main = "  + mainPath );
//				log( "Main = " + mainPath.Substring(expPath.Length));
//				
//				if (mainPath != null)
//					StartCoroutine(loadLevel());
//				
//			}
//			//GUI.Label(new Rect(10, 10, Screen.width-10, Screen.height-10), logtxt );
//			
//			if (mainPath == null)
//			{
//				GUI.Label(new Rect(Screen.width-600, Screen.height-230, 430, 60), "The game needs to download 200MB of game content. It's recommanded to use WIFI connexion.");
//				if (GUI.Button(new Rect(Screen.width-500, Screen.height-170, 250, 60), "Start Download !"))
//				{
//					GooglePlayDownloader.FetchOBB();
//					StartCoroutine(loadLevel());
//				}
//			}
//			
//		}
//		
//	}

	void Start ()
	{
//		nextScene = "";

#if UNITY_ANDROID && !UNITY_EDITOR
		if (Application.platform == RuntimePlatform.Android)
		{
//			log ("start => ");
//			expPath = GooglePlayDownloader.GetExpansionFilePath();
//			log ("expPath => " + expPath);
//			string mainPath = GooglePlayDownloader.GetMainOBBPath(expPath);
//			log ("mainPath => " + mainPath);
//			string patchPath = GooglePlayDownloader.GetPatchOBBPath(expPath);
//			log ("patchPath => " + patchPath);
//			if (mainPath != null)
//				StartCoroutine(loadLevel());
			GooglePlayDownloader.SetPublicKey(PUBLIC_KEY);
			expPath = GooglePlayDownloader.GetExpansionFilePath();
		}
#endif
		StartCoroutine(loadLevel());
	}

	protected IEnumerator loadLevel()
	{
		yield return null;
#if UNITY_ANDROID && !UNITY_EDITOR
//		yield return new WaitForSeconds(0.5f);
		if (Application.platform == RuntimePlatform.Android)
		{
//			string mainPath;
//			do
//			{
//				yield return new WaitForSeconds(0.5f);
//				mainPath = GooglePlayDownloader.GetMainOBBPath(expPath);
//				log("waiting mainPath => " + mainPath);
//			}
//			while( mainPath == null);

			string mainPath = null;
			yield return new WaitForSeconds(0.5f);
			mainPath = GooglePlayDownloader.GetMainOBBPath(expPath);

			if (mainPath != null)
			{
				if( downloadStarted == false )
				{
					downloadStarted = true;
					
					string uri = "file://" + mainPath;
					log("downloading " + uri);
					WWW www = WWW.LoadFromCacheOrDownload(uri , 0);		
					
					// Wait for download to complete
					yield return www;
					
					if (www.error != null)
					{
						log ("wwww error " + www.error);
					}
					else
					{
						Application.LoadLevel(nextScene);
					}
				}
			}
			else
			{
				Application.LoadLevel(nextScene);
			}
		}
#else
		Application.LoadLevel(nextScene);
#endif
	}
}

// original code
//using UnityEngine;
//using System.Collections;
//
//public class DownloadObbExample : MonoBehaviour {
//	
//	void OnGUI()
//	{
//		if (!GooglePlayDownloader.RunningOnAndroid())
//		{
//			GUI.Label(new Rect(10, 10, Screen.width-10, 20), "Use GooglePlayDownloader only on Android device!");
//			return;
//		}
//		
//		string expPath = GooglePlayDownloader.GetExpansionFilePath();
//		if (expPath == null)
//		{
//				GUI.Label(new Rect(10, 10, Screen.width-10, 20), "External storage is not available!");
//		}
//		else
//		{
//			string mainPath = GooglePlayDownloader.GetMainOBBPath(expPath);
//			string patchPath = GooglePlayDownloader.GetPatchOBBPath(expPath);
//			
//			GUI.Label(new Rect(10, 10, Screen.width-10, 20), "Main = ..."  + ( mainPath == null ? " NOT AVAILABLE" :  mainPath.Substring(expPath.Length)));
//			GUI.Label(new Rect(10, 25, Screen.width-10, 20), "Patch = ..." + (patchPath == null ? " NOT AVAILABLE" : patchPath.Substring(expPath.Length)));
//			if (mainPath == null || patchPath == null)
//				if (GUI.Button(new Rect(10, 100, 100, 100), "Fetch OBBs"))
//					GooglePlayDownloader.FetchOBB();
//		}
//
//	}
//}
