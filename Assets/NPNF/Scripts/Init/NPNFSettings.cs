using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NPNF;
using NPNF.Core.UserModule;
using NPNF.Core.AnalyticsModule;
using NPNF.Core.Admin;
using NPNF.Core;
using System.Text.RegularExpressions;
using System.Diagnostics;

#if !UNITY_WEBPLAYER
using System.Net.NetworkInformation;
#endif

#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
#endif

public class NPNFSettings : ScriptableObject, INPNFSettings, IAdminSettings
{
    private static NPNFSettings instance;

    public const string SDK_VERSION = "2.0.0";
    private const string OS_MACOSX = "Mac OS X";
    private const string USER_ID = "npnfsettings_manager";
    private const string npnfSettingsAssetName = "NPNFSettings";
    private const string npnfSettingsPath = "NPNF/Resources";
    private const string npnfSettingsAssetExtension = ".asset";

    public static readonly string[] MODULES = new string[] { "Collections", "Fusion", "Gacha", "Currency", "Energy", "InAppPurchase", "Events" };

    private AdminManager mAdmin;
    private CoroutineQueue mQueue;

#if !UNITY_EDITOR
    [NonSerialized]
    private GameObject bridgeObject = null;
#endif

#if !UNITY_EDITOR && UNITY_IPHONE
    [NonSerialized]
    public UnityBridgeIOSPlugin bridge;
#elif !UNITY_EDITOR && UNITY_ANDROID
    [NonSerialized]
    public UnityBridgeAndroidPlugin bridge;
#else
    [NonSerialized]
    public UnityBridgePlugin bridge;
#endif

    public static NPNFSettings Instance
    {
        get
		{
			if (instance == null)
			{
				instance = Resources.Load(npnfSettingsAssetName) as NPNFSettings;
				if (instance == null)
				{
					// If not found, create the asset object
					instance = CreateInstance<NPNFSettings>();
#if UNITY_EDITOR
					string properPath = Path.Combine(Application.dataPath, npnfSettingsPath);
					if (!Directory.Exists(properPath))
					{
						AssetDatabase.CreateFolder("Assets/NPNF", "Resources");
					}

					string fullPath = Path.Combine(Path.Combine("Assets", npnfSettingsPath),
					                               npnfSettingsAssetName + npnfSettingsAssetExtension);

					AssetDatabase.CreateAsset(instance, fullPath);
#endif
				}
				instance.Load();
			}
			return instance;
		}
	}

    private void CreateAdminManager()
    {
        mAdmin = AdminManager.Create(this, this, USER_ID, mQueue);
    }

    public void GetVersions(Action<List<NPNF.Core.Configuration.Version>, NPNFError> callback)
    {
        CreateAdminManager();
        mAdmin.GetAllVersions((List<NPNF.Core.Configuration.Version> versions, NPNFError error) => {
            callback(versions, error);
        });
    }

    // Callback will be called once for each type of validation (app, admin, version)
    public void VerifyAppSettings(Action<AdminManager.SettingsType, bool> callback)
    {
        CreateAdminManager();
        mAdmin.VerifyAppSettings((AdminManager.SettingsType type, bool isValid) =>
        {
            callback(type, isValid);
        });
    }

    public void VerifyAppKeys(Action<NPNFError> callback)
    {
        AdminManager.VerifyKeys(appId, appSecret, isUsingProduction, mQueue, callback);
    }

    public void Update()
    {
        if (mQueue != null)
        {
            mQueue.Update();
        }
    }

    #region Device Profile

