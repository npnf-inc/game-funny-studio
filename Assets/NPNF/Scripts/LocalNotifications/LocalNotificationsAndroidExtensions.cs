#if UNITY_ANDROID
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NPNF.Core.AnalyticsModule;

using UnityEngine;

namespace NPNF
{
    public partial class UnityBridgeAndroidPlugin
    {
        private const string CLASS_LOCAL_NOTIFICATIONS = "com.npnf.sdk.npnflocalnotifications.NPNFNotificationCenter";
        
        public void AndroidBridgeExtensionsInitLocalNotifications (string gameObjectName)
        {
            AndroidJNI.AttachCurrentThread ();            
        }
        
        public void AndroidBridgeExtensionsReceiveMessageLocalNotifications(string method, object[] args)
        {
            if (method.Equals("NPNFScheduleNotifications"))
            {
                this.NPNFScheduleNotifications(Convert.ToString(args[0]), Convert.ToString(args[1]), Convert.ToInt32(args[2]));
            }
        }
        
        public void NPNFScheduleNotifications (string notificationId, string message, int time)
        {
            object[] args = new[] { notificationId, message, time.ToString() };
            
            CallLNAndroidMethod ("npnfScheduleNotifications", args);  
        }
        
        private void CallLNAndroidMethod(string methodName, params object[] args)
        {
            if (!pause)
            {
                using (AndroidJavaClass clsUnityPlayer = new AndroidJavaClass(CLASS_UNITY_PLAYER))
                using (AndroidJavaObject objActivity = clsUnityPlayer.GetStatic<AndroidJavaObject>(CURRENT_ACTIVITY))
                using (AndroidJavaClass clsLocalNotifications = new AndroidJavaClass(CLASS_LOCAL_NOTIFICATIONS))
                using (AndroidJavaObject objLocalNotifications = clsLocalNotifications.CallStatic<AndroidJavaObject>(METHOD_GETINSTANCE, objActivity, gameObjectName))
                {
                    objLocalNotifications.Call(methodName, args);
                }
            }
            else
            {
                Debug.LogError("LocalNotificationsAndroidExtensions.CallLNAndroidMethod - Cannot make JNI call while in pause state");
            }
        }
        
    }
}
#endif
