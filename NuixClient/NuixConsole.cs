using System;
using System.Collections.Generic;
using System.Text;

namespace NuixClient
{
    /// <summary>
    /// Methods for running nuix scripts via the console
    /// </summary>
    public static class NuixConsole
    {
        /// <summary>
        /// Runs a nuix script on a local instance of nuix console
        /// </summary>
        /// <param name="nuixConsoleExePath">The path to the nuix_console executable</param>
        /// <param name="scriptPath">The path to the script</param>
        /// <param name="useDongle"></param>
        public static string RunScript(string nuixConsoleExePath, string caseDirectory, string scriptPath, bool useDongle)
        {
            //Nuix_Console.exe -Dcase_dir="path" -licencesourcetype dongle C:\Users\MarkWainwright\Documents\NuixScripts\Script1.rb

            var arguments = new List<string>();

            if (!string.IsNullOrWhiteSpace(caseDirectory))
                arguments.Add($"-Dcase_dir=\"{caseDirectory}\"");
            if(useDongle)
                arguments.Add("-licencesourcetype dongle");
            if(!string.IsNullOrWhiteSpace(scriptPath))
                arguments.Add(@"C:\Users\MarkWainwright\Documents\NuixScripts\Script1.rb");
            

            using var pProcess = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = nuixConsoleExePath,
                    Arguments = string.Join(' ', arguments),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden, //don't display a window
                    CreateNoWindow = true
                }
            };
            
            
            pProcess.Start();
            var output = pProcess.StandardOutput.ReadToEnd(); //The output result TODO stream this
            pProcess.WaitForExit();

            return output;
        }


    }
}
