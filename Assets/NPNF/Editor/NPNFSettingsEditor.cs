using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.NPNFEditor;
using NPNF.Core;
using NPNF.Core.Admin;

[CustomEditor (typeof(NPNFSettings))]
public class NPNFSettingsEditor : Editor
{
    private const int MODULES_ITEMS_PER_ROW = 4;

	private GUIContent appVersionLabel = new GUIContent("App Version:", "Set the version for your app");
	private GUIContent appIdLabel = new GUIContent("App ID:");
	private GUIContent appSecretLabel = new GUIContent("App Secret:");
	private GUIContent sdkVersionLabel = new GUIContent("SDK Version", "This is Unity NPNF SDK version. If you have problems or compliments, please include this so we know which version to look out for.");
    private GUIContent adminIdLabel = new GUIContent("Admin ID:");
    private GUIContent adminSecretLabel = new GUIContent("Admin Secret:");
    private GUIContent getVersionsLabel = new GUIContent("Get Version(s)");
    private GUIContent reloadVersionsLabel = new GUIContent("Refresh");

    private string verifyStatus = "";
	private NPNFSettings instance;

    private bool isAppSettingsValid = true;
    private bool isAdminSettingsValid = true;
    private bool isVersionSettingsValid = true;

    // use boolean to avoid repainting gui issues
    private bool haveVersions = false;

    public override void OnInspectorGUI()
	{
        instance = (NPNFSettings)target;

		AppKeyGUI();
		EditorGUILayout.Space();
        AdminKeyGUI();
		EditorGUILayout.Space();
        VersionsGUI();
        EditorGUILayout.Space();
        AutosyncGUI();
        EditorGUILayout.Space();
        VerifySettingsGUI();

        // SSDK-1154 - force OnInspectorGUI to be called on every frame
        EditorUtility.SetDirty(target);
        instance.Update();

        if (Event.current.type == EventType.Repaint)
            haveVersions = instance.ClientVersions != null && instance.ClientVersions.Length > 0;
    }

    private void AppKeyGUI()
	{
		EditorGUILayout.HelpBox("Add the App keys associated with this game", MessageType.None);
        string newAppId = EditableField(appIdLabel, instance.AppId, 180, isAppSettingsValid);
        string newAppSecret = EditableField(appSecretLabel, instance.AppSecret, 180, isAppSettingsValid);
        if (newAppId != instance.AppId || newAppSecret != instance.AppSecret)
        {
            ClearVersions();
            instance.AppId = newAppId;
            instance.AppSecret = newAppSecret;
            ManifestMod.GenerateManifest();
        }
	}

    private void AdminKeyGUI()
    {
        EditorGUILayout.HelpBox("Add the Admin keys associated with this game", MessageType.None);
        string newAdminId = EditableField(adminIdLabel, instance.AdminId, 180, isAdminSettingsValid);
        string newAdminSecret = EditableField(adminSecretLabel, instance.AdminSecret, 180, isAdminSettingsValid);
        if (newAdminId != instance.AdminId || newAdminSecret != instance.AdminSecret)
        {
            ClearVersions();
            instance.AdminId = newAdminId;
            instance.AdminSecret = newAdminSecret;
        }
    }

    private void VersionsGUI()
    {
        // Handle App Version Selection
        GUI.enabled = !String.IsNullOrEmpty(instance.AppId) && !String.IsNullOrEmpty(instance.AppSecret) && !String.IsNullOrEmpty(instance.AdminId) && !String.IsNullOrEmpty(instance.AdminSecret);
        EditorGUILayout.HelpBox("Select an App Version", MessageType.None);
        if (!isVersionSettingsValid)
            GUI.color = Color.red;
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(appVersionLabel, GUILayout.Width(180), GUILayout.Height (16));
        GUIContent buttonLabel;
        if (haveVersions)
        {
            instance.SelectedVersionIndex = EditorGUILayout.Popup(instance.SelectedVersionIndex, instance.ClientVersions);
            if (instance.ClientVersions.Length > instance.SelectedVersionIndex)
            {
                instance.AppVersion = instance.ClientVersions[instance.SelectedVersionIndex];
            }
            buttonLabel = reloadVersionsLabel;
        } else
        {
            buttonLabel = getVersionsLabel;
        }
        if (GUILayout.Button(buttonLabel))
        {
            RefreshVersions();
        }
        GUILayout.EndHorizontal();
        GUI.color = Color.white;
        GUI.enabled = true;

        // Display SDK Version
        SelectableLabelField (sdkVersionLabel, NPNFSettings.SDK_VERSION);
        if (!NPNFSettings.IsValidVersion())
        {
            EditorGUILayout.HelpBox("Mismatch SDK Version", MessageType.Error);
        }
    }

