using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace NPNF.Upgrade
{
    public class NPNFUpgradeUlti
    {
        public const string URL_CHECK_VERSION = "https://developer.npnf.com/releases";
        public const string UNITYPACKAGE_NAME = "NPNFLatestVersion.unitypackage";
        public const string URL_RELEASE_PACKAGE = "https:{0}/npnf-sdk-{1}.unitypackage";

        public static string PathPackage
        {
            get
            {
                return Path.Combine(Application.persistentDataPath, UNITYPACKAGE_NAME);
            }
        }

        #region CHECK LATEST VERSION
        public static bool GetLatestVersion(out string latestVersion, out string latestUrl)
        {
            latestVersion = string.Empty;
            latestUrl = string.Empty;
            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
                HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(URL_CHECK_VERSION);
                webrequest.Method = "GET";
                HttpWebResponse webResponse = (HttpWebResponse)webrequest.GetResponse();
                string responseHtml;
                using (StreamReader responseStream = new StreamReader(webResponse.GetResponseStream()))
                {
                    responseHtml = responseStream.ReadToEnd().Trim();
                    NPNFUpgreadeModel model = NPNF.Lib.LitJson.JsonMapper.ToObject<NPNFUpgreadeModel>(responseHtml);
                    NPNFUpgreadeModel.NPNFUpgradeVersionModel latest = model.GetLatestVersion();

                    latestVersion = latest.version;
                    latestUrl = string.Format(URL_RELEASE_PACKAGE, latest.path, latest.version);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region DOWNLOAD LATEST VERSION
        public static void SaveFile(byte[] Bytes)
        {
            System.IO.File.WriteAllBytes(PathPackage, Bytes);
        }
        #endregion

        #region UNINSTALL CURRENT VERSION
        public static bool UninstallCurrentVersion()
        {
            return NPNFUpgradeWithCommand.RunCommandLine();
        }
        #endregion

        #region IMPORT LATEST VERSION
        public static void ImportPackage()
        {
            string localFilePath = PathPackage;
            int assetIndex = localFilePath.IndexOf("Assets\\");

            if (assetIndex < 0)
                assetIndex = 0;
            localFilePath = localFilePath.Substring(assetIndex, localFilePath.Length - assetIndex);
            localFilePath = localFilePath.Replace('\\', '/');

            AssetDatabase.ImportPackage(localFilePath, true);
            AssetDatabase.Refresh();
        }
        #endregion
    }
}
