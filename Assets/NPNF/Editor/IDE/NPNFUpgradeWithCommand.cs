using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NPNF.Upgrade
{
    public class NPNFUpgradeWithCommand
    {
        public const string FILE_NAME_UNINSTALL = "npnf_uninstall";

        static string Extension
        {
            get
            {
                #if UNITY_EDITOR_OSX
                return "sh";
                #elif UNITY_EDITOR_WIN
                return "bat";
                #else
                return string.Empty;
                #endif
            }
        }

        static string FileName
        {
            get
            {
                return string.Format("{0}.{1}", FILE_NAME_UNINSTALL, Extension);
            }
        }

        public static bool RunCommandLine()
        {
            bool isError = false;
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.UseShellExecute = false;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.WorkingDirectory = Application.dataPath;
#if UNITY_EDITOR_OSX
                startInfo.FileName = Extension;
			    startInfo.Arguments = FileName;
#elif UNITY_EDITOR_WIN
                startInfo.FileName = "C:\\Windows\\system32\\cmd.exe";
                startInfo.Arguments = string.Format("/c {0}", FileName);
#endif
                Process p = Process.Start(startInfo);
                p.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    if (e.Data != null)
                        UnityEngine.Debug.Log("CommandLine: " + e.Data);
                };
                p.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    if (e.Data != null)
                    {
                        UnityEngine.Debug.LogError("CommandLine: " + e.Data);
                        isError = true;
                    }
                };
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("CommandLine: " + e.Message);
                return false;
            }
            return !isError;
        }

    }
}
