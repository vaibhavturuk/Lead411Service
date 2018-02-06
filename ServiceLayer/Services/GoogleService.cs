using CoreEntities.CustomModels;
using CoreEntities.enums;
using CoreEntities.Helper;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Oauth2.v2.Data;
using Google.Contacts;
using ServiceLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RepositoryLayer.Repositories.Interfaces;
using System.Data;
using Google.Apis.Gmail.v1.Data;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.IO;
using CoreEntities.Domain;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace ServiceLayer.Services
{
    public class GoogleService : IGoogleService
    {
        private readonly IAccountRespsitory _iAccount;
        private readonly object mailerDbClass;

        public GoogleService(
            IAuthentication authentication,
            IMailProcess mailProcess,
            IOperation operation,
            IAccountRespsitory iAccount
            )
        {
            Authentication = authentication;
            MailProcess = mailProcess;
            Operation = operation;
            _iAccount = iAccount;
        }

        public IAuthentication Authentication { get; }
        public IMailProcess MailProcess { get; }
        public IOperation Operation { get; }
        public int NoOfAttemp { get; private set; }


        /// <summary>
        /// Get User Details by pooja
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public ResponseModel GetUserInfoByToken(string accessToken)
        {
            ResponseModel response = new ResponseModel { IsSuccess = false, Content = null };
            try
            {
                if (Authentication.IsTokenExpired(accessToken))
                {
                    //getting refresh Token 
                    var authDetail = _iAccount.GetRefreshTokenByAccessToken(accessToken);

                    //Passing RefreshToken to get AccessToken
                    var googleValidationResponse = (GoogleValidationResponse)Authentication.GetAccessTokenByRefreshToken(authDetail.RefreshToken, RequestType.web);
                    if (!string.IsNullOrEmpty(googleValidationResponse.access_token))
                    {
                        //Saving New AccessToken
                        _iAccount.SaveAccessToken(googleValidationResponse.access_token, authDetail.UserMembershipId);

                        //Get the UserDetails for New Access Token
                        var User = Authentication.GetUserProfile(googleValidationResponse.access_token);
                        if (User != null)
                        {

                            UserDetail userDetail = new UserDetail();
                            userDetail = (UserDetail)User;
                            //userDetail.family_name = null;
                            //userDetail.given_name = null;
                            //userDetail.id = null;
                            //userDetail.locale = null;
                            userDetail.accessToken = googleValidationResponse.access_token;
                            response.Content = userDetail;
                            response.IsSuccess = true;
                            response.Message = "Success";
                            response.AccessToken= googleValidationResponse.access_token;
                        }
                    }
                }
                else
                {
                    var User = Authentication.GetUserProfile(accessToken);
                    if (User != null)
                    {
                        UserDetail userDetail = new UserDetail();
                        userDetail = (UserDetail)User;
                        userDetail.accessToken = accessToken;
                        response.Content = userDetail;
                        response.IsSuccess = true;
                        response.Message = "Success";
                        response.AccessToken = accessToken;

                    }
                }

            }
            catch (Exception excep)
            {
                response.IsSuccess = false;
                response.Message = "Error Occured! Message: " + excep.Message;
            }
            return response;
        }
        
        

        /// <summary>
        /// Google authentication
        /// </summary>
        /// <param name="authorizedCode"></param>
        /// <param name="accountType"></param>
        /// <param name="deviceInfo"></param>
        /// <param name="reqType"></param>
        /// <returns></returns>
        public ResponseModel UserAuthentication(
            string authorizedCode,
            AccountType accountType,
            DeviceInfo deviceInfo,
            RequestType reqType)
        {
            ResponseModel response = new ResponseModel { IsSuccess = false, Content = null };
            //Validation start
            if (string.IsNullOrWhiteSpace(authorizedCode))
            {
                response.Message = Messages.RequiredField + " authorizedCode";
                return response;
            }
            else if (string.IsNullOrWhiteSpace(accountType.ToString()))
            {
                response.Message = Messages.RequiredField + " accountType";
                return response;
            }
            else if (deviceInfo == null)
            { 
                response.Message = Messages.RequiredField + " deviceInfo";
                return response;
            }

            try
            {
                //Validate user and get access and refresh token : 1st time user
                var gValidationResponse = (GoogleValidationResponse)Authentication.GetAccessTokenByAuthCode(authorizedCode, reqType);
                if (string.IsNullOrWhiteSpace(gValidationResponse.access_token))
                {
                    response.Content = null;
                    response.Message = "Invalid code.Unable to get refresh token and access token";
                    response.IsSuccess = false;
                    return response;
                }

                var tokeninfo = (Tokeninfo)Authentication.GetTokenInfo(gValidationResponse.access_token);
                if (tokeninfo.ExpiresIn <= 0)
                {
                    response.Content = null;
                    response.Message = "Invalid code / Token expired";
                    response.IsSuccess = false;
                    return response;
                }

                //If user is comming next time then process will take refresh token from db
                if (string.IsNullOrWhiteSpace(gValidationResponse.refresh_token))
                {
                    gValidationResponse.refresh_token = _iAccount.GetRefreshToken(accountType, deviceInfo, tokeninfo.Email);
                }

                //Create new user account
                //Comment For Azure
                var successResponseModel = _iAccount.AddUpdateUserInformation(gValidationResponse.access_token, accountType, (UserDetail)Authentication.GetUserProfile(gValidationResponse.access_token), deviceInfo, reqType);// need to remove GoogleGetUserInfo
                #region comment need                    
                //Save refresh token in db
                //_iAccount.SaveRefreshToken(gValidationResponse.refresh_token, gValidationResponse.access_token,                     tokeninfo.Email, accountType);

                //Authentication.CreateAutherizationServiceObject(tokeninfo.Scope, tokeninfo.ExpiresIn, tokeninfo.UserId, gValidationResponse.access_token, gValidationResponse.refresh_token);
                #endregion
                //Save refresh token in db
                //Comment for Azure
                _iAccount.SaveRefreshToken(gValidationResponse.refresh_token, gValidationResponse.access_token, tokeninfo.Email, accountType);

                successResponseModel.AccessToken = gValidationResponse.access_token;
                //successResponseModel.RefreshToken = gValidationResponse.refresh_token;
                response.Content = successResponseModel;
                response.AccessToken= gValidationResponse.access_token;
                //ends///////
                //response.Content = " accessToken: " + gValidationResponse.access_token; 
                response.IsSuccess = true;
                response.Message = Messages.ProcessCompeleted;

            }
            catch (Exception ex)
            {
                response.Content = "";
                response.IsSuccess = false;
                response.Message = Messages.InvalidToken + " Exception:" + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Get user information by userid and access token. (After login when user start application next time it checks existance with this method.
        /// </summary>
        /// <param name="Lead411UserInfo"></param>
        /// <param name="reqType"></param>
        /// <returns></returns>
        public Lead411UserInfo GetUserInfoByUserId(
            Lead411UserInfo Lead411UserInfo,
            RequestType reqType)
        {
            if (Lead411UserInfo.Provider == AccountType.Gmail.ToString() && Authentication.IsTokenExpired(Lead411UserInfo.AccessToken))
            {
                var googleValidationResponse = (GoogleValidationResponse)Authentication.GetAccessTokenByRefreshToken(Lead411UserInfo.RefreshToken, reqType);
                if (!string.IsNullOrEmpty(googleValidationResponse.access_token))
                {
                    Lead411UserInfo.AccessToken = googleValidationResponse.access_token;
                    _iAccount.SaveRefreshToken(Lead411UserInfo.RefreshToken, Lead411UserInfo.AccessToken, Lead411UserInfo.Email, AccountType.Gmail);

                }
                else
                {
                    Lead411UserInfo = null;
                } 
            }

            return Lead411UserInfo;
        }


        /// <summary>
        /// Process single mail on the call of web jobs.
        /// </summary>
        /// <param name="emailId"></param>
        public async Task ProcessSingleEmail(string emailId)
        {
            try
            {
                Lead411UserInfo Lead411UserInfo = _iAccount.GetUserSessionEmailWise(emailId);

                if (Lead411UserInfo != null)
                {
                    var reqType = _iAccount.GetRequestType(emailId);

                    if (Lead411UserInfo.Provider == AccountType.Gmail.ToString())
                    {
                        var googleValidationResponse = new GoogleValidationResponse();
                        //Get new access token
                        if (Authentication.IsTokenExpired(Lead411UserInfo.AccessToken))
                        {
                            googleValidationResponse =
                                (GoogleValidationResponse)
                                Authentication.GetAccessTokenByRefreshToken(Lead411UserInfo.RefreshToken, reqType);
                            //Save new access token to db
                            _iAccount.SaveRefreshToken(Lead411UserInfo.RefreshToken, Lead411UserInfo.AccessToken,
                                Lead411UserInfo.Email, AccountType.Gmail);
                        }
                        else
                        {
                            googleValidationResponse.access_token = Lead411UserInfo.AccessToken;
                        }
                        if (googleValidationResponse.access_token != null)
                        {
                            Lead411UserInfo.AccessToken = googleValidationResponse.access_token;

                            //Get Userinfo
                            var tokeninfo = (Tokeninfo)Authentication.GetTokenInfo(Lead411UserInfo.AccessToken);

                            //Process emails
                            var gmailService =
                                (GmailService)
                                Authentication.CreateAutherizationServiceObject(tokeninfo.Scope, tokeninfo.ExpiresIn,
                                    tokeninfo.UserId, Lead411UserInfo.AccessToken, Lead411UserInfo.RefreshToken);

                            //Get lable to process the mails
                            ////List<string> labels = await Operation.GetLabels(gmailService, tokeninfo.UserId);

                            //Process mails

                            var userCredential =
                                (UserCredential)
                                Authentication.CreateAutherizationObject(tokeninfo.Scope, tokeninfo.ExpiresIn,
                                    tokeninfo.UserId, Lead411UserInfo.AccessToken, Lead411UserInfo.RefreshToken);

                            var contactRequest =
                                (ContactsRequest)
                                Authentication.ContactsRequest(userCredential.Token.AccessToken,
                                    userCredential.Token.RefreshToken);

                            var contactList = (List<Google.Contacts.Contact>)await Operation.GetContacts(contactRequest);

                            var f = contactRequest.GetContacts();
                            List<string> list = new List<string>();
                            foreach (Google.Contacts.Contact t in f.Entries)
                            {
                                foreach (var email in t.Emails)
                                {
                                    list.Add(email.Address.ToString());
                                }
                            }
                            //await MailProcess.Process(gmailService, tokeninfo.UserId, labels,
                            //    Lead411UserInfo.UserMembershipId, userCredential, contactList,
                            //    contactRequest);
                        }
                        else
                        {
                            //Disable account
                            _iAccount.DisableAccount(Lead411UserInfo.UserMembershipId);
                            //Send mail for relogin
                            EmailHelper.SendEmail(Lead411UserInfo.Email,
                                "Your account is inactive, Please login to Lead411 to activate it.",
                                "Lead411: Inactive Notification");
                        }
                    }

                }
                else
                {

                }
                //return true;
            }
            catch (Exception)
            {
                // return false;
            }
        }

        /// <summary>
        /// Delete contact group created by dot align project.
        /// </summary>
        /// <param name="Lead411UserInfo"></param>
        /// <param name="reqType"></param>
        public async Task<bool> DeleteContactFolder(Lead411UserInfo Lead411UserInfo, RequestType reqType)
        {
            bool result = false;
            // Retrieving the contact group is required in order to get the Etag.
            try
            {
                GoogleValidationResponse googleValidationResponse = new GoogleValidationResponse();
                //Get new access token
                if (Authentication.IsTokenExpired(Lead411UserInfo.AccessToken))
                {
                    googleValidationResponse = (GoogleValidationResponse)Authentication.GetAccessTokenByRefreshToken(Lead411UserInfo.RefreshToken, reqType);
                    //Save new access token to db
                    _iAccount.SaveRefreshToken(Lead411UserInfo.RefreshToken, Lead411UserInfo.AccessToken, Lead411UserInfo.Email, AccountType.Gmail);
                }
                else
                {
                    googleValidationResponse.access_token = Lead411UserInfo.AccessToken;
                }
                if (googleValidationResponse.access_token != null)
                {
                    Lead411UserInfo.AccessToken = googleValidationResponse.access_token;
                    //Get Userinfo
                    Tokeninfo tokeninfo = (Tokeninfo)Authentication.GetUserProfile(Lead411UserInfo.AccessToken);

                    UserCredential userCredential = (UserCredential)Authentication.CreateAutherizationServiceObject(tokeninfo.Scope, tokeninfo.ExpiresIn, tokeninfo.UserId, Lead411UserInfo.AccessToken, Lead411UserInfo.RefreshToken);

                    ContactsRequest contactRequest = (ContactsRequest)Authentication.ContactsRequest(Lead411UserInfo.AccessToken, Lead411UserInfo.RefreshToken);

                    result = await Operation.DeleteContactFolder(contactRequest);
                }
            }
            catch //(GDataVersionConflictException e)
            {
                // Etags mismatch: handle the exception.
            }
            return result;
        }

        /// <summary>
        /// send mail directly from API.
        /// </summary>
        /// <param name="emailList"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <returns></returns>
       
        public string Base64UrlEncode(string input)
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(inputBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        public EmailFormat SplitMsgText(string msgText)
        {
            EmailFormat emailformat = new EmailFormat();
            #region--get email detail(split)

            string[] MailDetails = msgText.Split(new string[] { "akjshdyt45lkh" }, StringSplitOptions.None);

            emailformat.EmailDetailsId = Convert.ToInt64(MailDetails[0].ToString());
            emailformat.EmailTempletId = Convert.ToInt64(MailDetails[1].ToString());
            emailformat.UserMembershipId = Convert.ToInt64(MailDetails[2].ToString());
            emailformat.EmailFrom = MailDetails[3].ToString();
            emailformat.EmailTo= MailDetails[4].ToString();
            emailformat.EmailSubject = MailDetails[5].ToString();
            emailformat.EmailBody = MailDetails[6].ToString();
            emailformat.FirstName = MailDetails[7].ToString();
            #endregion
            emailformat.InProcess = true;

            return emailformat;
        }

        public GmailService GetGmailServiceObject(string emailId)
        {
            GmailService gmailService = new GmailService();
            Lead411UserInfo Lead411UserInfo = _iAccount.GetUserSessionEmailWise(emailId);
            if (Lead411UserInfo != null)
            {
                var reqType = _iAccount.GetRequestType(emailId);

                if (Lead411UserInfo.Provider == AccountType.Gmail.ToString())
                {
                    var googleValidationResponse = new GoogleValidationResponse();
                    //Get new access token
                    if (Authentication.IsTokenExpired(Lead411UserInfo.AccessToken))
                    {
                        googleValidationResponse =
                            (GoogleValidationResponse)
                            Authentication.GetAccessTokenByRefreshToken(Lead411UserInfo.RefreshToken, reqType);
                        //Save new access token to db
                        _iAccount.SaveRefreshToken(Lead411UserInfo.RefreshToken, Lead411UserInfo.AccessToken,
                            Lead411UserInfo.Email, AccountType.Gmail);
                    }
                    else
                    {
                        googleValidationResponse.access_token = Lead411UserInfo.AccessToken;
                    }
                    if (googleValidationResponse.access_token != null)
                    {
                        Lead411UserInfo.AccessToken = googleValidationResponse.access_token;

                        //Get Userinfo
                        var tokeninfo = (Tokeninfo)Authentication.GetTokenInfo(Lead411UserInfo.AccessToken);

                        //Process emails

                        gmailService =
                            (GmailService)
                            Authentication.CreateAutherizationServiceObject(tokeninfo.Scope, tokeninfo.ExpiresIn,
                                tokeninfo.UserId, Lead411UserInfo.AccessToken, Lead411UserInfo.RefreshToken);

                        //Process mails

                        var userCredential =
                            (UserCredential)
                            Authentication.CreateAutherizationObject(tokeninfo.Scope, tokeninfo.ExpiresIn,
                                tokeninfo.UserId, Lead411UserInfo.AccessToken, Lead411UserInfo.RefreshToken);


                    }
                }
            }
            return gmailService;
        }

        public ResponseModel CheckBounceMail(EmailFormat emailformat, GmailService gmailService)
        {
            ResponseModel response = new ResponseModel();
            CommonService cs;
            EmailDetails obj = new EmailDetails();

             //var obj = new EmailDetails();
            int Attemps = 0;
           

            #region--Check bounce mail or not
            emailformat.IsBounce = false;
            emailformat.Notification = "Processing";
            var messageId = emailformat.MessageId;
            var details = gmailService.Users.Threads.Get("me", messageId).Execute();
            response.Content = details;
            var msgCount = details.Messages.Count;
            if (details.Id == messageId)
            {
                foreach (var item in details.Messages)
                {
                    if (item.LabelIds.Count >= 2 && item.Id != messageId)
                    {
                        var headers = item.Payload.Headers;
                        foreach (var header in headers)
                        {
                            if (header.Name == "From")
                            {
                                if (header.Value == "Mail Delivery Subsystem <mailer-daemon@googlemail.com>" || header.Value.Contains("postmaster@"))// Added by Pooja On 23 nov 2017 for Hadbounce check '|| header.Value.Contains("postmaster@")'
                                {
                                    response.Message = "Bounced";
                                    #region--Save the messageId , IsBounce and Notification to DB
                                    emailformat.IsBounce = true;
                                    emailformat.InProcess = false;
                                    emailformat.BounceStatus = 1;//HARD BOUNCE
                                    emailformat.Notification = response.Message;
                                    #endregion
                                    #region--resend the soft bounce email
                                    ////Kartik/// New queue method
                                    ////1) How many times email (emailformat) has been resend
                                    ////2) If count is > 2 then no process
                                    ////3) If count is <= 2 then add into the queue again.

                                    //ReProcessBouncedMail(emailformat.EmailDetailsId);
                                    
                                    #endregion
                                }
                                else
                                {
                                    response.Message = "Sent";
                                    emailformat.IsBounce = false;
                                    emailformat.InProcess = false;
                                    emailformat.BounceStatus = 0;// SENT
                                    emailformat.Notification = response.Message;
                                    //call method for adding/deleteing Gmail Contact
                                }
                            }
                        }
                    }
                    else
                    {
                        response.Message = "Sent";
                        emailformat.IsBounce = false;
                        emailformat.InProcess = false;
                        emailformat.BounceStatus = 0;//SENT
                        emailformat.Notification = response.Message;
                        //call method for adding/deleteing Gmail Contact
                        //
                    }

                }
                //call save method here
                _iAccount.UpdateEmailDetails(emailformat);


                

            }
            else
            {
                response.Message = "Email Not register with us.";
            }
            #endregion
            response.Content = emailformat;
            return response;

        }

       
        public async Task ProcessMail(string msgText)
        {
            ResponseModel response = new ResponseModel();
            EmailFormat emailformat = new EmailFormat();
            //split the msgtext
            emailformat = SplitMsgText(msgText);
            
            var deviceInfo = new DeviceInfo() { Model = "Release", Platform = "web", Uuid = "987609890" };
            var emailId = emailformat.EmailFrom; // emailList[1].EmailFrom.ToString();

            #region--for creating service object

            try
            {
                GmailService gmailService = GetGmailServiceObject(emailId);
                if (gmailService != null)
                {
                    #region--send mail
                        string plainText = "To:" + emailformat.EmailTo + "\r\n" +
                                            "Subject:" + emailformat.EmailSubject + "\r\n" +
                                            "Content-Type: text/html; charset=us-ascii\r\n\r\n" +
                                            emailformat.EmailBody;
                        var newMsg = new Google.Apis.Gmail.v1.Data.Message();
                        newMsg.Raw = Base64UrlEncode(plainText.ToString());
                        var sendEmailResponce = gmailService.Users.Messages.Send(newMsg, "me").Execute();
                        var messageId = sendEmailResponce.ThreadId;
                        //loop end
                        response.IsSuccess = true;
                        emailformat.MessageId= sendEmailResponce.ThreadId;

                     #endregion

                    //delay for 1 min
                    await Task.Delay(60000);

                    //call checkMailSendOrNot
                    response = CheckBounceMail(emailformat, gmailService);

                }
                else
                {
                    EmailHelper.SendEmail(emailformat.EmailTo,
                        "Your account is inactive, Please login to Lead411 to activate it.",
                        "Lead411: Inactive Notification");
                }
               
            }
           
            catch (Exception ex)
            {
                try
                {
                    ////Kartik/// New queue method
                    ////1) How many times email (emailformat) has been resend?
                    ////2) If count is > 2 then no process
                    ////3) If count is <= 2 then add into the queue again.

                    ReProcessBouncedMail(emailformat.EmailDetailsId);

                    response.Message = ex.Message;
                    response.Message = "Reprocessed!";
                    emailformat.InProcess = false;
                    emailformat.IsBounce = true;
                    emailformat.BounceStatus = 2;//SOFT BOUNCE
                    emailformat.Notification = ex.HResult + " " +  ex.Message;
                    _iAccount.UpdateEmailDetails(emailformat);

                    
                }
                catch (Exception exc)
                {
                    emailformat.Notification = "InnerException:" + exc.Message;
                    _iAccount.UpdateEmailDetails(emailformat);
                }
                

            }
            #endregion


        }

        public void ReProcessBouncedMail(long emailDetailsId)
        {
            MessageText messageText = _iAccount.ReProcessBouncedMail(emailDetailsId);
            if (messageText != null)
            {
                Task.Run(() => PushMailInQueue(new List<MessageText>() { new MessageText() { msgtxt = messageText.msgtxt } }));
            }
        }

        /// <summary>
        /// Push the List of to Azure storage Queue
        /// </summary>
        /// <param name="mailList">List<string></param>
        public async Task PushMailInQueue(List<MessageText> mailList)
        {
            ResponseModel response = new ResponseModel();

            if (mailList != null)
            {
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                    System.Configuration.ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);

                foreach (var email in mailList)
                {
                    // Create the queue client.
                    CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

                    if (queueClient != null)
                    {
                        CloudQueue queue = queueClient.GetQueueReference("gmailqueue");
                        //CloudQueue queue = queueClient.GetQueueReference("testqueue");

                        // Create the queue if it doesn't already exist.
                        queue.CreateIfNotExists();

                        // Create a message and add it to the queue.
                        CloudQueueMessage msg = new CloudQueueMessage(email.msgtxt);
                        queue.AddMessage(msg);
                    }
                }


                response.Message = "Following mail will be processing on the Queue";
            }
            else
            {
                response.Message = "No emails found to process";
            }
            //return response;
        }


        public bool CheckTokenExpired(
          string accessToken)
        {
            ResponseModel response = new ResponseModel();
            if (Authentication.IsTokenExpired(accessToken))
            {
                return true;
                response.IsSuccess = false;
                response.Message = "Session Time out! Please login again.";
                response.Content = accessToken;
            }
            else
            {
                return false;
                response.IsSuccess = true;
                response.Message = "User Authenticated!";
                response.Content = accessToken;
            }
        }

        /// <summary>
        /// Get new Access Token When Old Expires from Refresh token
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns>ResponceModel</returns>
        public ResponseModel GetNewAccessToken(string accessToken)
        {
            ResponseModel response = new ResponseModel();

            if (Authentication.IsTokenExpired(accessToken))
            {
                //getting refresh Token 
                var authDetail = _iAccount.GetRefreshTokenByAccessToken(accessToken);

                //Passing RefreshToken to get AccessToken
                var googleValidationResponse = (GoogleValidationResponse)Authentication.GetAccessTokenByRefreshToken(authDetail.RefreshToken, RequestType.web);
                if (!string.IsNullOrEmpty(googleValidationResponse.access_token))
                {
                    //Saving New AccessToken
                    _iAccount.SaveAccessToken(googleValidationResponse.access_token, authDetail.UserMembershipId);

                    response.Content = googleValidationResponse.access_token;
                    response.AccessToken= googleValidationResponse.access_token;// Added by Pooja On 23 Nov 2017 For Returning AccessToken with evry request
                    response.IsSuccess = true;
                    response.Message = "Success";

                }
               
            }

                return response;
        }
       
    }
}
