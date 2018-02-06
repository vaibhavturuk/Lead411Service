using System;

namespace CoreEntities.CustomModels
{
    public class Lead411UserInfo
    {
        public long UserMembershipId { get; set; }

        public string Email { get; set; }

        public string Provider { get; set; }

        public string RefreshToken { get; set; }

        public string AccessToken { get; set; }

        public DateTime? CreatedOn { get; set; }
    }
}
