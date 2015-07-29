#if UNITY_IPHONE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

namespace NPNF
{
    public partial class UnityBridgeIOSPlugin
    {
        [System.Runtime.InteropServices.DllImport ("__Internal")]
        extern static public void iOSBridgeInitSocial(string gameObjectName);
    
        public void iOSBridgeExtensionsInitSocial(string gameObjectName)
        {
            iOSBridgeInitSocial(gameObjectName);
        }

        public void iOSBridgeExtensionsReceiveMessageSocial(string method, object[] args)
        {
            if (method.Equals("GetContactAccessPermissionStatus"))
            {
                this.GetContactAccessPermissionStatus();
            }
            else if (method.Equals("AskContactAccessPermission"))
            {
                this.AskContactAccessPermission();
            }
            else if (method.Equals("SynchronizeContacts"))
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
            else if (method.Equals("LoadFriendsFromAddressBook"))
            {
                this.LoadFriendsFromAddressBook();
            }
        }

        [DllImport("__Internal")]
        private static extern bool getContactAccessPermissionStatus();
        
        [DllImport("__Internal")]
        private static extern void askContactAccessPermission();
        
        [DllImport("__Internal")]
        private static extern string getAllPhoneNumbers();
        
        [DllImport("__Internal")]
        private static extern string getAllEmailAddresses();
        
        [DllImport("__Internal")]
        private static extern void synchronizeContacts (bool addressBookChangeCallback);
        
        [DllImport("__Internal")]
        private static extern void loadFriendsFromAddressBook();
        
        public void GetContactAccessPermissionStatus()
        {
            bool permission = getContactAccessPermissionStatus();
            Debug.Log(permission);
            
            object[] args = new object[1];
            args[0] = permission;
            
            NPNF.Core.NPNFBridge.Instance.ReceiveMessage("OnGetPermissionStatus", args);
        }
        
        public void AskContactAccessPermission()
        {
            Debug.Log("Asking permission...");
            askContactAccessPermission();
        }
        
        public void SynchronizeContacts(bool enableAddressBookChangeCallback)
        {
            synchronizeContacts(enableAddressBookChangeCallback);
        }
        
        public void LoadFriendsFromAddressBook()
        {
            loadFriendsFromAddressBook ();
        }
        
        public void GetAddressBookPhoneList()
        {
            string phoneNumbers = getAllPhoneNumbers();
            object[] args = new object[1];
            args[0] = phoneNumbers;
            NPNF.Core.NPNFBridge.Instance.ReceiveMessage("OnAskPermissionResponsed", args);
        }
        
        public void GetAddressBookEmailList()
        {
            string emails = getAllEmailAddresses();
            object[] args = new object[1];
            args[0] = emails;
            NPNF.Core.NPNFBridge.Instance.ReceiveMessage("OnAskPermissionResponsed", args);
        }
        
        public void OnAskPermissionResponsed(string response)
        {
            object[] args = new object[1];
            args[0] = response;
            NPNF.Core.NPNFBridge.Instance.ReceiveMessage("OnAskPermissionResponsed", args);
        }
        
        public void OnSynchronizeContactsComplete(string contacts)
        {
            object[] args = new object[1];
            args[0] = contacts;
            NPNF.Core.NPNFBridge.Instance.ReceiveMessage("OnSynchronizeContactsComplete", args);
        }
        
        public void OnLoadFriendsFromAddressBookComplete(string response)
        {
            object[] args = new object[1];
            args[0] = response;
            NPNF.Core.NPNFBridge.Instance.ReceiveMessage("OnLoadFriendsFromAddressBookComplete", args);
        }
    }
}
#endif