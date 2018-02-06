using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.WebJobs;
using Ninject;
using CoreEntities.CustomModels;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage;
using CoreEntities.enums;

namespace ControllerWebJob
{
    public class Functions
    {
        /// <summary>
        /// Create Queue messages of mail batch to be process.
        /// </summary>
        public static void SetMailsToBeProcessed(TextWriter log, TimeSpan value, [Queue("dummyinitilization")] ICollector<string> message)
        {
            var kernel = new StandardKernel(new ControllerJobBindings());
            var commonService = kernel.Get<ServiceLayer.Services.CommonService>();

            //List<Lead411UserInfo> mailList = commonService.GetNextMailToBeProcessed();
            ///Need to edit. giving value for run the Controllerwebjob
            List<Lead411UserInfo> mailList=new List<Lead411UserInfo>() { new Lead411UserInfo() {Email="kartikbhave.sdn@gmail.com",Provider="Gmail" } };
            if (mailList != null)
            {
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                    System.Configuration.ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);

                foreach (Lead411UserInfo email in mailList)
                {
                    // Create the queue client.
                    CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

                    if (queueClient != null)
                    {
                        CloudQueue queue = queueClient.GetQueueReference((AccountType)Enum.Parse(typeof(AccountType), email.Provider) == AccountType.Gmail ? "gmailqueue" : "outlookqueue");

                        // Create the queue if it doesn't already exist.
                        queue.CreateIfNotExists();

                        // Create a message and add it to the queue.
                        CloudQueueMessage msg = new CloudQueueMessage(email.Email);
                        queue.AddMessage(msg);
                    }
                }

                log.WriteLine("Following mail will be processing on the Queue at " + value);
            }
            else
            {
                log.WriteLine("No emails found to process");
            }
        }
    }
}