    private void InitDeviceProfile()
    {
        #if !UNITY_EDITOR
        InitBridge();
        #if (UNITY_IPHONE || UNITY_ANDROID)
        Platform.utController = new UserTrackingController (bridge);
        NPNFMain.UTController = Platform.utController;
        NPNFBridge.Instance.UnityBridge = bridge;
        #endif
        #endif

        #if UNITY_EDITOR || UNITY_WEBPLAYER || UNITY_STANDALONE
        DeviceProfile = new DeviceProfile();
        DeviceProfile.deviceIdentity["userAgent"] = "UserAgent";

        // set mac address
        string macAddress = GetMacAddress(UnityEngine.Network.player.ipAddress);
        Dictionary<string, object> deviceIds = DeviceProfile.deviceIdentity[DeviceProfile.IDKEY_DEVICEIDS] as Dictionary<string, object>;
        deviceIds.Add("macAddress", macAddress);
        #elif UNITY_IPHONE
        DeviceProfile = new IOSDeviceProfile();
		((IOSDeviceProfile) DeviceProfile).SetiOSDeviceInfo (bridge.GetIDFA (), bridge.GetIDFV (), bridge.GetUserAgent (), bridge.GetLimitingAdTrackingEnabled());
        #elif UNITY_ANDROID
        DeviceProfile = new AndroidDeviceProfile();
        ((AndroidDeviceProfile) DeviceProfile).SetAndroidDeviceInfo (bridge.GetUserAgent ());
        #else
        DeviceProfile = new DeviceProfile();
        #endif
    }

