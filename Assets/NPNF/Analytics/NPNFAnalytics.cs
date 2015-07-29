using System;
using NPNF.Core.AnalyticsModule;

namespace NPNF
{
	public class NPNFAnalytics : INPNFAnalytics
	{
		public void Init (string serverUrl, string appKey, string clientId, string accessToken)
		{
			NPNFAnalyticsBridge.Init (serverUrl, appKey, clientId, accessToken);
		}

		public void UpdateAccessToken(string value)
		{
			NPNFAnalyticsBridge.UpdateAccessToken(value);
		}

		public void Start ()
		{
			NPNFAnalyticsBridge.Start ();
		}

		public void Stop ()
		{
            NPNFAnalyticsBridge.Stop ();
		}

		public void SetUserId (string userId)
		{
            NPNFAnalyticsBridge.SetUserId (userId);
		}

		public void RecordEvent (string key)
		{
            NPNFAnalyticsBridge.RecordEvent (key);
		}

		public void RecordEvent (string key, int count)
		{
            NPNFAnalyticsBridge.RecordEvent (key, count);
		}

		public void RecordEvent (string key, int count, double sum)
		{
            NPNFAnalyticsBridge.RecordEvent (key, count, sum);
		}

		public void RecordEvent (string key, System.Collections.Generic.Dictionary<string, string> segmentation, int count)
		{
            NPNFAnalyticsBridge.RecordEvent (key, segmentation, count);
		}

		public void RecordEvent (string key, System.Collections.Generic.Dictionary<string, string> segmentation, int count, double sum)
		{
            NPNFAnalyticsBridge.RecordEvent (key, segmentation, count, sum);
		}
	}
}