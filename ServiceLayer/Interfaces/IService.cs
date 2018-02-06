using CoreEntities.CustomModels;
using CoreEntities.enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLayer.Interfaces
{
    public interface IService
    {
        ResponseModel UserAuthentication(string authorizedCode, AccountType accountType, DeviceInfo deviceInfo, RequestType reqType);

        Task ProcessSingleEmail(string emailId);
        Task<bool> DeleteContactFolder(Lead411UserInfo Lead411UserInfo, RequestType reqType);
        Lead411UserInfo GetUserInfoByUserId(Lead411UserInfo Lead411UserInfo, RequestType reqType);
        ResponseModel GetUserInfoByToken(string accessToken);
        bool CheckTokenExpired(string accessToken);
        Task ProcessMail(string msgText);

        ResponseModel GetNewAccessToken(string accessToken);
      

    }
}
