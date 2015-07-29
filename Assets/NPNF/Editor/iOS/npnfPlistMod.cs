using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;

namespace UnityEditor.NPNFEditor
{
    public class PlistMod
    {
        private static XmlNode FindPlistDictNode(XmlDocument doc)
        {
            XmlNode curr = doc.FirstChild;
            while(curr != null)
            {
                if(curr.Name.Equals("plist") && curr.ChildNodes.Count == 1)
                {
                    XmlNode dict = curr.FirstChild;
                    if(dict.Name.Equals("dict"))
                        return dict;
                }
                curr = curr.NextSibling;
            }
            return null;
        }
        
        private static XmlElement AddChildElement(XmlDocument doc, XmlNode parent, string elementName, string innerText=null)
        {
            XmlElement newElement = doc.CreateElement(elementName);
            if(innerText != null && innerText.Length > 0)
                newElement.InnerText = innerText;
            
            parent.AppendChild(newElement);
            return newElement;
        }
        
		public static void UpdatePlist(string path, Dictionary<string, string> keys)
        {
            string fileName = "Info.plist";
            string fullPath = Path.Combine(path, fileName);
            
			if(keys == null || keys.Count == 0)
            {
				Debug.LogError("There are missing keys in the settings. Please check and add back the missing ones");
                return;
            }
            
            XmlDocument doc = new XmlDocument();
            doc.Load(fullPath);
            
            if(doc == null)
            {
                Debug.LogError("Couldn't load " + fullPath);
                return;
            }
            
            XmlNode dict = FindPlistDictNode(doc);
            if(dict == null)
            {
                Debug.LogError("Error parsing " + fullPath);
                return;
            }
            
            //add the app id to the plist
            //the xml should end up looking like this
            /*
            <key>NPNFAppID</key>
            <string>YOUR_APP_ID</string>
             */

			foreach (var pair in keys) {
					AddChildElement(doc, dict, "key", pair.Key);
					AddChildElement(doc, dict, "string", pair.Value);
			}
		

            
            doc.Save(fullPath);
            
            //the xml writer barfs writing out part of the plist header.
            //so we replace the part that it wrote incorrectly here
            System.IO.StreamReader reader = new System.IO.StreamReader(fullPath);
            string textPlist = reader.ReadToEnd();
            reader.Close();
            
            int fixupStart = textPlist.IndexOf("<!DOCTYPE plist PUBLIC");
            if(fixupStart <= 0)
                return;
            int fixupEnd = textPlist.IndexOf('>', fixupStart);
            if(fixupEnd <= 0)
                return;
            
            string fixedPlist = textPlist.Substring(0, fixupStart);
            fixedPlist += "<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">";
            fixedPlist += textPlist.Substring(fixupEnd+1);
            
            System.IO.StreamWriter writer = new System.IO.StreamWriter(fullPath, false);
            writer.Write(fixedPlist);
            writer.Close();
        }
    }
}