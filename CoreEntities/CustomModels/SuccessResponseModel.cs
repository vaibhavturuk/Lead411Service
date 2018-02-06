using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreEntities.CustomModels
{
    public class SuccessResponseModel
    {
        public long UserId { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public bool IsOldUser { get; set; }
    }
}
