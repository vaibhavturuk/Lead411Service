using CoreEntities.CustomModels;
using CoreEntities.Domain;
using RepositoryLayer.Repositories.Interfaces;
using System;
using System.Linq;
using CoreEntities.enums;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;

namespace RepositoryLayer.Repositories
{
    public class AccountRepository : IAccountRespsitory
    {
        private AzureDbContext dbcontext;
        public object MailProcessParentsList { get; }

        public AccountRepository(object mailProcessParentsList)
        {
            MailProcessParentsList = mailProcessParentsList;
            if (dbcontext == null)
            {
                dbcontext = new AzureDbContext();
                Database.SetInitializer<AzureDbContext>(null);
            }
        }

        /// <summary>
        /// Create new user on Lead411 application
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="accountType"></param>
        /// <param name="userInfoV1"></param>
        /// <param name="userInfoV2"></param>
        /// <param name="deviceInfo"></param>
        /// <param name="requestType"></param>
        public SuccessResponseModel AddUpdateUserInformation(
            string accessToken,
            AccountType accountType,
            UserDetail userInfoV1,
            DeviceInfo deviceInfo,
            RequestType requestType)
        {
            SuccessResponseModel successResponseModel = new SuccessResponseModel();
            try
            {
                if (accountType == AccountType.Gmail)
                {
                    if (userInfoV1 != null)
                    {
                        //string emailid = userInfoV1.email.Split('@')[0] + "@dotalign.com";
                        UserMembership oldUser = (from membership in dbcontext.UserMembership
                                                  where membership.Email == userInfoV1.email
                                                  select membership).FirstOrDefault();
                        successResponseModel.IsOldUser = true;
                        if (oldUser == null)
                        {
                            successResponseModel.IsOldUser = false;
                            var users = dbcontext.UserMembership.Add(new UserMembership()
                            {
                                IsActive = true,
                                IsDeleted = false,
                                CreatedBy = 1,
                                CreatedOn = DateTime.UtcNow,
                                Email = userInfoV1.email,
                                FirstName = userInfoV1.given_name,
                                LastName = userInfoV1.family_name,
                                Provider = accountType.ToString()
                            });

                            dbcontext.SaveChanges();

                        }

                    }
                }
                return successResponseModel;
            }
            catch (Exception excep)
            {
                var msg = excep.Message;
                return successResponseModel;
            }

        }

        /// <summary>
        /// Save refresh token in db
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <param name="accessToken"></param>
        /// <param name="emailId"></param>
        /// <param name="accountType"></param>
        public void SaveRefreshToken(
            string refreshToken,
            string accessToken,
            string emailId,
            AccountType accountType)
        {
            if (!string.IsNullOrEmpty(refreshToken))
            {
                long userMembershipId = (from userMembership in dbcontext.UserMembership.Where(x => x.Email == emailId)//&& x.IsActive == true && x.IsDeleted == false
                                         select userMembership.UserMembershipId).FirstOrDefault();
                if (userMembershipId > 0)
                {
                    var authenticationDetail = dbcontext.AuthenticationDetail.Where(authDetails => authDetails.UserMembershipId == userMembershipId && authDetails.RefreshToken == refreshToken).OrderByDescending(authDetails => authDetails.AuthenticationDetailId).FirstOrDefault();
                    if (authenticationDetail != null)
                    {
                        authenticationDetail.AuthenticationTokens.Add(new AuthenticationToken()
                        {
                            AccessToken = accessToken,
                            CreatedBy = 1,
                            CreatedOn = DateTime.UtcNow,
                            IsDeleted = false,

                        });
                    }
                    else
                    {
                        authenticationDetail = dbcontext.AuthenticationDetail.Add(new AuthenticationDetail()
                        {
                            CreatedBy = 1,
                            CreatedOn = DateTime.UtcNow,
                            IsActive = true,
                            RefreshToken = refreshToken,
                            UserMembershipId = userMembershipId,
                            Provider = accountType.ToString()
                        });

                        authenticationDetail.AuthenticationTokens.Add(new AuthenticationToken()
                        {
                            AccessToken = accessToken,
                            CreatedBy = 1,
                            CreatedOn = DateTime.UtcNow,
                        });
                    }
                    dbcontext.SaveChanges();
                }
            }
        }


