﻿using Microsoft.Azure.WebJobs;

namespace IndexerWebJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            JobHostConfiguration config = new JobHostConfiguration();
            //config.UseGroupQueueTriggers();
            config.Queues.BatchSize = 32;
            var host = new JobHost(config);

            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }
    }
}
