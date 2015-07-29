#if UNITY_IPHONE
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NPNF.Core.AnalyticsModule;
using NPNF.Core.UserModule;
using NPNF.Lib.JsonUtil;
using NPNF.Lib.MiniJSON;
using NPNF.Core;
using UnityEngine;

namespace NPNF
{
	public partial class UnityBridgeIOSPlugin: UnityBridgePlugin
	{
		[System.Runtime.InteropServices.DllImport ("__Internal")]
		extern static public void bridgeInit (string gameObjectName);

		[System.Runtime.InteropServices.DllImport ("__Internal")]
		extern static public String bridgeGetIDFA ();

		[System.Runtime.InteropServices.DllImport ("__Internal")]
		extern static public String bridgeGetIDFV ();

		[System.Runtime.InteropServices.DllImport ("__Internal")]
		extern static public String bridgeGetUserAgent ();

		[System.Runtime.InteropServices.DllImport ("__Internal")]
		extern static public Boolean bridgeGetLimitingAdTrackingEnabled ();

		public void Awake()
		{
			DontDestroyOnLoad(this.gameObject);
		}

		public void Init (string gameObjectName)
		{
			bridgeInit (gameObjectName);

			CustomFeaturesInitialization(gameObjectName);
		}

		private void CustomFeaturesInitialization (string gameObjectName)
		{
			Type type = typeof(UnityBridgeIOSPlugin);
			MethodInfo[] infos = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
			
			for (int i = 0; i < infos.Length; i++)
			{
				MethodInfo info = infos [i];
				
				if (info.Name.StartsWith("iOSBridgeExtensionsInit"))
				{
					info.Invoke(this, new object[] { gameObjectName});
				}
			}
		}

		public override void ReceiveMessage(string method, object[] args)
		{
		    CustomFeaturesReceiveMessage (method, args);
        }

        private void CustomFeaturesReceiveMessage (string method, object[] args)
        {
            Type type = typeof(UnityBridgeIOSPlugin);
            MethodInfo[] infos = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            
            for (int i = 0; i < infos.Length; i++)
            {
                MethodInfo info = infos [i];
                
                if (info.Name.StartsWith("iOSBridgeExtensionsReceiveMessage"))
                {
                    info.Invoke(this, new object[] { method, args});
                }
            }
        }

		public override String GetIDFA ()
		{
			return bridgeGetIDFA ();
		}

		public override String GetIDFV ()
		{
			return bridgeGetIDFV ();
		}

		public override String GetUserAgent ()
		{
			return bridgeGetUserAgent ();
		}

		public override Boolean GetLimitingAdTrackingEnabled ()
		{
			return bridgeGetLimitingAdTrackingEnabled ();
		}

		public override void OnInAppPurchase(string message)
		{
			Dictionary<string, object> purchases = JsonUtil.Deserialize(message);
			foreach(string productId in purchases.Keys)
			{
				Dictionary<string, object> purchase = (Dictionary<string, object>) purchases[productId];
				object[] args = new object[5];
				args[0] = productId;
				args[1] = purchase["quantity"];
				args[2] = purchase["price"];
				args[3] = purchase["currencyType"];
				args[4] = purchase["description"];
				NPNFBridge.Instance.ReceiveMessage("OnInAppPurchase", args);
			}
		}

	}
}
#endif
