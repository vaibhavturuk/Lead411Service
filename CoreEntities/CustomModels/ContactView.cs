using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreEntities.CustomModels
{
    public class ContactView
    {
       
        public long ContactId { get; set; }
        public long UserMembershipId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string Company { get; set; }
        public string Address { get; set; }
        public string Website { get; set; }
        public string InternetCall { get; set; }
        public string IM { get; set; }
        public string JobTittle { get; set; }
        public string Notes { get; set; }
        public string Me { get; set; }
    }


}