    private void AutosyncGUI()
    {
        // Handle App Version Selection
        GUI.enabled = !String.IsNullOrEmpty(instance.AppId) && !String.IsNullOrEmpty(instance.AppSecret) && !String.IsNullOrEmpty(instance.AdminId) && !String.IsNullOrEmpty(instance.AdminSecret);
        EditorGUILayout.HelpBox("Select any modules to automatically sync on launch", MessageType.None);
        GUILayout.BeginVertical();

        for (int i = 0; i < NPNFSettings.MODULES.Length; i += MODULES_ITEMS_PER_ROW)
        {
            GUILayout.BeginHorizontal();
            for (int j = i; j < (i + MODULES_ITEMS_PER_ROW) && j < NPNFSettings.MODULES.Length; ++j)
            {
                int flag = (1 << j);
                bool selected = (((int)instance.LoadFlags & flag) == flag);
                selected = GUILayout.Toggle(selected, NPNFSettings.MODULES[j], new GUILayoutOption[] { GUILayout.Width(115) });
                if (selected)
                {
                    instance.LoadFlags = (LoadFlags)((int)instance.LoadFlags | flag);
                } else
                {
                    instance.LoadFlags = (LoadFlags)((int)instance.LoadFlags & ~flag);
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        GUI.color = Color.white;
        GUI.enabled = true;
    }

    private void ClearVersions()
    {
        instance.ClientVersions = new string[] {};
        instance.AppVersion = null;
    }

    private void RefreshVersions()
    {
        instance.GetVersions((List<NPNF.Core.Configuration.Version> versions, NPNFError error) => {
            if (error != null)
            {
                if (error.Messages != null && error.Messages.Count > 0)
                {
                    EditorGUILayout.HelpBox(error.Messages[0], MessageType.Error);
                }
                else
                {
                    EditorGUILayout.HelpBox("Error occurred while reloading data", MessageType.Error);
                }

                EditorUtility.DisplayDialog("NPNFSettings", "Unable to retrieve versions.\nPlease check your keys and network connection.", "OK");
            }
            else
            {
                if (versions.Count > 0)
                {
                    instance.ClientVersions = new string[versions.Count];
                    for (int i = 0; i < versions.Count; ++i)
                    {
                        instance.ClientVersions[i] = versions[i].ClientVersion;
                    }
                }
                
                // find the index for the current versions after refresh
                if (instance.AppVersion != null)
                {
                    for (int i = 0; i < versions.Count; i++)
                    {
                        if (versions[i].ClientVersion.Equals(instance.AppVersion))
                        {
                            instance.SelectedVersionIndex = i;
                            break;
                        }
                    }
                }
                else if (versions.Count > 0)
                {
                    instance.SelectedVersionIndex = 0;
                    instance.AppVersion = versions[0].ClientVersion;
                }
                ResetVerifyStatus();
            }
        });
    }

    private string EditableField(GUIContent label, string value, int width = 180, bool isValid = true)
	{
		string ret = "";
		EditorGUILayout.BeginHorizontal ();
        if (isValid == false)
            GUI.color = Color.red;
        EditorGUILayout.LabelField (label, GUILayout.Width (width), GUILayout.Height (16));
        ret = EditorGUILayout.TextField(value, GUILayout.Height(16));
        GUI.color = Color.white;
		EditorGUILayout.EndHorizontal ();
		return ret;
    }

    private void VerifySettingsGUI()
    {
        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Verify Configuration"))
        {
            ResetVerifyStatus();
            instance.VerifyAppSettings(CheckSettingStatus);
        }
        EditorGUILayout.LabelField(verifyStatus, GUILayout.Height(48));
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Check settings status
    /// </summary>
    /// <param name="type"></param>
    /// <param name="isValid"></param>
    private void CheckSettingStatus(AdminManager.SettingsType type, bool isValid)
    {
        switch(type)
        {
            case AdminManager.SettingsType.AppSettings:
                isAppSettingsValid = isValid;
                verifyStatus += isValid ? "App settings are valid" : "App settings are not valid";
                break;
            case AdminManager.SettingsType.AdminSettings:
                isAdminSettingsValid = isValid;
                verifyStatus += isValid ? "Admin settings are valid" : "Admin settings are not valid";
                break;
            case AdminManager.SettingsType.VersionSettings: 
                isVersionSettingsValid = isValid;
                verifyStatus += isValid ? "Version set to " + instance.AppVersion : "App version not set";
                break;
        }
        verifyStatus += "\n";
        if (!isValid)
            ClearVersions();
    }

    private void ResetVerifyStatus()
    {
        verifyStatus = "";
        isAppSettingsValid = true;
        isAdminSettingsValid = true;
        isVersionSettingsValid = true;
    }

    private void SelectableLabelField (GUIContent label, string value)
	{
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField (label, GUILayout.Width (180), GUILayout.Height (16));
		EditorGUILayout.SelectableLabel (value, GUILayout.Height (16));
		EditorGUILayout.EndHorizontal ();
	}

	public static void SetAppIdAndSecret(string appId, string appSecret)
	{
		if (appId == null || appSecret == null)
		{
			throw new ArgumentNullException("NPNF app Id and app secret cannot be null");
		}
		else
		{
			NPNFSettings.Instance.AppId = appId;
			NPNFSettings.Instance.AppSecret = appSecret;
			ManifestMod.GenerateManifest();
		}
	}

	public static void SetAppVersion(string value)
	{
		if (value == null)
		{
			Debug.Log("ERROR: App Version is null");
		}
		else
		{
			NPNFSettings.Instance.AppVersion = value;
		}
	}

    public static void ClearAllKeys()
    {
        NPNFSettings.Instance.AdminId = "";
        NPNFSettings.Instance.AdminSecret = "";
        NPNFSettings.Instance.AppId = "";
        NPNFSettings.Instance.AppSecret = "";
        NPNFSettings.Instance.AndroidGCMSenderID = "";
        NPNFSettings.Instance.AppVersion = "";
        NPNFSettings.Instance.LoadFlags = LoadFlags.None;
    }
}
