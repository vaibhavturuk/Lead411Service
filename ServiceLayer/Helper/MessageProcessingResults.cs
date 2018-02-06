using Microsoft.Office365.OutlookServices;

namespace ServiceLayer.Services.Helper
{
    public class MessageProcessingResults
    {
        public IMessage LastProcessedMessage { get; set; }
        public long ProcessedCount { get; set; }
    }
}