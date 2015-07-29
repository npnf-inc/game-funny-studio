using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;
using System.Text;
using System.Linq;

namespace UnityEditor.NPNFEditor
{
	public class ManifestMod
	{
		public static void GenerateManifest()
		{
			var outputFile = Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");
			
			// only copy over a fresh copy of the AndroidManifest if one does not exist
			if (!File.Exists(outputFile))
			{
				var inputFile = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines/androidplayer/AndroidManifest.xml");
				File.Copy(inputFile, outputFile);
			}
			UpdateManifest(outputFile);
		}
		
		private static XmlNode FindChildNode(XmlNode parent, string name)
		{
			XmlNode curr = parent.FirstChild;
			while (curr != null)
			{
				if (curr.Name.Equals(name))
				{
					return curr;
				}
				curr = curr.NextSibling;
			}
			return null;
		}
		
		private static XmlElement FindMainActivityNode(XmlNode parent)
		{
			XmlNode curr = parent.FirstChild;
			while (curr != null)
			{
				if (curr.Name.Equals("activity") && curr.FirstChild != null && curr.FirstChild.Name.Equals("intent-filter"))
				{
					return curr as XmlElement;
				}
				curr = curr.NextSibling;
			}
			return null;
		}
		
		private static XmlElement FindElementWithAndroidName(string name, string androidName, string ns, string value, XmlNode parent)
		{
			var curr = parent.FirstChild;
			while (curr != null)
			{
				if (curr.Name.Equals(name) && curr is XmlElement && ((XmlElement)curr).GetAttribute(androidName, ns) == value)
				{
					return curr as XmlElement;
				}
				curr = curr.NextSibling;
			}
			return null;
		}
		
		public static void UpdateManifest(string fullPath)
		{
            if (NPNFSettings.Instance.AppId != null)
			{
			    UpdateAppKeyInManifest(fullPath);
            }
		}
		
		public static void UpdateAppKeyToManifest(string AppKey, string id)
		{
			string outputFile = GetFilePath();
			
			UpdateAppKeyInManifest(outputFile);
		}


		private static void UpdateAppKeyInManifest(string path)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(path);
			
			if (doc == null)
			{
				Debug.LogError("Couldn't load " + path);
				return;
			}
			
			XmlNode manNode = FindChildNode(doc, "manifest");
			XmlNode dict = FindChildNode(manNode, "application");
			
			if (dict == null)
			{
				Debug.LogError("Error parsing " + path);
				return;
			}
			
			string ns = dict.GetNamespaceOfPrefix("android");
			
			//add the NPNF app key 
			//<meta-data android:name="com.npnf.UnityBridgeAndroid.npnf_app_key" android:value="\ 83a4373e9c0a493dbecea12aabc5e9dc" />
			XmlElement AppIdElement = FindElementWithAndroidName("meta-data", "name", ns, "com.npnf.npnf_app_key", dict);
			if (AppIdElement == null)
			{
				AppIdElement = doc.CreateElement("meta-data");
				AppIdElement.SetAttribute("name", ns, "com.npnf.npnf_app_key");
				dict.AppendChild(AppIdElement);
			}

			AppIdElement.SetAttribute("value", ns, NPNFSettings.Instance.AppId);

			doc.Save(path);
		}
		
		
		public static string GetFilePath()
		{
			var outputFile = Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");
			
			// only copy over a fresh copy of the AndroidManifest if one does not exist
			if (!File.Exists(outputFile))
			{
				var inputFile = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines/androidplayer/AndroidManifest.xml");
				File.Copy(inputFile, outputFile);
			}

			return outputFile;
		}

	}
}
