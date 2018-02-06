using System.IO;
using Microsoft.Azure.WebJobs;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Ninject;
using System.Collections.Generic;

namespace IndexerWebJob
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure 'gmailqueue' Queue.                                      
        //public static async Task GmailMailProcess([QueueTrigger("gmailqueue")] string message, TextWriter log)
        //public static void GmailMailProcess([GroupQueueTrigger("testqueue", 2)]List<object> message)
        public static async Task GmailMailProcess([QueueTrigger("gmailqueue")] string message, TextWriter log)
        {
            //foreach (var message in messages)
            //{
            var kernel = new StandardKernel(new IndexerJobBindings());
            var googleService = kernel.Get<ServiceLayer.Services.GoogleService>();

            await googleService.ProcessMail(message);


            log.WriteLine("Gmail Mail processed {0} on {1}", message, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
            //}
        }
    }
}