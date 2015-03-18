using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using System;
using System.Configuration;
using Newtonsoft.Json;

namespace Incremental_Backup
{
    public class Functions
    {
        [NoAutomaticTrigger]
        public static void QueueBackup([Queue("backupqueue")] ICollector<string> message, TextWriter log)
        {
            //This job should only run Monday - Thursday. So if it is Friday, Saturday or Sunday exit here.
            DateTime dt = DateTime.Now;
            if (dt.DayOfWeek == DayOfWeek.Friday || dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday) return;

            //Get timestamp for last Friday to save incremental backup to the last full backup
            while (dt.DayOfWeek != DayOfWeek.Friday) dt = dt.AddDays(-1);
            string datestamp = dt.ToString("yyyyMMdd");

            //extract the storage account names and keys
            string sourceUri, destUri, sourceKey, destKey;
            GetAcctInfo("Source-Account", "Backup-Account", out sourceUri, out destUri, out sourceKey, out destKey);

            //create a job name to make it easier to trace this through the WebJob logs.
            string jobName = "Incremental Backup";

            //set backup type either "full" or "incremental"
            string backup = "incremental";

            //Add the json from CreateJob() to the WebJobs queue, pass in the Container name for each call
            message.Add(CreateJob(jobName, "images", datestamp, sourceUri, destUri, sourceKey, destKey, backup, log));
            message.Add(CreateJob(jobName, "stuff", datestamp, sourceUri, destUri, sourceKey, destKey, backup, log));
        }
        public static void GetAcctInfo(string from, string to, out string sourceUri, out string destUri, out string sourceKey, out string destKey)
        {
            //Get the Connection Strings for the Storage Accounts to copy from and to
            string source = ConfigurationManager.ConnectionStrings[from].ToString();
            string dest = ConfigurationManager.ConnectionStrings[to].ToString();

            //Split the connection string along the semi-colon
            string sourceaccount = source.Split(';')[0].ToString();
            //write out the URI to the container 
            sourceUri = @"https://" + sourceaccount + @".blob.core.windows.net/";
            //and save the account key
            sourceKey = source.Split(';')[1].ToString();

            string destaccount = dest.Split(';')[0].ToString();
            destUri = @"https://" + destaccount + @".blob.core.windows.net/";
            destKey = dest.Split(';')[1].ToString();
        }
        public static string CreateJob(string job, string container, string datestamp, string sourceUri, string destUri, string sourceKey, string destKey, string backup, TextWriter log)
        {
            //Create a Dictionary object, then serialize it to pass to the WebJobs queue
            Dictionary<string, string> d = new Dictionary<string, string>();
            d.Add("job", job + " " + container);
            d.Add("source", sourceUri + container + @"/");
            d.Add("destination", destUri + container + datestamp);
            d.Add("sourcekey", sourceKey);
            d.Add("destkey", destKey);
            d.Add("backup", backup);
            log.WriteLine("Queued: " + job);

            return JsonConvert.SerializeObject(d);
        }
    }
}
