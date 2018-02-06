using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreEntities.CustomModels.AdminPanel
{
    public class RegisteredUser
    {
        public long UserId { get; set; }

        public long UserMembershipId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Provider { get; set; }

        public string EmailId { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }

        public int NoOfRecords { get; set; }

       

        public List<RegisteredUser> RegisteredUsers { get; set; }

    }
}