    private string GetMacAddress(string ipAddress)
    {
        #if UNITY_WEBPLAYER
        return "WEBPLAYER";
        #else
        string os = UnityEngine.SystemInfo.operatingSystem;
        string macAddress = "";
        const string macAddressPattern = @"ether (?<mac>([0-9A-F]{2}[:-]){5}([0-9A-F]{2}))";
        string ipAddressPattern = "inet " + ipAddress;
        Regex macAddrRegex = new Regex(macAddressPattern, RegexOptions.IgnoreCase);

        if (os.Contains(OS_MACOSX))
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("ifconfig");
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;

            var proc = new Process();
            proc.StartInfo = startInfo;
            proc.Start();
            string line = proc.StandardOutput.ReadLine();
            while (line != null)
            {
                MatchCollection matches = macAddrRegex.Matches(line);
                if (matches.Count > 0)
                {
                    macAddress = matches [0].Groups ["mac"].Value;
                }
                if (line.Contains(ipAddressPattern))
                    break;
                line = proc.StandardOutput.ReadLine();
            }
            proc.WaitForExit();
        }
        else
        {
            // Ref: http://msdn.microsoft.com/en-us/library/system.net.networkinformation.physicaladdress(v=vs.110).aspx
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                PhysicalAddress address = adapter.GetPhysicalAddress();
                if (address != null)
                {
                    macAddress = address.ToString();
                    break;
                }
            }
        }
        macAddress = macAddress.Replace(":", "");
        return macAddress;
        #endif
    }

    #if !UNITY_EDITOR
    private void InitBridge()
    {
        if (bridge == null)
        {
            bridgeObject = new GameObject();
            bridgeObject.name = "NPNFUnityBridgePlugin";
            #if UNITY_IPHONE
            bridgeObject.AddComponent<UnityBridgeIOSPlugin> ();
            bridge = bridgeObject.GetComponent<UnityBridgeIOSPlugin> ();
            bridge.Init (bridgeObject.name);
            #elif UNITY_ANDROID
            bridgeObject.AddComponent<UnityBridgeAndroidPlugin>();
            bridge = bridgeObject.GetComponent<UnityBridgeAndroidPlugin>();
            bridge.Init(bridgeObject.name);
            #endif
        }
    }

    public void AddBridgeComponent<T>() where T:UnityEngine.Component
    {
        bridgeObject.AddComponent<T>();
    }

    public T GetBridgeComponent<T>() where T:UnityEngine.Component
    {
        return bridgeObject.GetComponent<T>();
    }

    public void RefreshDeviceProfile()
    {
        #if UNITY_IPHONE
        IOSDeviceProfile devProfile = new IOSDeviceProfile ();
		devProfile.SetiOSDeviceInfo (bridge.GetIDFA (), bridge.GetIDFV (), bridge.GetUserAgent (), bridge.GetLimitingAdTrackingEnabled());
        DeviceProfile.deviceIdentity = devProfile.deviceIdentity;
        #endif

        #if UNITY_ANDROID
        AndroidDeviceProfile devProfile = new AndroidDeviceProfile ();
        devProfile.SetAndroidDeviceInfo (bridge.GetUserAgent ());
        DeviceProfile.deviceIdentity = devProfile.deviceIdentity;
        #endif
    }
    #endif
    #endregion

    #if UNITY_EDITOR
    [MenuItem("NPNF/Edit Settings")]
    public static void Edit()
    {
        Selection.activeObject = Instance;
    }
    #endif

    #region App Settings

    public void Load()
    {
        Analytics = new NPNFAnalytics();
        mQueue = new CoroutineQueue();
        InitDeviceProfile();
    }

    public INPNFAnalytics Analytics { get; set; }

    public DeviceProfile DeviceProfile { get; private set; }

	public static bool IsValidVersion()
	{
		return NPNF.Core.Configuration.ConfigurationManager.IsPlatformVersionValid(NPNFSettings.SDK_VERSION);
	}

    [SerializeField]
    private LoadFlags loadFlags = LoadFlags.None;
    public LoadFlags LoadFlags
    {
        get
        {
            return Instance.loadFlags;
        }
        set
        {
            if (Instance.loadFlags != value)
            {
                Instance.loadFlags = value;
                DirtyEditor();
            }
        }
    }

	[SerializeField]
	private string appVersion = "";
	public string AppVersion
	{
		get
		{
			return Instance.appVersion;
		}
		set
		{
			if (Instance.appVersion != value)
			{
				Instance.appVersion = value;
				DirtyEditor();
			}
		}
	}

	[SerializeField]
	private string appId = "";
	public string AppId
	{
		get
		{
			return Instance.appId;
		}
		set
		{
			if (Instance.appId != value)
			{
				Instance.appId = value;
				DirtyEditor();
			}
		}
	}

	[SerializeField]
	public string appSecret;
	public string AppSecret
	{
		get
		{
			return Instance.appSecret;
		}
		set
		{
			if (Instance.appSecret != value)
			{
				Instance.appSecret = value;
				DirtyEditor();
			}
		}
	}

    [SerializeField]
    private string adminId = "";
    public string AdminId
    {
        get
        {
            return Instance.adminId;
        }
        set
        {
            if (Instance.adminId != value)
            {
                Instance.adminId = value;
                DirtyEditor();
            }
        }
    }

    [SerializeField]
    public string adminSecret;
    public string AdminSecret
    {
        get
        {
            return Instance.adminSecret;
        }
        set
        {
            if (Instance.adminSecret != value)
            {
                Instance.adminSecret = value;
                DirtyEditor();
            }
        }
    }

	[SerializeField]
	private string androidGCMSenderID;
	public string AndroidGCMSenderID
	{
		get
		{
			return Instance.androidGCMSenderID;
		}
		set
		{
			if (Instance.androidGCMSenderID != value)
			{
				Instance.androidGCMSenderID = value;
				DirtyEditor();
			}
		}
	}

	[SerializeField]
	private bool isUsingProduction;
	internal bool IsUsingProduction
	{
		get
		{
			return Instance.isUsingProduction;
		}
		set
		{
			if (Instance.isUsingProduction != value)
			{
				Instance.isUsingProduction = value;
				DirtyEditor();
			}
		}
	}

    [SerializeField]
    private string[] clientVersions;
    public string[] ClientVersions
    {
        get
        {
            return Instance.clientVersions;
        }
        set
        {
            if (Instance.clientVersions != value)
            {
                Instance.clientVersions = value;
                DirtyEditor();
            }
        }
    }

    [SerializeField]
    private int selectedVersionIndex;
    public int SelectedVersionIndex
    {
        get
        {
            return Instance.selectedVersionIndex;
        }
        set
        {
            if (Instance.selectedVersionIndex != value)
            {
                Instance.selectedVersionIndex = value;
                DirtyEditor();
            }
        }
    }

	private static void DirtyEditor ()
	{
#if UNITY_EDITOR
		EditorUtility.SetDirty(Instance);
#endif
	}

	#endregion
}
