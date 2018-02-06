using System;
using System.ComponentModel;

namespace CoreEntities.CustomModels
{
    public class MailProcessDates
    {
        [DefaultValue(false)]
        public bool IsOldMailProcessCompleted { get; set; }

        public DateTime? NewLastProcessedMailDate { get; set; }

        public DateTime? OldLastProcessedMailDate { get; set; }
    }
}
