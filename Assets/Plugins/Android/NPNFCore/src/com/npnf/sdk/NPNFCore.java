package com.npnf.sdk;

import android.app.Activity;

import com.unity3d.player.UnityPlayer;

public class NPNFCore {
	   private static final String TAG = "NPNFCore";
	   private static NPNFCore _instance = null;
	   private static String gameObjectName;

	   private Activity activity;

	   public static NPNFCore getInstance(Activity currentActivity, String objectName) {
			//Log.d("AndroidAPI", "NPNFCore - getInstance");
			if (_instance == null)
			{
				//Log.d("AndroidAPI", "NPNFCore - Init");

				_instance =  new NPNFCore( currentActivity);
				gameObjectName = objectName;
			}
			
			if (!gameObjectName.equals(objectName))
			{
				gameObjectName = objectName;
			}
			
			return _instance;
	   }
	   
	   /***
	    *
	    * @param currentActivity
	    */
	   public NPNFCore(Activity currentActivity)
	   {
	       activity = currentActivity;	
	   }
	   
	   public static String getUserAgent()
	   {
		   //Log.d(TAG,	"getUserAgent");
		   return System.getProperty("http.agent");
	   }
	   
	   public static void sendMessageToUnity(String methodName, String message)
	   { 
		  //Log.d(TAG,	"sendMessageToUnity");
	       UnityPlayer.UnitySendMessage(gameObjectName, methodName, message);   
	   }
}
