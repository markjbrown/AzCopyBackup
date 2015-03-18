using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace BackupExec
{
    class Program
    {
        static void Main()
        {
            JobHostConfiguration config = new JobHostConfiguration();

            //AzCopy cannot be invoked multiple times in the same host
            //process, so read and process one message at a time
            config.Queues.BatchSize = 1;
            var host = new JobHost(config);
            host.RunAndBlock();
        }
    }
}
