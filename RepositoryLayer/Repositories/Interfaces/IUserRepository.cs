using CoreEntities.CustomModels.AdminPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Repositories.Interfaces
{
    public interface IUserRepository
    {
        RegisteredUser GetUserList(int pageNumber, int pageSize, string sortBy, bool reverse, string search);
        RegisteredUserDetails GetUserDetails(long userMembershipId);
    }
}