        /// <summary>
        /// Save refresh token in db
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <param name="accessToken"></param>
        /// <param name="emailId"></param>
        /// <param name="accountType"></param>
        public void SaveAccessToken(
            string accessToken,
            long AuthDetailId
            )
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                var token = dbcontext.AuthenticationToken.Add(new AuthenticationToken()
                {
                    AccessToken = accessToken,
                    AuthenticationDetailId = AuthDetailId,
                    CreatedBy = 1,
                    CreatedOn = DateTime.UtcNow,
                    IsDeleted = false
                });
                dbcontext.SaveChanges();

            }
        }



        /// <summary>
        /// Get refresh token
        /// </summary>
        /// <param name="accountType"></param>
        /// <param name="deviceInfo"></param>
        /// <param name="emailId"></param>
        public string GetRefreshToken(
            AccountType accountType,
            DeviceInfo deviceInfo,
            string emailId)
        {
            string refreshToken = (from userMembership in dbcontext.UserMembership
                                   join userAuthentication in dbcontext.AuthenticationDetail on userMembership.UserMembershipId equals userAuthentication.UserMembershipId
                                   where userMembership.Provider == accountType.ToString()
                                   && userMembership.Email == emailId
                                   && userMembership.IsActive && userMembership.IsDeleted == false
                                   && userAuthentication.IsActive && userAuthentication.IsDeleted == false
                                   orderby userAuthentication.AuthenticationDetailId descending
                                   select userAuthentication.RefreshToken).FirstOrDefault();
            return refreshToken;
        }

        /// <summary>
        /// Get refresh token by access token
        /// </summary>
        /// <param name="accountType"></param>
        public Lead411UserInfo GetRefreshTokenByAccessToken(
            string accessToken)
        {
            var refreshToken = (from authToken in dbcontext.AuthenticationToken
                                from authDetail in dbcontext.AuthenticationDetail
                                where authToken.AccessToken == accessToken &&
                                authToken.AuthenticationDetailId == authDetail.AuthenticationDetailId
                                select new Lead411UserInfo()
                                {
                                    UserMembershipId = authDetail.AuthenticationDetailId,
                                    RefreshToken = authDetail.RefreshToken,
                                    CreatedOn = authDetail.CreatedOn

                                }).OrderByDescending(x => x.CreatedOn).FirstOrDefault();


            return refreshToken;
        }
        /// <summary>
        /// Get user information by userid and access token. (After login when user start application next time it checks existance with this method.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="deviceInfo"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public Lead411UserInfo GetUserInfoByUserId(
            long userId,
            DeviceInfo deviceInfo,
            string accessToken)
        {
            Lead411UserInfo Lead411UserInfo = null;
            var isValidUser = (from userMembership in dbcontext.UserMembership
                               join authenticationDetail in dbcontext.AuthenticationDetail on userMembership.UserMembershipId equals authenticationDetail.UserMembershipId
                               join authenticationToken in dbcontext.AuthenticationToken on authenticationDetail.AuthenticationDetailId equals authenticationToken.AuthenticationDetailId
                               where userMembership.UserMembershipId == userId
                               select userMembership.UserMembershipId).Count();

            if (isValidUser > 0)
            {
                Lead411UserInfo = (from userMembership in dbcontext.UserMembership.Where(x => x.UserMembershipId == userId)
                                   join userAuthentication in dbcontext.AuthenticationDetail on userMembership.UserMembershipId equals userAuthentication.UserMembershipId
                                   join authenticationToken in dbcontext.AuthenticationToken on userAuthentication.AuthenticationDetailId equals authenticationToken.AuthenticationDetailId
                                   where userAuthentication.Provider == AccountType.Gmail.ToString()
                                    && userMembership.IsActive && userMembership.IsDeleted == false
                                    && userAuthentication.IsActive && userAuthentication.IsDeleted == false
                                   orderby authenticationToken.AuthenticationTokenId descending
                                   select new Lead411UserInfo
                                   {
                                       UserMembershipId = userMembership.UserMembershipId,
                                       RefreshToken = userAuthentication.RefreshToken,
                                       AccessToken = authenticationToken.AccessToken,
                                       Email = userMembership.Email,
                                       Provider = userMembership.Provider
                                   }).FirstOrDefault();

                if (Lead411UserInfo == null)
                {
                    Lead411UserInfo = (from userMembership in dbcontext.UserMembership.Where(x => x.UserMembershipId == userId)
                                       join userAuthentication in dbcontext.AuthenticationDetail on userMembership.UserMembershipId equals userAuthentication.UserMembershipId
                                       join authenticationToken in dbcontext.AuthenticationToken on userAuthentication.AuthenticationDetailId equals authenticationToken.AuthenticationDetailId
                                       where userAuthentication.Provider == AccountType.Office365.ToString()
                                        && userMembership.IsActive && userMembership.IsDeleted == false
                                        && userAuthentication.IsActive && userAuthentication.IsDeleted == false
                                       orderby authenticationToken.AuthenticationTokenId descending
                                       select new Lead411UserInfo
                                       {
                                           UserMembershipId = userMembership.UserMembershipId,
                                           RefreshToken = userAuthentication.RefreshToken,
                                           AccessToken = authenticationToken.AccessToken,
                                           Email = userMembership.Email,
                                           Provider = userMembership.Provider
                                       }).FirstOrDefault();
                }
            }
            return Lead411UserInfo;
        }

        /// <summary>
        /// Set user active or inactive from front end into database.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="deviceInfo"></param>
        /// <returns></returns>
        public bool SetUserInActive(long userId, DeviceInfo deviceInfo)
        {
            try
            {
                List<UserMembership> userMembershipList = (from users in dbcontext.UserMembership.Where(x => x.IsDeleted == false && x.IsActive)
                                                           where users.IsActive && users.IsDeleted == false
                                                           && users.IsActive && users.IsDeleted == false
                                                           select users).ToList();

                foreach (UserMembership userMembership in userMembershipList)
                {
                    userMembership.IsActive = false;
                }

                dbcontext.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get active session of user by email id.
        /// </summary>
        /// <param name="emailId"></param>
        /// <returns></returns> 
        public Lead411UserInfo GetUserSessionEmailWise(string emailId)
        {
            //emailId = Uri.UnescapeDataString(emailId);
            Lead411UserInfo Lead411UserInfo = (from userMembership in dbcontext.UserMembership.Where(user => user.Email == emailId && user.IsActive == true && user.IsDeleted != true)
                                               join authDetails in dbcontext.AuthenticationDetail.Where(auth => auth.IsActive == true && auth.IsDeleted != true) on userMembership.UserMembershipId equals authDetails.UserMembershipId
                                               join authenticationToken in dbcontext.AuthenticationToken on authDetails.AuthenticationDetailId equals authenticationToken.AuthenticationDetailId
                                               orderby authenticationToken.AuthenticationTokenId descending
                                               select new Lead411UserInfo()
                                               {
                                                   AccessToken = authenticationToken.AccessToken,
                                                   RefreshToken = authDetails.RefreshToken,
                                                   Email = userMembership.Email,
                                                   Provider = userMembership.Provider,
                                                   UserMembershipId = userMembership.UserMembershipId
                                               }).FirstOrDefault();
            return Lead411UserInfo;
        }

        /// <summary>
        /// Save mail process completed end date of parent.
        /// </summary>
        /// <param name="userMembershipId"></param>
        /// <returns></returns>
        public long SaveMailProcessParentCompleted(long userMembershipId)
        {
            var count = 0;
            if (userMembershipId > 0)
            {
                Lead411UserInfo lead = new Lead411UserInfo();
                lead.UserMembershipId = userMembershipId;
                count++;

            }
            return 0;
        }


        /// <summary>Pooja
        /// Save log after completion of new mail process.
        /// </summary>
        /// <param name="mailProcessParentId"></param>
        /// <param name="noOfMailsProcessed"></param>
        /// <param name="newLastProcessedMailDate"></param>
        /// <param name="processDuration"></param>
        //public void SaveNewMailProcessClients(
        //    long mailProcessParentId, 
        //    long noOfMailsProcessed, 
        //    DateTime newLastProcessedMailDate, 
        //    long processDuration)
        //{
        //    if (mailProcessParentId > 0)
        //    {
        //        dbcontext.NewMailProcessChild.Add(new NewMailProcessChild()
        //        {
        //            CreatedBy = 1,
        //            CreatedOn = DateTime.UtcNow,
        //            IsDeleted = false,
        //            MailProcessParentId = mailProcessParentId,
        //            NoOfMailsProcessed = noOfMailsProcessed,
        //            NewLastProcessedMailDate = newLastProcessedMailDate,
        //            ProcessDuration = processDuration
        //        });

        //        dbcontext.SaveChanges();
        //    }
        //}

        /// <summary>
        /// Get dates of old and new mail process from db.
        /// </summary>
        /// <param name="mailProcessParentId"></param>
        /// <returns></returns>
        public MailProcessDates GetMailProcessDates(long mailProcessParentId)
        {
            MailProcessDates mail = new MailProcessDates();
            //MailProcessDates mailProcessDates = (from mailProcessParent in dbcontext.MailProcessParent.Where(mailp => mailp.MailProcessParentId == mailProcessParentId && mailp.IsDeleted == false)
            //                                     select new MailProcessDates
            //                                     {
            //                                         IsOldMailProcessCompleted = mailProcessParent.IsOldMailProcessCompleted,
            //                                     }).FirstOrDefault();


            return mail;//return mailProcessDates;
        }

        /// <summary>
        /// Get batch of mails to be processed.
        /// </summary>
        /// <param name="numberOfmailsToBeProcessed"></param>
        /// <returns></returns>
        public List<Lead411UserInfo> GetNextMailToBeProcessed(int numberOfmailsToBeProcessed)
        {
            List<Lead411UserInfo> mailList = new List<Lead411UserInfo>();
            //List<Lead411UserInfo> userMembershipList;

            //var mailProcessLog = dbcontext.MailProcessLog.Where(x => x.IsDeleted == false)
            //    .OrderByDescending(x => x.MailProcessLogId)
            //    .FirstOrDefault();

            //if (mailProcessLog != null)
            //{
            //    mailList = dbcontext.UserMembership.Where(x => x.IsDeleted == false && x.IsActive).OrderBy(x => x.UserMembershipId)
            //        .Where(umember => umember.UserMembershipId > mailProcessLog.UserMembershipId && umember.IsActive)
            //        .OrderBy(x => x.UserMembershipId)
            //        .Take(numberOfmailsToBeProcessed)
            //        .Select(x=> new Lead411UserInfo {Provider =x.Provider,Email = x.Email,UserMembershipId = x.UserMembershipId})
            //        .ToList();

            //    //mailList.AddRange(userMembershipList.Select(x =>new { x.Email,x.Provider}).AsEnumerable());

            //    if (mailList.Count() < numberOfmailsToBeProcessed)
            //    {
            //        userMembershipList = dbcontext.UserMembership.Where(x => x.IsDeleted == false && x.IsActive)
            //            .Where(umember => umember.UserMembershipId <= mailProcessLog.UserMembershipId && umember.IsActive)
            //            .OrderBy(x => x.UserMembershipId)
            //            .Take(numberOfmailsToBeProcessed - mailList.Count())
            //            .Select(x => new Lead411UserInfo { Provider = x.Provider, Email = x.Email, UserMembershipId = x.UserMembershipId })
            //            .ToList();

            //        mailList.AddRange(userMembershipList);
            //    }
            //}
            //else
            //{
            //    userMembershipList = dbcontext.UserMembership.Where(x => x.IsDeleted == false && x.IsActive)
            //        .OrderBy(x => x.UserMembershipId)
            //        .Take(numberOfmailsToBeProcessed)
            //        .Select(x => new Lead411UserInfo { Provider = x.Provider, Email = x.Email, UserMembershipId = x.UserMembershipId })
            //        .ToList();

            //    mailList.AddRange(userMembershipList);
            //}

            //SaveLog(mailList.Count, mailList);
            //
            //


            return mailList;
        }

        /// <summary>
        /// Save log of number of mails process in request.
        /// </summary>
        /// <param name="numberOfmailsToBeProcessed"></param>
        /// <param name="userMembership"></param>
        //private void SaveLog(int numberOfmailsToBeProcessed, List<Lead411UserInfo> userMembership)
        //{
        //    dbcontext.MailProcessLog.Add(new MailProcessLog()
        //    {
        //        CreatedBy = 1,
        //        CreatedOn = DateTime.UtcNow,
        //        IsDeleted = false,
        //        NoOfMailsProcessed = numberOfmailsToBeProcessed,
        //        UserMembershipId = userMembership.Max(m => m.UserMembershipId)
        //    });

        //    dbcontext.SaveChanges();
        //}

        /// <summary>
        /// Mark inactive to the entries of processed mail in db email wise.
        /// </summary>
        /// <param name="userMembershipId"></param>
        /// <returns></returns>
        public bool ResetIndexing(long userMembershipId)
        {
            try
            {
                //var mailProcessParentsList2 = dbcontext.MailProcessParent.Where(x => x.UserMembershipId == userMembershipId && x.IsDeleted == false).ToList();
                //if (mailProcessParentsList2.Count > 0)
                //{
                //    foreach (var mailProcessParent in mailProcessParentsList2)
                //    {
                //        mailProcessParent.IsDeleted = true;
                //        mailProcessParent.DeletedBy = 1;
                //        mailProcessParent.DeletedOn = DateTime.UtcNow;


                //    }
                //    dbcontext.SaveChanges();
                //}
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Set User membership account inactive.
        /// </summary>
        /// <param name="userMembershipId"></param>
        /// <returns></returns>
        public bool DeleteAccount(long userMembershipId)
        {
            try
            {
                var userMembership = dbcontext.UserMembership.Where(x => x.UserMembershipId == userMembershipId && x.IsDeleted == false).FirstOrDefault();
                if (userMembership != null)
                {
                    ResetIndexing(userMembershipId);
                    userMembership.IsActive = false;
                    dbcontext.SaveChanges();
                    return true;
                }
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get full active session of user by user membership id.
        /// </summary>
        /// <param name="userMembershipId"></param>
        /// <returns></returns>
        public Lead411UserInfo GetUserSessionUserMembershipWise(long userMembershipId)
        {
            var authDetail = (from authenticationDetail in dbcontext.AuthenticationDetail.Where(authDetails => authDetails.UserMembershipId == userMembershipId)
                              join authenticationToken in dbcontext.AuthenticationToken on authenticationDetail.AuthenticationDetailId equals authenticationToken.AuthenticationDetailId
                              orderby authenticationDetail.AuthenticationDetailId descending
                              select new { authenticationDetail, authenticationToken }
                               ).FirstOrDefault();

            if (authDetail != null)
            {
                Lead411UserInfo Lead411UserInfo =
                    (from userMembership in
                        dbcontext.UserMembership.Where(user => user.UserMembershipId == userMembershipId)
                     select new Lead411UserInfo()
                     {
                         AccessToken = authDetail.authenticationToken.AccessToken,
                         RefreshToken = authDetail.authenticationDetail.RefreshToken,
                         Email = userMembership.Email,
                         Provider = userMembership.Provider,
                         UserMembershipId = userMembership.UserMembershipId
                     }).FirstOrDefault();

                return Lead411UserInfo;
            }
            else
                return null;
        }


        public bool DisableAccount(long userMembershipId)
        {
            try
            {
                var userMembership = dbcontext.UserMembership.Where(x => x.UserMembershipId == userMembershipId && x.IsDeleted == false).FirstOrDefault();
                if (userMembership != null)
                {
                    userMembership.IsActive = false;
                    dbcontext.SaveChanges();
                    return true;
                }
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get request type
        /// </summary>
        /// <param name="emailId"></param>
        /// <returns></returns>
        public RequestType GetRequestType(string emailId)
        {
            try
            {
                //var appType = (from usersMembership in dbcontext.UserMembership.Where(x => x.Email == emailId && x.IsDeleted == false)
                //               select applicationType.AppType).FirstOrDefault();

                //if (appType != null) return (RequestType)Enum.Parse(typeof(RequestType), appType);
            }
            catch
            {
                return RequestType.web;
            }
            return RequestType.web;
        }

        /// <summary>
        /// Get the AccountTyppe by emailID
        /// </summary>
        /// <param name="emailId"></param>
        /// <returns>AccountType</returns>
        public AccountType GetAccountTypeByEmailId(string emailId)
        {
            try
            {
                var appType = dbcontext.UserMembership.Where(x => x.Email == emailId && x.IsDeleted == false).Select(x => x.Provider).FirstOrDefault();

                if (appType != null) return (AccountType)Enum.Parse(typeof(AccountType), appType);
            }
            catch
            {
                return AccountType.Gmail;
            }
            return AccountType.Gmail;
        }



        /// <summary>
        /// Save the email to local DB Lead411
        /// </summary>
        /// <param name="emailList"></param>
        /// <returns></returns>

        /// <summary>
        /// Add Email detail and Templete Detail in DB
        /// </summary>
        /// <param name="userMembershipId"></param>
        /// <returns></returns>
        public async Task<EmailTemplets> AddEmailDetail(
            List<EmailFormat> emailList, string subject, string body, string filename)
        {
            ResponseModel ResponseModel = new ResponseModel();
            List<EmailDetails> eList = new List<EmailDetails>();
            EmailTemplets eTemp = new EmailTemplets();
            try
            {

                var Efrom = (from em in emailList select em.EmailFrom).FirstOrDefault();
                var user = (from us in dbcontext.UserMembership
                            where us.Email == Efrom
                            select us).FirstOrDefault();
                var userId = user.UserMembershipId;
                var cdate = DateTime.UtcNow;
                if (emailList != null)
                {
                    var temp = (new EmailTemplets()
                    {
                        EmailFrom = Efrom,
                        UserMembershipId = userId,
                        EmailBody = body,
                        EmailSubject = subject,
                        FileName = filename,
                        FilePath = Convert.ToString(emailList.FirstOrDefault().FileName),
                        CreatedBy = userId,
                        CreatedOn = DateTime.UtcNow,
                        IsDeleted = false
                    });
                    eTemp = dbcontext.EmailTemplets.Add(temp);
                    var asd = dbcontext.SaveChanges();


                    foreach (var email in emailList)
                    {
                        var emails = (new EmailDetails()
                        {
                            EmailTempletId = temp.EmailTempletId,
                            EmailFrom = email.EmailFrom,
                            EmailTo = email.EmailTo,
                            EmployeCode = email.EmployeCode,
                            FirstName = email.FirstName,
                            UserMembershipId = userId,
                            IsBounce = email.IsBounce,
                            FileName = email.FileName,
                            InProcess = true,
                            CreatedBy = 1,
                            CreatedOn = DateTime.UtcNow,
                            NoOfAttemp = 1
                        });
                        eList.Add(emails);
                    }
                    var pqr = dbcontext.EmailDetails.AddRange(eList);
                    var xyz = dbcontext.SaveChanges();

                    var ddate = DateTime.Now;

                    return eTemp;
                    //ResponseModel.Message = "Email Save";
                    //ResponseModel.IsSuccess = true;
                    //ResponseModel.Content=
                }
            }
            catch (Exception exception)
            {
                ResponseModel.Message = exception.Message;
                ResponseModel.IsSuccess = false;
            }
            return eTemp;
        }



        /// <summary>
        /// To update EmailDetails from Indexer WebJob
        /// </summary>
        /// <param name="emailformat"></param>
        /// <returns></returns>
        public void UpdateEmailDetails(EmailFormat emailformat)
        {
            ResponseModel response = new ResponseModel();
            long userMembershipId = (from userMembership in dbcontext.UserMembership.Where(x => x.UserMembershipId == emailformat.UserMembershipId && x.IsActive == true && x.IsDeleted == false)
                                     select userMembership.UserMembershipId).FirstOrDefault();
            if (userMembershipId > 0)
            {
                var emailDetail = dbcontext.EmailDetails.Where(x => x.EmailDetailsId == emailformat.EmailDetailsId && x.EmailTempletId == emailformat.EmailTempletId).FirstOrDefault();
                // emailDetail=
                if (emailDetail != null)
                {

                    emailDetail.CreatedBy = userMembershipId;
                    emailDetail.ModifiedOn = DateTime.UtcNow;
                    emailDetail.MessageId = emailformat.MessageId;
                    emailDetail.IsBounce = emailformat.IsBounce;
                    emailDetail.EmailTo = emailformat.EmailTo;
                    emailDetail.Notification = emailformat.Notification;
                    emailDetail.InProcess = emailformat.InProcess;
                    emailDetail.BounceStatus = emailformat.BounceStatus;

                    dbcontext.SaveChanges();
                    response.IsSuccess = true;

                }
            }
            // return response;
        }


        /// <summary>
        /// Get list of Uploaded file by emailId
        /// </summary>
        /// <param name="emailId"></param>
        /// <returns>ResponseModel</returns>
        public ResponseModel GetFilesByEmailId(string emailId)
        {
            ResponseModel response = new ResponseModel();
            //List<EmailDetails> detail = new List<EmailDetails>();
            List<FileDetails> fileDetail = new List<FileDetails>();
            try
            {
                //detail = dbcontext.EmailDetails.Where(x => x.EmailFrom == emailId).ToList();
                //var detail = (from temp in dbcontext.EmailTemplets
                //              from details in dbcontext.EmailDetails
                //              where temp.EmailFrom == emailId && temp.EmailTempletId == details.EmailTempletId
                //              select new { temp, details }).ToList();


                //FileDetails result = new FileDetails();
                //result.EmailTempletId = detail.FirstOrDefault().temp.EmailTempletId;




                fileDetail = (from temp in dbcontext.EmailTemplets
                              where temp.EmailFrom == emailId
                              select new FileDetails()
                              {
                                  EmailTempletId = temp.EmailTempletId,
                                  FileName = temp.FileName,
                                  FilePath = temp.FilePath,
                                  Total = (from cs in dbcontext.EmailDetails
                                           where cs.EmailFrom == emailId && cs.EmailTempletId == temp.EmailTempletId
                                           select cs.FileName).Count(),
                                  HardBounce = (from cs in dbcontext.EmailDetails
                                                where cs.EmailFrom == emailId && cs.EmailTempletId == temp.EmailTempletId && cs.IsBounce == true
                                                && cs.MessageId != null && cs.BounceStatus == 1 //&& cs.Notification=="Sent"
                                                select cs.FileName).Count(),
                                  SoftBounce = (from cs in dbcontext.EmailDetails
                                                where cs.EmailFrom == emailId && cs.EmailTempletId == temp.EmailTempletId && cs.IsBounce == true &&
                                                cs.MessageId == null && cs.BounceStatus == 2
                                                select cs.FileName).Count(),
                                  Sent = (from cs in dbcontext.EmailDetails
                                          where cs.EmailFrom == emailId && cs.EmailTempletId == temp.EmailTempletId && cs.IsBounce == false
                                          && cs.Notification == "Sent" && cs.BounceStatus == 0 && cs.MessageId != null
                                          select cs.FileName).Count(),
                                  IsSoftBounce = ((from cs in dbcontext.EmailDetails
                                                   where cs.EmailFrom == emailId && cs.EmailTempletId == temp.EmailTempletId && cs.IsBounce == true &&
                                                 cs.MessageId == null && cs.BounceStatus == 2
                                                   select cs.FileName).Count() > 0),
                                  InProcess = ((from cs in dbcontext.EmailDetails
                                                where cs.EmailFrom == emailId && cs.EmailTempletId == temp.EmailTempletId
                                                && cs.InProcess == true
                                                select cs.FileName).Count() > 0),
                                  CreatedOn = temp.CreatedOn

                              }
                             ).Distinct().OrderByDescending(x => x.EmailTempletId).ToList();

                response.Content = fileDetail;
                response.IsSuccess = true;
            }
            catch (Exception e)
            {
                response.Message = e.Message;
                return response;
            }
            return response;
        }


        /// <summary>
        /// Get list of emails(bounced/sent) by from a filelist by emailDetailId
        /// </summary>
        /// <param name="emailId"></param>
        /// <returns>ResponseModel</returns>
        public List<EmailFormat> GetEmailByTempletId(long emailTempletId, bool isBounce)
        {
            //ResponseModel response = new ResponseModel();
            //List<EmailDetails> detail = new List<EmailDetails>();
            List<EmailFormat> EmailList = new List<EmailFormat>();
            try
            {
                //detail = dbcontext.EmailDetails.Where(x => x.EmailFrom == emailId).ToList();
                EmailList = (from ed in dbcontext.EmailDetails
                             from temp in dbcontext.EmailTemplets
                             where ed.EmailTempletId == emailTempletId && ed.EmailTempletId == temp.EmailTempletId && ed.IsBounce == isBounce
                             select new EmailFormat()
                             {
                                 EmailDetailsId = ed.EmailDetailsId,
                                 FirstName = ed.FirstName,
                                 EmailSubject = temp.EmailSubject,
                                 EmailTo = ed.EmailTo,
                                 IsBounce = ed.IsBounce


                             }
                             ).Distinct().ToList();

                //response.Content = EmailList;
                //response.IsSuccess = true;
            }
            catch (Exception e)
            {
                //response.Message = e.Message;
                return EmailList;
            }
            return EmailList;
        }

        public List<MessageText> GetMessageText(long emailTempletId)
        {
            List<MessageText> email = new List<MessageText>();
            try
            {
                email = (from ed in dbcontext.EmailDetails
                         from temp in dbcontext.EmailTemplets
                         where ed.EmailTempletId == emailTempletId && ed.EmailTempletId == temp.EmailTempletId && ed.BounceStatus == 2
                         select new MessageText()
                         {
                             msgtxt = ed.EmailDetailsId + "akjshdyt45lkh" + ed.EmailTempletId +
                            "akjshdyt45lkh" + ed.UserMembershipId + "akjshdyt45lkh" +
                            ed.EmailFrom + "akjshdyt45lkh" + ed.EmailTo + "akjshdyt45lkh" +
                            temp.EmailSubject + "akjshdyt45lkh" + temp.EmailBody + "akjshdyt45lkh" + ed.FirstName


                         }).ToList();
                return email;
            }
            catch (Exception e)
            {
                return email;
            }

        }

        public async Task ChangeStatusOfProcess(long emailDetailsId)
        {
            var emailDetail = dbcontext.EmailDetails.Where(x => x.EmailDetailsId == emailDetailsId).FirstOrDefault();
            // emailDetail=
            if (emailDetail != null)
            {
                emailDetail.InProcess = true;
                dbcontext.SaveChanges();
            }

        }
        public bool GetProcessStatus(long emailDetailsId)
        {

            var InProcess = ((from cs in dbcontext.EmailDetails
                              where cs.EmailTempletId == emailDetailsId
                              && cs.InProcess == true
                              select cs.FileName).Count() > 0);
            if (InProcess)
            {
                return true;

            }
            else
            {
                return false;
            }
        }

        public ResponseModel SaveContact(ContactView contact)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var user = (from userMembership in dbcontext.UserMembership.Where(x => x.Email == contact.Me && x.IsActive == true && x.IsDeleted == false)
                            select userMembership).FirstOrDefault();
                //var user1 = dbcontext.UserMembership.Where(x => x.Email == contact.Me).FirstOrDefault();
                if (user.UserMembershipId > 0)
                {
                    var Contact = dbcontext.Contacts.Where(x => x.ContactId == contact.ContactId && x.UserMembershipId == contact.UserMembershipId).FirstOrDefault();
                    if (Contact != null)// If already exist
                    {
                        Contact.CreatedBy = 1;
                        Contact.ModifiedOn = DateTime.UtcNow;
                        Contact.Name = contact.Name;
                        Contact.Company = contact.Company;
                        Contact.Email = contact.Email;
                        Contact.JobTittle = contact.JobTittle;
                        Contact.PhoneNo = contact.PhoneNo;
                        Contact.Notes = contact.Notes;
                        Contact.Address = contact.Address;
                        Contact.Website = contact.Website;
                        Contact.InternetCall = contact.InternetCall;
                        Contact.IM = contact.IM;
                        Contact.UserMembershipId = user.UserMembershipId;

                        dbcontext.SaveChanges();
                        response.IsSuccess = true;
                        response.Message = "Record Updated Successfully!";

                    }
                    else// new contact for user.
                    {
                        var temp = (new Contact()
                        {
                            CreatedBy = 1,
                            CreatedOn = DateTime.UtcNow,
                            UserMembershipId = user.UserMembershipId,
                            Name = contact.Name,
                            Company = contact.Company,
                            Email = contact.Email,
                            JobTittle = contact.JobTittle,
                            PhoneNo = contact.PhoneNo,
                            Notes = contact.Notes,
                            Address = contact.Address,
                            Website = contact.Website,
                            InternetCall = contact.InternetCall,
                            IM = contact.IM

                        });

                        dbcontext.Contacts.Add(temp);
                        dbcontext.SaveChanges();
                        response.IsSuccess = true;
                        response.Message = "New Record Added Successfully!";
                    }

                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }



        public ResponseModel GetContactDetails(string emailId, string userMailId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                long userMembershipId = (from userMembership in dbcontext.UserMembership.Where(x => x.Email == userMailId && x.IsActive == true && x.IsDeleted == false)
                                         select userMembership.UserMembershipId).FirstOrDefault();
                if (userMembershipId > 0)
                {
                    var ContactDetail = dbcontext.Contacts.Where(x => x.Email == emailId && x.UserMembershipId == userMembershipId).FirstOrDefault();

                    if (ContactDetail != null)
                    {
                        response.IsSuccess = true;
                        response.Message = "Old Contact";
                        response.Content = ContactDetail;
                    }
                    else
                    {
                        response.IsSuccess = true;
                        response.Message = "New Contact";
                        response.Content = null;
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid Request.";
                    response.Content = null;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Something went wrong!";
                response.Content = "Message: " + ex.Message + " InnerException: " + ex.InnerException;
            }
            return response;

        }
        public ResponseModel GetContactList(string emailId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                long userMembershipId = (from userMembership in dbcontext.UserMembership.Where(x => x.Email == emailId && x.IsActive == true && x.IsDeleted == false)
                                         select userMembership.UserMembershipId).FirstOrDefault();

                if (userMembershipId > 0)
                {
                    var Contact = (from item in dbcontext.Contacts
                                   where item.UserMembershipId == userMembershipId
                                   select new
                                   {
                                       ContactId = item.ContactId,
                                       Name = item.Name,
                                       Email = item.Email,
                                       PhoneNo = item.PhoneNo

                                   }).ToList();

                    if (Contact.Count > 0)
                    {
                        response.IsSuccess = true;
                        response.Message = "List";
                        response.Content = Contact;

                    }
                    else
                    {
                        response.IsSuccess = true;
                        response.Message = "No Contact found.";
                        response.Content = null;
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid Request.";
                    response.Content = null;
                }

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Something went wrong!";
                response.Content = ex.Message;
            }
            return response;

        }

        public ExportData ListOfStatus(long emailTempletId)
        {
            ExportData exportData = new ExportData();
            exportData.RecipientStatusList = new List<ExportFile>();
            //List<ExportFile> emailstatus = new List<ExportFile>();
            try
            {
                exportData.RecipientStatusList = (from file in dbcontext.EmailDetails.Where(x => x.EmailTempletId == emailTempletId)
                                                  join contact in dbcontext.Contacts on file.EmailTo equals contact.Email into ps
                                                  from contact in ps.DefaultIfEmpty()
                                                  select new ExportFile()
                                                  {
                                                      EmployeCode = file.EmployeCode.ToString(),
                                                      Recipient = file.EmailTo,
                                                      Status = file.BounceStatus == 1 ? "Hard Bounced" : (file.BounceStatus == 2 ? "Soft Bounce" : "Sent"),
                                                      ModifiedOn = file.ModifiedOn,
                                                      PhoneNo = (contact.PhoneNo ?? string.Empty).ToString(),
                                                  }).ToList();


                var EmailTempleteData = dbcontext.EmailTemplets.Where(x => x.EmailTempletId == emailTempletId).FirstOrDefault();
                exportData.Body = EmailTempleteData.EmailBody;
                exportData.Subject = EmailTempleteData.EmailSubject;
                exportData.Sender = EmailTempleteData.EmailFrom;
                exportData.FileName = EmailTempleteData.FileName;
                //exportData.PhoneNo = EmailTempleteData.PhoneNo;

            }
            catch (Exception e)
            {

            }


            return exportData;
        }

        public ResponseModel GetContactListToExport(string emailId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var datetimeBefore24hrs = DateTime.UtcNow.AddHours(-24);
                long userMembershipId = (from userMembership in dbcontext.UserMembership.Where(x => x.Email == emailId && x.IsActive == true && x.IsDeleted == false)
                                         select userMembership.UserMembershipId).FirstOrDefault();

                if (userMembershipId > 0)
                {
                    var Contact = (from item in dbcontext.Contacts
                                   where item.UserMembershipId == userMembershipId && item.CreatedOn >= datetimeBefore24hrs
                                   select new
                                   {
                                       ContactId = item.ContactId,
                                       Name = item.Name,
                                       Email = item.Email,
                                       PhoneNo = item.PhoneNo,
                                       Company = item.Company,
                                       Address = item.Address,
                                       Website = item.Website,
                                       InternetCall = item.InternetCall,
                                       IM = item.IM,
                                       JobTittle = item.JobTittle,
                                       Notes = item.Notes,
                                       CreatedOn = item.CreatedOn
                                   }).ToList();

                    if (Contact.Count > 0)
                    {
                        response.IsSuccess = true;
                        response.Message = "List";
                        response.Content = Contact;

                    }
                    else
                    {
                        response.IsSuccess = true;
                        response.Message = "No Contact found.";
                        response.Content = null;
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid Request.";
                    response.Content = null;
                }

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Something went wrong!";
                response.Content = ex.Message;
            }
            return response;

        }

        public MessageText ReProcessBouncedMail(long emailDetailsId)
        {
            try
            {
                var emailDetails = dbcontext.EmailDetails.Where(x => x.EmailDetailsId == emailDetailsId).Select(x => x).FirstOrDefault();
                int noOfAttemp = Convert.ToInt32(emailDetails.NoOfAttemp);
                if (noOfAttemp <= 3)
                {
                    emailDetails.NoOfAttemp = emailDetails.NoOfAttemp + 1;
                    dbcontext.SaveChanges();

                    MessageText email = (from ed in dbcontext.EmailDetails
                                         join temp in dbcontext.EmailTemplets on ed.EmailTempletId equals temp.EmailTempletId
                                         where ed.EmailDetailsId == emailDetailsId
                                         select new MessageText()
                                         {
                                             msgtxt = ed.EmailDetailsId + "akjshdyt45lkh" + ed.EmailTempletId +
                                            "akjshdyt45lkh" + ed.UserMembershipId + "akjshdyt45lkh" +
                                            ed.EmailFrom + "akjshdyt45lkh" + ed.EmailTo + "akjshdyt45lkh" +
                                            temp.EmailSubject + "akjshdyt45lkh" + temp.EmailBody + "akjshdyt45lkh" + ed.FirstName
                                         }).FirstOrDefault();
                    return email;
                }
                else
                    return null;

            }
            catch (Exception e)
            {
                return null;
            }
        }

    }
}


