using CoreEntities.CustomModels;
using CoreEntities.enums;
using RestSharp;
using ServiceLayer.Helper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ServiceLayer.Interfaces
{
    public interface ICommonService
    {
        Lead411UserInfo GetUserInfoByUserId(long userId, DeviceInfo deviceInfo, string accessToken, RequestType reqType);
        bool SetUserInActive(long userId, DeviceInfo deviceInfo);
        List<Lead411UserInfo> GetNextMailToBeProcessed();
        bool ResetIndexing(long userMembershipId);
        bool DeleteAccount(long userMembershipId);
        IRestResponse RestClientRequest(RequestObject requestObject);
        Lead411UserInfo GetUserSessionUserMembershipWise(long userMembershipId);
        AccountType GetAccountTypeByEmailId(string emailId);
        Task<ResponseModel> PushMailInDB(string subject, string body, DataSet res, string fileName, string emailFrom, string filePath);
        Task PushMailInQueue(List<MessageText> mailList);
        string Base64UrlEncode(string input);
        ResponseModel GetListOfFile(string emailId);
        List<EmailFormat> GetListOfEmail(long emailTempletId, bool isBounce);
        List<MessageText> GetMessageText(long emailTempletId);
        bool CheckProcessStatus(long emailDetailsId);
        ResponseModel SaveOrUpdateContact(ContactView contact);
        ResponseModel GetContactDetails(string emailId, string userMailId);
        ResponseModel GetContactList(string emailId);
        ResponseModel ExportToExcel(long emailTempletId);
        DataTable ConvertToDataTable<T>(IList<T> data);
        ResponseModel ExportStatusToExcel(DataTable table, long emailTempletId);
        ExportData GetExportData(long emailTempletId);

        ResponseModel GetContactListToExport(string emailId);
    }
}
