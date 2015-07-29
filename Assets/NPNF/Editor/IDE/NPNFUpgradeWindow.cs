using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace NPNF.Upgrade
{
    public class NPNFUpgradeWindow : EditorWindow
    {
        public enum UpgreadePhase
        {
            Check_Version = 1,
            Download_Latest_SDK = 2,
            Uninstall_Current_Version = 3,
            Import_Latest_Version = 4,
        }

        #region private variable
        private bool initialized;
        private string latestUrlSDK;
        private UpgreadePhase _currentPhase = UpgreadePhase.Check_Version;
        #endregion

        #region Download Variable
        WWW downloadRequest = null;
        float downloadProgress = 0f;
        #endregion

        private UpgreadePhase CurrentPhase
        {
            get { return _currentPhase; }
            set
            {
                if (_currentPhase != value)
                {
                    _currentPhase = value;

                    switch (_currentPhase)
                    {
                        case UpgreadePhase.Download_Latest_SDK:
                            DownloadLatestVersion();
                            break;
                        case UpgreadePhase.Uninstall_Current_Version:
                            UninstallCurrentVersion();
                            break;
                        case UpgreadePhase.Import_Latest_Version:
                            NPNFUpgradeUlti.ImportPackage();
                            break;
                    }
                }
            }
        }

        static NPNFUpgradeWindow()
        {
        }

        void StartCheckVersion()
        {
            initialized = true;
            CheckLatestVersion();
        }

        public void OnGUI()
        {
            RenderDetails();
        }

        public void OnInspectorUpdate()
        {
            UpdateUIDownload();
            Repaint();

            if (!initialized)
                StartCheckVersion();
        }

        private void RenderDetails()
        {
            GUILayout.Space(20f);
            //Line 1
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical();
                {
                    foreach (Enum _e in Enum.GetValues(typeof(UpgreadePhase)))
                    {
                        UpgreadePhase e = (UpgreadePhase)_e;
                        EditorGUILayout.BeginHorizontal();
                        {
                            if ((int)CurrentPhase > (int)e)
                                GUI.color = Color.green;
                            else if (CurrentPhase == e)
                                GUI.color = Color.yellow;

                            EditorGUILayout.LabelField(string.Format("{0}. {1}", (int)e, e.ToString().Replace("_", " ")));

                            GUI.color = Color.white;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20f);

            //Line 2
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(20f);
                Rect r = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(r, downloadProgress, "Download... - " + decimal.Round(new Decimal(downloadProgress * 100f), 1) + " %");
                GUILayout.Space(20f);
                EditorGUILayout.EndVertical();
                GUILayout.Space(20f);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void CheckLatestVersion()
        {
            string latestVersion;
            if(!NPNFUpgradeUlti.GetLatestVersion(out latestVersion, out latestUrlSDK))
            {
                EditorUtility.DisplayDialog("Check for New Version", "Can not check versions, network fault", "Close");
                this.Close();
                return;
            }

            Version current = new Version(NPNFSettings.SDK_VERSION);
            Version latest = new Version(latestVersion);

            if (latest.CompareTo(current) > 0)
            {
                bool pick = EditorUtility.DisplayDialog("Check for New Version", "There is a new version of NPNF SDK available for upgrade", "Upgrade", "Later");
                if (pick)
                    CurrentPhase = UpgreadePhase.Download_Latest_SDK;
                else
                    this.Close();
            }
            else
            {
                EditorUtility.DisplayDialog("Check for New Version", "No newer version found. You are already using the latest version", "OK");
                this.Close();
            }
        }

        #region DOWNLOAD LATEST VERSION
        private void DownloadLatestVersion()
        {
            downloadRequest = new WWW(latestUrlSDK);
        }

        private void UpdateUIDownload()
        {
            if(downloadRequest != null)
            {
                downloadProgress = downloadRequest.progress;
                if(downloadRequest.isDone)
                {
                    if (string.IsNullOrEmpty(downloadRequest.error))
                    {
                        downloadProgress = 1f;
                        NPNFUpgradeUlti.SaveFile(downloadRequest.bytes);
                        CurrentPhase = UpgreadePhase.Uninstall_Current_Version;
                        downloadRequest.Dispose();
                        downloadRequest = null;
                    }
                    else
                    {
                        string error = downloadRequest.error;
                        downloadRequest.Dispose();
                        downloadRequest = null;
                        if (EditorUtility.DisplayDialog("Download NPNF SDK Latest Version", error, "Retry", "Close"))
                            downloadRequest = new WWW(latestUrlSDK);
                        else
                            this.Close();
                    }
                }
            }
        }
        #endregion

        private void UninstallCurrentVersion()
        {
            if (NPNFUpgradeUlti.UninstallCurrentVersion())
                CurrentPhase = UpgreadePhase.Import_Latest_Version;
        }

        [MenuItem("NPNF/Check for updates")]
        public static NPNFUpgradeWindow ShowWindow()
        {
            NPNFUpgradeWindow window = GetWindowWithRect<NPNFUpgradeWindow>(new Rect(0, 0, 400f, 150f), true, "Upgrade NPNF SDK");
            window.ShowPopup();
            return window;
        }
    }
}