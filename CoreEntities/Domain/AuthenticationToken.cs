using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreEntities.Domain
{
    public class AuthenticationToken : BaseEntity
    {
        public AuthenticationToken()
        {
        }

        public AuthenticationToken(int id) : base(id)
        {
        }

        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AuthenticationTokenId { get; set; }

        [Required]
        public long AuthenticationDetailId { get; set; }

        [Required]
        public string AccessToken { get; set; }
    }
}
