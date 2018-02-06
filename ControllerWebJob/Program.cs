using Microsoft.Azure.WebJobs;

namespace ControllerWebJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        /// <summary>
        /// Mail method to be call on regular interval.
        /// </summary>
        static void Main()
        {
            var host = new JobHost();

            // The following code ensures that the WebJob will be running continuously
            host.Call(typeof(Functions).GetMethod("SetMailsToBeProcessed"), new { value = System.DateTime.UtcNow.TimeOfDay });
        }
    }
}