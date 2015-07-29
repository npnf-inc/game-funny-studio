using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.CountlyXCodeEditor;

namespace UnityEditor.NPNFEditor
{
	public class AnalyticsPostProcess
	{
		[PostProcessBuild(800)]
		public static void OnPostProcessBuild (BuildTarget target, string path)
		{
			Boolean isIOS = false;
			#if UNITY_5 
			isIOS = (target == BuildTarget.iOS);
			#else
			isIOS = (target == BuildTarget.iPhone);
			#endif

			if (isIOS) {
				// Create a new project object from build target
				CountlyXCodeEditor.XCProject project = new CountlyXCodeEditor.XCProject (path);

				// Find and run through all projmods files to patch the project
				string projModPath = System.IO.Path.Combine (Application.dataPath, "NPNF/Analytics/Editor");
				var files = System.IO.Directory.GetFiles (projModPath, "*.projmods", SearchOption.AllDirectories);
				foreach (var file in files) {
					project.ApplyMod (file);
				}

				project.AddOtherBuildSetting ("GCC_ENABLE_OBJC_EXCEPTIONS", "YES");

				// Finally save the xcode project
				project.Save ();

				//FixSimulator(path);
			}
		}

		public static void FixSimulator (string path)
		{
			string fullPath = Path.Combine (path, Path.Combine ("Libraries", "RegisterMonoModules.cpp"));
			string data = Load (fullPath);

			data = Regex.Replace (data, @"\s+void\s+mono_dl_register_symbol\s+\(const\s+char\*\s+name,\s+void\s+\*addr\);", "");
			data = Regex.Replace (data, "typedef int gboolean;", "typedef int gboolean;\n\tvoid mono_dl_register_symbol (const char* name, void *addr);");

			if (!data.Contains ("#endif // !(TARGET_IPHONE_SIMULATOR) && !defined(__arm64__)")) {
				data = Regex.Replace (data, @"#endif\s+//\s*!\s*\(\s*TARGET_IPHONE_SIMULATOR\s*\)\s*}\s*void RegisterAllStrippedInternalCalls\s*\(\s*\)", "}\n\nvoid RegisterAllStrippedInternalCalls()");
				data = Regex.Replace (data, @"mono_aot_register_module\(mono_aot_module_mscorlib_info\);", 
			                     "mono_aot_register_module(mono_aot_module_mscorlib_info);\n#endif // !(TARGET_IPHONE_SIMULATOR)");			
			}

			Save (fullPath, data);
		}

		protected static string Load (string fullPath)
		{
			string data;
			FileInfo projectFileInfo = new FileInfo (fullPath);
			StreamReader fs = projectFileInfo.OpenText ();
			data = fs.ReadToEnd ();
			fs.Close ();

			return data;
		}

		protected static void Save (string fullPath, string data)
		{
			System.IO.StreamWriter writer = new System.IO.StreamWriter (fullPath, false);
			writer.Write (data);
			writer.Close ();
		}
	}
}
