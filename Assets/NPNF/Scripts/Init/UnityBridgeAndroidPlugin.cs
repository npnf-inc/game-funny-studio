#if UNITY_ANDROID
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NPNF.Core.AnalyticsModule;
using System.Reflection;

using UnityEngine;

namespace NPNF
{
    public partial class UnityBridgeAndroidPlugin: UnityBridgePlugin
    {
        private const string CURRENT_ACTIVITY = "currentActivity";
        private const string CLASS_UNITY_PLAYER = "com.unity3d.player.UnityPlayer";
        protected const string METHOD_GETINSTANCE = "getInstance";
        private const string CLASS_NPNFCORE = "com.npnf.sdk.NPNFCore";
        Boolean pause = false;
        String gameObjectName;

        public void Init(String gameObjectName)
        {
            this.gameObjectName = gameObjectName;
            AndroidJNI.AttachCurrentThread();            
            
            CustomFeaturesInitialization(gameObjectName);
        }

        private void CustomFeaturesInitialization(string gameObjectName)
        {
            Type type = typeof(UnityBridgeAndroidPlugin);
            MethodInfo[] infos = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            
            for (int i = 0; i < infos.Length; i++)
            {
                MethodInfo info = infos [i];
                
                if (info.Name.StartsWith("AndroidBridgeExtensionsInit"))
                {
                    info.Invoke(this, new object[] { gameObjectName});
                }
            }
        }
        
        public override void ReceiveMessage(string method, object[] args)
        {
            CustomFeaturesReceiveMessage(method, args);
        }
        
        private void CustomFeaturesReceiveMessage(string method, object[] args)
        {
            Type type = typeof(UnityBridgeAndroidPlugin);
            MethodInfo[] infos = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            
            for (int i = 0; i < infos.Length; i++)
            {
                MethodInfo info = infos [i];
                
                if (info.Name.StartsWith("AndroidBridgeExtensionsReceiveMessage"))
                {
                    info.Invoke(this, new object[] { method, args});
                }
            }
        }
        
        public override String GetUserAgent()
        {
            return CallAndroidStaticMethodWithReturnValue<String>(CLASS_NPNFCORE, "getUserAgent");
        }

        private void CallCoreAndroidMethod(string methodName, params object[] args)
        {
            if (!pause)
            {
                using (AndroidJavaClass clsUnityPlayer = new AndroidJavaClass(CLASS_UNITY_PLAYER))
                using (AndroidJavaObject objActivity = clsUnityPlayer.GetStatic<AndroidJavaObject>(CURRENT_ACTIVITY))
                using (AndroidJavaClass clsCore = new AndroidJavaClass(CLASS_NPNFCORE))
                using (AndroidJavaObject objCore = clsCore.CallStatic<AndroidJavaObject>(METHOD_GETINSTANCE, objActivity, gameObjectName))
                {
                    objCore.Call(methodName, args);
                }
            } else
            {
                Debug.LogError("CoreAndroidPlugin.CallCoreAndroidMethod - Cannot make JNI call while in pause state");
            }
        }

        private void CallAndroidStaticMethod(String javaClassName, String methodName, params object[] args)
        {
            using (AndroidJavaObject androidClass = new AndroidJavaClass (javaClassName))
            {
                androidClass.CallStatic(methodName, args);
            }
        }

        private void CallAndroidMethod(string javaClassName, String methodName, params object[] args)
        {
            using (AndroidJavaObject androidClass = new AndroidJavaClass (javaClassName))
            {
                androidClass.Call(methodName, args);
            }
        }

        private T CallAndroidStaticMethodWithReturnValue<T>(String javaClassName, String methodName, params object[] args)
        {
            using (AndroidJavaObject androidClass = new AndroidJavaClass(javaClassName))
            {
                if (args == null)
                {
                    return androidClass.CallStatic<T>(methodName);
                }
                return androidClass.CallStatic<T>(methodName, args);
            }
        }

        public void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            pause = pauseStatus;
        }
    }
}
#endif