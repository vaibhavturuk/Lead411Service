
namespace CoreEntities.Helper
{
    public class GeneralParameters
    {
        public static string RedirectUrlMobile { get; } = "http://localhost/callback";//
        //For Azure Staging
        //public static string RedirectUrlWeb { get; } = "http://lead411testapp.azurewebsites.net/test/index.html";
        //public static string RedirectUrlApplication { get; } = "http://lead411testapp.azurewebsites.net/test/index.html";
        //end/////
        //For Azure
        public static string RedirectUrlWeb { get; } = "http://lead411chromeex.azurewebsites.net/test/index.html";
        public static string RedirectUrlApplication { get; } = "http://lead411chromeex.azurewebsites.net/test/index.html";
        //end/////
        //For Local
        //public static string RedirectUrlWeb { get; } = "http://localhost:55555";
        //public static string RedirectUrlApplication { get; } = "http://localhost:55555";
        //end/////
        //For 108
        //public static string RedirectUrlWeb { get; } = "http://stagingwin.com/lead411/test/index.html";
        //public static string RedirectUrlApplication { get; } = "http://stagingwin.com/lead411/test/index.html";
        //end/////


        public static string RedirectUrlExtension { get; } = "https://mail.google.com";
        public static int NumberOfConcurrentJobs { get; } = 5;
        public static int MaximumSingleRunIndexTimeInMinutes { get; } = 2 * 60 * 1000;
        public static int MaximumSingleRunIndexCount { get; } = 300;
        public static string Lead411FolderName { get; } = "Lead411";
        public static string TrialPeriodInDays { get; } = "15";
    }
}
