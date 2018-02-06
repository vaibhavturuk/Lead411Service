using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreEntities.Domain
{
   public class EmailDetails : BaseEntity
    {
        public EmailDetails()
        {
        }

        public EmailDetails(int id) : base(id)
        {
        }


        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long EmailDetailsId { get; set; }

        [Required]
        public long EmailTempletId { get; set; }
        public string EmailFrom { get; set; }

        [Required]
        public string EmailTo { get; set; }

        public string EmployeCode { get; set; }

        public string FirstName { get; set; }

        public bool IsBounce { get; set; }
        public bool InProcess { get; set; }

        public int BounceStatus { get; set; }

        public string Notification { get; set; }
        public long UserMembershipId { get; set; }
        public string MessageId { get; set; }
        public string FileName { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? NoOfAttemp { get; set; }


    }
}
