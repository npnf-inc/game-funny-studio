/**
 * Platform - This class is for initialization of the platform 
 * 
 */
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Reflection;
using NPNF.Core;
using NPNF.Core.UserModule;

#if !UNITY_WEBPLAYER
using System.Net.NetworkInformation;
#endif

namespace NPNF
{
    internal enum NPNFPlatformInitState
    {
        NotInitialized,
        Initializing,
        HasInitialized
    }
    public partial class Platform
    {
        private static NPNFPlatformInitState initState = NPNFPlatformInitState.NotInitialized;

        public static bool IsInitialized
        {
            get
            {
                return initState == NPNF.NPNFPlatformInitState.HasInitialized;
            }
        }

        private static NPNFSettings Settings { get; set; }

        #if UNITY_EDITOR || UNITY_IPHONE || UNITY_ANDROID
        public static UserTrackingController utController;
        #endif

        private static Queue<Action<NPNFError>> CallbackQueue = new Queue<Action<NPNFError>>();

        /** 
         * Call this method to initialize the NPNF platform
         **/
        public static void Init(Action<NPNFError> callback = null)
        {
            switch(initState)
            {
                case NPNFPlatformInitState.NotInitialized:
                    ProcessInit(callback);
                    break;
                case NPNFPlatformInitState.Initializing:
                    QueueInit(callback);
                    break;
                case NPNFPlatformInitState.HasInitialized:
                    SkipInit(callback);
                    break;
            }
        }

        private static void ProcessInit(Action<NPNFError> callback = null)
        {
            initState = NPNFPlatformInitState.Initializing;

            QueueInit(callback);
            UnityEngine.Debug.Log("NPNF Platform Init");
            Settings = NPNFSettings.Instance;
            NPNFMain.Settings = Settings;
            if (NPNFMain.Settings != null)
            {
                NPNFMain.OnInitComplete += InitCompleteHandler;
                NPNFMain.OnInitDeviceProfile += InitDeviceProfileHandler;
            }
            CustomFeaturesInitialization();

            NPNFMain.Init();
            #if !UNITY_WEBPLAYER
            Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
            #endif
        }

        private static void QueueInit(Action<NPNFError> callback = null)
        {
            if (callback != null)
                CallbackQueue.Enqueue(callback);
        }

        private static void SkipInit(Action<NPNFError> callback = null)
        {
            UnityEngine.Debug.Log("Platform already Initialized");
            if (callback != null)
            {
                callback(null);
            }
        }

        private static void CustomFeaturesInitialization()
        {
            Type type = typeof(Platform);
            MethodInfo[] infos = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
            
            for (int i = 0; i < infos.Length; i++)
            {
                MethodInfo info = infos [i];
                
                if (info.Name.StartsWith("PlatformInit"))
                {
                    info.Invoke(null, null);
                }
            }
        }
        
        private static void InitCompleteHandler(NPNFError error)
        {
            initState = NPNFPlatformInitState.HasInitialized;
            foreach (Action<NPNFError> callback in CallbackQueue)
            {
                callback(error);
            }
            CallbackQueue.Clear();
        }

        private static void InitDeviceProfileHandler(NPNFError error)
        {
#if !UNITY_EDITOR
            if (error != null)
                NPNFSettings.Instance.RefreshDeviceProfile();
#endif
        }
        
    }
}
