using CoreEntities.CustomModels;
using CoreEntities.Domain;
using CoreEntities.enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryLayer.Repositories.Interfaces
{
    public interface IAccountRespsitory
    {
       
        SuccessResponseModel AddUpdateUserInformation(string accessToken, AccountType accountType, UserDetail userInfoV1, DeviceInfo deviceInfo, RequestType requestType);
        void SaveRefreshToken(string refreshToken, string accessToken, string emailId, AccountType accountType);
        void SaveAccessToken(string accessToken,long AuthDetailId);
        string GetRefreshToken(AccountType accountType, DeviceInfo deviceInfo, string emailId);
        Lead411UserInfo GetUserInfoByUserId(long userId, DeviceInfo deviceInfo, string accessToken);
        bool SetUserInActive(long userId, DeviceInfo deviceInfo);
        Lead411UserInfo GetUserSessionEmailWise(string emailId);
        long SaveMailProcessParentCompleted(long userMembershipId);
        //void SaveNewMailProcessClients(long mailProcessParentId, long noOfMailsProcessed, DateTime newLastProcessedMailDate, long processDuration);
        MailProcessDates GetMailProcessDates(long mailProcessParentId);
        List<Lead411UserInfo> GetNextMailToBeProcessed(int numberOfmailsToBeProcessed);
        bool ResetIndexing(long userMembershipId);
        bool DeleteAccount(long userMembershipId);
        Lead411UserInfo GetUserSessionUserMembershipWise(long userMembershipId);
        //bool IsUserTrialPeriodExpired(string emailId, long trialDays);
        bool DisableAccount(long userMembershipId);
        RequestType GetRequestType(string emailId);
        AccountType GetAccountTypeByEmailId(string emailId);
        ResponseModel GetFilesByEmailId(string emailId);
        List<EmailFormat> GetEmailByTempletId(long emailTempletId, bool Isbounce);
        Lead411UserInfo GetRefreshTokenByAccessToken(string accessToken);
        Task<EmailTemplets> AddEmailDetail(List<EmailFormat> emailList, string subject, string body, string filename);
        void UpdateEmailDetails(EmailFormat emailformat);
        List<MessageText> GetMessageText(long emailTempletId);
        Task ChangeStatusOfProcess(long emailDetailsId);

        bool GetProcessStatus(long emailDetailsId);
        ResponseModel SaveContact(ContactView contact);
        ResponseModel GetContactDetails(string emailId, string userMailId);
        ResponseModel GetContactList(string emailId);
        ExportData ListOfStatus(long emailTempletId);

        ResponseModel GetContactListToExport(string emailId);

        MessageText ReProcessBouncedMail(long emailDetailsId);

    }
}
