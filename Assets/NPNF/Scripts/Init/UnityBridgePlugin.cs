using System;
using UnityEngine;
using System.Collections;

namespace NPNF
{
	public abstract class UnityBridgePlugin: MonoBehaviour, NPNF.Core.IUnityBridge
	{
		public UnityBridgePlugin ()
		{
		}

		public abstract String GetUserAgent ();

		public abstract void ReceiveMessage(string method, object[] args);

#if UNITY_IPHONE
		public abstract String GetIDFA ();

		public abstract String GetIDFV ();

		public abstract void OnInAppPurchase(string message);

		public abstract Boolean GetLimitingAdTrackingEnabled ();
#endif

	}
}

