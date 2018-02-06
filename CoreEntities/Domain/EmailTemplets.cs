using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreEntities.Domain
{
    public class EmailTemplets:BaseEntity
    {
        public EmailTemplets()
        {
            this.EmailDetails = new HashSet<EmailDetails>();
        }

        public EmailTemplets(int id) : base(id)
        {
        }

        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long EmailTempletId { get; set; }

        [Required]
        public long UserMembershipId { get; set; }

        public virtual UserMembership UserMembership { get; set; }

        [Required]
        public string EmailFrom { get; set; }

        [Required]
        public string EmailBody { get; set; }

        public string EmailSubject { get; set; }

        public string FileName { get; set; }
        public string FilePath { get; set; }

        public DateTime CreatedOn { get; set; }

        public ICollection<EmailDetails> EmailDetails { get; set; }
    }
}
