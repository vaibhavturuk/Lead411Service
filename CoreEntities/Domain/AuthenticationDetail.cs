using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreEntities.Domain
{
    public class AuthenticationDetail : BaseEntity
    {
        public AuthenticationDetail()
        {
            this.AuthenticationTokens = new HashSet<AuthenticationToken>();
        }

        public AuthenticationDetail(int id) : base(id)
        {
        }

        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AuthenticationDetailId { get; set; }

        [Required]
        public long UserMembershipId { get; set; }

        public virtual UserMembership UserMembership { get; set; }

        [Required]
        public string RefreshToken { get; set; }

        [Required]
        public string Provider { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }

        public ICollection<AuthenticationToken> AuthenticationTokens { get; set; }

        
    }
}
