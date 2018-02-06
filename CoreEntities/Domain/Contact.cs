using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreEntities.Domain
{
    public class Contact: BaseEntity
    { 
        public Contact()
        {
        }

        public Contact(int id) : base(id)
        {
        }



        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ContactId { get; set; }

        [Required]
        public long UserMembershipId { get; set; }

        //public virtual UserMembership UserMembership { get; set; }


        //[Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string Company { get; set; }
        public string Address { get; set; }
        public string Website { get; set; }
        public string InternetCall { get; set; }
        public string IM { get; set; }
        public string JobTittle { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
    }
}
