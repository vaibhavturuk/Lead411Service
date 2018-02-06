using CoreEntities.CustomModels.AdminPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Interfaces.AdminPanel
{
    public interface IUserService
    {
        RegisteredUser GetUserList(int pageNumber, int pageSize, string sortBy, bool reverse, string search);

        RegisteredUser GetUserDetails   (long userMembershipId);
    }
}
