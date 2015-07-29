using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.NPNFEditor
{
	public class FixupFiles
	{
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

		public static void FixSimulator (string path)
		{
			string fullPath = Path.Combine (path, Path.Combine ("Libraries", "RegisterMonoModules.cpp"));
			string data = Load (fullPath);


			data = Regex.Replace (data, @"\s+void\s+mono_dl_register_symbol\s+\(const\s+char\*\s+name,\s+void\s+\*addr\);", "");
			data = Regex.Replace (data, "typedef int gboolean;", "typedef int gboolean;\n\tvoid mono_dl_register_symbol (const char* name, void *addr);");

			if (!data.Contains ("#endif // !(TARGET_IPHONE_SIMULATOR) && !defined(__arm64__)")) {
				data = Regex.Replace (data, @"#endif\s+//\s*!\s*\(\s*TARGET_IPHONE_SIMULATOR\s*\)\s*}\s*void RegisterAllStrippedInternalCalls\s*\(\s*\)", "}\n\nvoid RegisterAllStrippedInternalCalls()");
				if (!data.Contains ("mono_aot_register_module(mono_aot_module_mscorlib_info);\n#endif // !(TARGET_IPHONE_SIMULATOR)")) {
					data = Regex.Replace (data, @"mono_aot_register_module\(mono_aot_module_mscorlib_info\);", 
					"mono_aot_register_module(mono_aot_module_mscorlib_info);\n#endif // !(TARGET_IPHONE_SIMULATOR)");
				}
			}

			Save (fullPath, data);
		}
	}
}
