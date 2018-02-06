using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreEntities.CustomModels.AdminPanel
{
    public class RegisteredUserDetails : RegisteredUser
    {
        public List<string> ApplicationType { get; set; }

        [DefaultValue(false)]
        public bool IsOldProcessCompleted { get; set; }

        public DateTime? OldProcessCompletedDate { get; set; }

        public long? NoOfIndexersTakenToComplateOldProcess { get; set; }

        public long? TotalNoOfMailsProcessed { get; set; }

        public long? NoOfIndexerProcessRunForNewMail { get; set; }

        public long? TimeTakenToFinishedOldMailProcess { get; set; }

        public long? TotalTimeTakenByProcessTillNow { get; set; }

        public long? TotalNoOfIndexerRunTillNow { get; set; }

        public string CurrentAccessToken { get; set; }

        public string CurrentRefreshToken { get; set; }

        public DateTime? LastDateOfIndexerRun { get; set; }
    }
}
