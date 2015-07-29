#if UNITY_IPHONE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NPNF.Lib.JsonUtil;
using NPNF.Lib.MiniJSON;

using UnityEngine;

namespace NPNF
{
    public partial class UnityBridgeIOSPlugin
    {
        [System.Runtime.InteropServices.DllImport ("__Internal")]
        extern static public void iOSBridgeInitLocalNotifications (string gameObjectName);
        
        public void iOSBridgeExtensionsInitLN (string gameObjectName)
        {
            iOSBridgeInitLocalNotifications(gameObjectName);
        }
        
        [System.Runtime.InteropServices.DllImport ("__Internal")]
        extern static public void npnfScheduleNotifications (string notificationId, string message, int time);

        [System.Runtime.InteropServices.DllImport ("__Internal")]
        extern static public void registerForNotifications ();

        public void iOSBridgeExtensionsReceiveMessageLocalNotifications(string method, object[] args)
        {
            if (method.Equals("NPNFScheduleNotifications"))
            {
                npnfScheduleNotifications(Convert.ToString(args[0]), Convert.ToString(args[1]), Convert.ToInt32(args[2]));
            } else if (method.Equals("NPNFRegisterNotifications"))
            {
                registerForNotifications();
            }
        }
    }
}
#endif
