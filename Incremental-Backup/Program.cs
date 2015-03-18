using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace Incremental_Backup
{
    class Program
    {
        static void Main()
        {
            var host = new JobHost();
            host.Call(typeof(Functions).GetMethod("QueueBackup"));
        }
    }
}
