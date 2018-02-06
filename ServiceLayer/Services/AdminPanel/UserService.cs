using CoreEntities.CustomModels.AdminPanel;
using ServiceLayer.Interfaces.AdminPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services.AdminPanel
{
    public class UserService : IUserService
    {
        private readonly RepositoryLayer.Repositories.Interfaces.IUserRepository _iUserRepository;
        public UserService(RepositoryLayer.Repositories.Interfaces.IUserRepository iUserRepository)
        {
            _iUserRepository = iUserRepository;
        }

        public RegisteredUser GetUserList(int pageNumber, int pageSize, string sortBy, bool reverse, string search)
        {
            return _iUserRepository.GetUserList(pageNumber, pageSize, sortBy, reverse, search);
        }

        public RegisteredUser GetUserDetails(long userMembershipId)
        {
            return _iUserRepository.GetUserDetails(userMembershipId);
        }
    }
}
