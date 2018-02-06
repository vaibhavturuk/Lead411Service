using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreEntities.CustomModels
{
   public class EmailFormat
    {
        public long EmailDetailsId { get; set; }
        public long EmailTempletId { get; set; }
        public string EmailFrom { get; set; }
        public string EmployeCode { get; set; }
        public string EmailTo { get; set; }
        public string FirstName { get; set; }

        public string EmailBody { get; set; }

        public string EmailSubject { get; set; }

        public string Provider { get; set; }
        public bool IsBounce { get; set; }
        public int BounceStatus { get; set; }
        public bool InProcess { get; set; }
        public string Notification { get; set; }
        public string MessageId { get; set; }
        public long UserMembershipId { get; set; }
        public string FileName { get; set; }

    }
}
