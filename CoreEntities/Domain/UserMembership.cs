using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreEntities.Domain
{
    public class UserMembership : BaseEntity
    {
        public UserMembership()
        { 
            this.AuthenticationDetails = new HashSet<AuthenticationDetail>();
          
        }

        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UserMembershipId { get; set; }

        [Required]
        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        public string Password { get; set; }


        [Required]
        public string Provider { get; set; }


        [DefaultValue(true)]
        public bool IsActive { get; set; }

        public ICollection<AuthenticationDetail> AuthenticationDetails { get; set; }

     
    }
}
