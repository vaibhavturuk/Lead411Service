using CoreEntities.CustomModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLayer.Interfaces
{
    public interface IMailProcess
    {
       Task Process(object service, string userId, List<string> labels, long userMembershipId, object userCredential, object contactList, object contactRequest);
    }
}
