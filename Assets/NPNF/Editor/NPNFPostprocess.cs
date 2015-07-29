using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEditor.NPNFEditor;

namespace UnityEditor.NPNFEditor
{
    public static class XCodePostProcess
    {
		[PostProcessBuild(200)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
//            // If integrating with facebook on any platform, throw a warning if the app id is invalid
//            if (!FBSettings.IsValidAppId)
//            {
//                Debug.LogWarning("You didn't specify a Facebook app ID.  Please add one using the Facebook menu in the main Unity editor.");
//            }

			Boolean isIOS = false;
			#if UNITY_5 
			isIOS = (target == BuildTarget.iOS);
			#else
			isIOS = (target == BuildTarget.iPhone);
			#endif

			if (isIOS)
            {
				UnityEditor.NPNFEditor.XCProject project = new UnityEditor.NPNFEditor.XCProject(path);

                // Find and run through all projmods files to patch the project

				string projModPath = System.IO.Path.Combine(Application.dataPath, "NPNF/Editor/iOS");
                var files = System.IO.Directory.GetFiles(projModPath, "*.projmods", System.IO.SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    project.ApplyMod(Application.dataPath, file);
                }
                project.Save();
				
				Dictionary<string, string> keys = new Dictionary<string, string> ();
				keys.Add ("NPNFAppId", NPNFSettings.Instance.AppId);

				PlistMod.UpdatePlist(path, keys);

				FixupFiles.FixSimulator(path);
            }

            if (target == BuildTarget.Android)
            {
				// If something is wrong with the Android Manifest, try to regenerate it to fix it for the next build.
                // ManifestMod.GenerateManifest();
			}
        }
    }
}
