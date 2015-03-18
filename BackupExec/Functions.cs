using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using System.Diagnostics;
using Newtonsoft.Json;

namespace BackupExec
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written on the Azure WebJobs Queue called backupqueue
        public static void ExecuteAzCopy([QueueTrigger("backupqueue")] string message, TextWriter log)
        {
            string apptorun = @"D:\home\site\wwwroot\AzCopy\AzCopy.exe";
            Dictionary<string, string> d = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            log.WriteLine("Start Backup Job: " + d["job"] + " - " + DateTime.Now.ToString());

            StringBuilder arguments = new StringBuilder();
            arguments.Append(@"/source:" + d["source"]);
            arguments.Append(@" /dest:" + d["destination"]);
            arguments.Append(@" /sourcekey:" + d["sourcekey"]);
            arguments.Append(@" /destkey:" + d["destkey"]);
            //backup type: if "incremental" add /XO switch to arguments
            arguments.Append(@" /S /Y" + ((d["backup"] == "incremental") ? " /XO" : ""));

            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = apptorun,
                Arguments = arguments.ToString(),
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                ErrorDialog = false,
                CreateNoWindow = true
            };

            Process proc = new Process();
            proc.StartInfo = info;
            proc.Start();
            //max wait time, 3 hours = 10800000, 2 hours = 7200000, 1 hour = 3600000
            proc.WaitForExit(10800000);

            string msg = proc.StandardOutput.ReadToEnd();
            log.WriteLine(msg);
            log.WriteLine("Complete Backup Job: " + d["job"] + " - " + DateTime.Now.ToString());
            proc = null;
        }
    }
}
