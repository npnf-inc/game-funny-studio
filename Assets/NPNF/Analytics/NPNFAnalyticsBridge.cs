using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace NPNF
{
    public class NPNFAnalyticsBridge
    {
    #if !UNITY_EDITOR && UNITY_IPHONE
        /* Interface to native implementation */

        [DllImport ("__Internal")]
        private static extern void _Init(string serverUrl, string appKey, string clientId, string accessToken);
        
        [DllImport ("__Internal")]
        private static extern void _Start();
        
        [DllImport ("__Internal")]
        private static extern void _UpdateAccessToken(string value);
        
        [DllImport ("__Internal")]
        private static extern void _Stop();
        
        [DllImport ("__Internal")]
        private static extern void _RecordEvent1(string key, int count);
        
        [DllImport ("__Internal")]
        private static extern void _RecordEvent2(string key, int count, double sum);
        
        [DllImport ("__Internal")]
        private static extern void _RecordEvent3(string key, string segmentation, int count);
        
        [DllImport ("__Internal")]
        private static extern void _RecordEvent4(string key, string segmentation, int count, double sum);
        
        [DllImport ("__Internal")]
        private static extern void _SetSkieUserId(string userId);
        
        
    #elif !UNITY_EDITOR && UNITY_ANDROID
        
        [DllImport ("NPNFAnalytics")]
        private static extern void _Init(string serverUrl, string appKey, string clientId, string accessToken);
        
        [DllImport ("NPNFAnalytics")]
        private static extern void _UpdateAccessToken(string value);
        
        [DllImport ("NPNFAnalytics")]
        private static extern void _Start();
        
        [DllImport ("NPNFAnalytics")]
        private static extern void _Stop();
        
        [DllImport ("NPNFAnalytics")]
        private static extern void _RecordEvent1(string key, int count);
        
        [DllImport ("NPNFAnalytics")]
        private static extern void _RecordEvent2(string key, int count, double sum);
        
        [DllImport ("NPNFAnalytics")]
        private static extern void _RecordEvent3(string key, string segmentation, int count);
        
        [DllImport ("NPNFAnalytics")]
        private static extern void _RecordEvent4(string key, string segmentation, int count, double sum);
        
        [DllImport ("NPNFAnalytics")]
        private static extern void _SetSkieUserId(string userId);

    #else

        private static void _Init(string serverURL, string appKey, string clientId, string accessToken)
        {
            Debug.Log("Analytics: Init: serverURL=" + serverURL + ", appKey=" + appKey + ", clientId=" + clientId);
        }
        
        private static void _UpdateAccessToken(string value)
        {
            Debug.Log("Analytics: UpdateAccessToken");
        }
        
        private static void _Start()
        {
            Debug.Log("Analytics: Start");
        }
        
        private static void _Stop()
        {
            Debug.Log("Analytics: Stop");
        }
        
        private static void _RecordEvent1(string key, int count)
        {
            Debug.Log("Analytics: RecordEvent: key=" + key + ", count=" + count);
        }
        
        private static void _RecordEvent2(string key, int count, double sum)
        {
            Debug.Log("Analytics: RecordEvent: key=" + key + ", count=" + count + ", sum=" + sum);
        }
        private static void _RecordEvent3(string key, string segmentation, int count)
        {
            Debug.Log("Analytics: RecordEvent: key=" + key + ", segmentation=" + segmentation + ", count=" + count);
        }
        private static void _RecordEvent4(string key, string segmentation, int count, double sum)
        {
            Debug.Log("Analytics: RecordEvent: key=" + key + ", segmentation=" + segmentation + ", count=" + count + ", sum=" + sum);
        }
        
        private static void _SetSkieUserId(string userId)
        {
            Debug.Log("Analytics: SetSkieUserId: " + userId);
        }
    #endif

        public static void Init(string serverUrl, string appKey, string clientId, string accessToken)
        {
            _Init(serverUrl, appKey, clientId, accessToken);
        }
        
        public static void UpdateAccessToken(string value)
        {
            _UpdateAccessToken(value);
        }
        
        public static void Start()
        {
            _Start();
        }
        
        public static void Stop()
        {
            _Stop();
        }
        
        public static void SetUserId(string userId)
        {
            _SetSkieUserId(userId);
        }
        
        public static void RecordEvent(string key)
        {
            _RecordEvent1(key, 1);
        }
        
        public static void RecordEvent(string key, int count)
        {
            _RecordEvent1(key, count);
        }
        
        public static void RecordEvent(string key, int count, double sum)
        {
            _RecordEvent2(key, count, sum);
        }
        
        public static void RecordEvent(string key, Dictionary<string, string> segmentation, int count)
        {
            string jsonSeg = NPNFAnalyticsTools.MiniJSON.jsonEncode(segmentation);
            _RecordEvent3(key, jsonSeg, count);
        }
        
        public static void RecordEvent(string key, Dictionary<string, string> segmentation, int count, double sum)
        {
            string jsonSeg = NPNFAnalyticsTools.MiniJSON.jsonEncode(segmentation);
            _RecordEvent4(key, jsonSeg, count, sum);
        }
    }
}