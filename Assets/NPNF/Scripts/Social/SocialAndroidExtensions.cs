#if UNITY_ANDROID
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NPNF.Core.AnalyticsModule;
using NPNF.Core;

using UnityEngine;

namespace NPNF
{
    public partial class UnityBridgeAndroidPlugin
    {
        private const string CLASS_SOCIAL = "com.npnf.sdk.npnfsocial.ContactListController";
    
        public void AndroidBridgeExtensionsInitSocial(string gameObjectName)
        {
            AndroidJNI.AttachCurrentThread();            
        }

        public void AndroidBridgeExtensionsReceiveMessageSocial(string method, object[] args)
        {
            if (method.Equals("SynchronizeContacts"))
            {
                this.SynchronizeContacts(Convert.ToBoolean(args[0]));
            }
            else if (method.Equals("GetAddressBookPhoneList"))
            {
                this.GetAddressBookPhoneList();
            }
            else if (method.Equals("GetAddressBookEmailList"))
            {
                this.GetAddressBookEmailList();
            }
            else if (method.Equals("AskContactAccessPermission"))
            {
                this.AskContactAccessPermission();
            }
        }

        public void SynchronizeContacts(bool enableAddressBookChangeCallback)
        {
            object[] args = new[] { Convert.ToString(enableAddressBookChangeCallback) };
            
            CallSocialAndroidMethod ("synchronizeContacts", args);     
        }
        
        public void AskContactAccessPermission()
        {
            LoadFriendsFromAddressBook();
        }
        
        public void LoadFriendsFromAddressBook()
        {
            CallSocialAndroidMethod ("loadFriendsFromAddressBook");        
        }
        
        public void GetAddressBookPhoneList()
        {
            CallSocialAndroidMethod ("getAddressBookPhoneList");   
        }
        
        public void GetAddressBookEmailList()
        {
            CallSocialAndroidMethod ("getAddressBookEmailList");   
        }
        
        public void OnLoadFriendsFromAddressBookComplete(string contacts)
        {
            object[] args = new object[1];
            args[0] = contacts;
            NPNFBridge.Instance.ReceiveMessage("OnLoadFriendsFromAddressBookComplete", args);
        }
        
        public void OnSynchronizeContactsComplete(string contacts)
        {
            object[] args = new object[1];
            args[0] = contacts;
            NPNFBridge.Instance.ReceiveMessage("OnSynchronizeContactsComplete", args);
        }
    

        private void CallSocialAndroidMethod(string methodName, params object[] args)
        {
            if (!pause)
            {
                using (AndroidJavaClass clsUnityPlayer = new AndroidJavaClass(CLASS_UNITY_PLAYER))
                using (AndroidJavaObject objActivity = clsUnityPlayer.GetStatic<AndroidJavaObject>(CURRENT_ACTIVITY))
                using (AndroidJavaClass clsSocial = new AndroidJavaClass(CLASS_SOCIAL))
                using (AndroidJavaObject objSocial = clsSocial.CallStatic<AndroidJavaObject>(METHOD_GETINSTANCE, objActivity, gameObjectName))
                {
                    objSocial.Call(methodName, args);
                }
            } else
            {
                Debug.LogError("SocialAndroidExtensions.CallSocialAndroidMethod - Cannot make JNI call while in pause state");
            }
        }
    
    }
}
#endif