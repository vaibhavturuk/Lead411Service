using System;
using CoreEntities.CustomModels;
using CoreEntities.Domain;
using CoreEntities.enums;
using RestSharp;
using ServiceLayer.Helper;
using ServiceLayer.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using RepositoryLayer.Repositories.Interfaces;
using System.Data;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using ExcelExport = Microsoft.Office.Interop.Excel;
using System.ComponentModel;


namespace ServiceLayer.Services
{
    public class CommonService : ICommonService
    {
        private readonly IAccountRespsitory _iAccount;
        private readonly IGoogleService _iGoogleService;
        public CommonService(
            IAccountRespsitory iAccount,
            IGoogleService iGoogleService
            )
        {
            _iAccount = iAccount;
            _iGoogleService = iGoogleService;
        }

        /// <summary>
        /// Get user information by userid and access token. (After login when user start application next time it checks existance with this method.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="deviceInfo"></param>
        /// <param name="accessToken"></param>
        /// <param name="reqType"></param>
        /// <returns></returns>
        public Lead411UserInfo GetUserInfoByUserId(long userId, DeviceInfo deviceInfo, string accessToken, RequestType reqType)
        {
            return _iAccount.GetUserInfoByUserId(userId, deviceInfo, accessToken);
        }

        /// <summary>
        /// Set user active or inactive from front end.
        /// </summary>
        /// <param name="userId"></param>                           
        /// <param name="deviceInfo"></param>
        /// <returns></returns>
        public bool SetUserInActive(long userId, DeviceInfo deviceInfo)
        {
            return _iAccount.SetUserInActive(userId, deviceInfo);
        }

        /// <summary>
        /// Create mail batches to be process next time.
        /// </summary>
        /// <returns></returns>
        public List<Lead411UserInfo> GetNextMailToBeProcessed()
        {
            return _iAccount.GetNextMailToBeProcessed(CoreEntities.Helper.GeneralParameters.NumberOfConcurrentJobs);
        }

        /// <summary>
        /// Delete account from front end.
        /// </summary>
        /// <param name="userMembershipId"></param>
        /// <returns></returns>
        public bool DeleteAccount(long userMembershipId)
        {
            try
            {
                return _iAccount.DeleteAccount(userMembershipId);
            }
            catch (Exception)
            {
                return true;
            }
        }

        /// <summary>
        /// Get new session for the user on the basis of usermembership id
        /// </summary>
        /// <param name="userMembershipId"></param>
        /// <returns></returns>
        public Lead411UserInfo GetUserSessionUserMembershipWise(long userMembershipId)
        {
            try
            {
                Lead411UserInfo Lead411UserInfo = _iAccount.GetUserSessionUserMembershipWise(userMembershipId);

                return Lead411UserInfo;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Reset indexing from front end.
        /// </summary>
        /// <param name="userMembershipId"></param>
        /// <returns></returns>
        public bool ResetIndexing(long userMembershipId)
        {
            return _iAccount.ResetIndexing(userMembershipId);
        }

        public IRestResponse RestClientRequest(RequestObject requestObject)
        {
            RestClient restClient = new RestClient(requestObject.Url);
            var request = new RestRequest("", requestObject.RequestType) { RequestFormat = DataFormat.Json };
            if (requestObject.Parameters != null)
            {
                foreach (KeyValuePair<string, string> keyValuePair in requestObject.Parameters)
                {
                    request.AddParameter(keyValuePair.Key, keyValuePair.Value);
                }
            }

            if (requestObject.Headers != null)
            {
                foreach (KeyValuePair<string, string> keyValuePair in requestObject.Headers)
                {
                    request.AddHeader(keyValuePair.Key, keyValuePair.Value);
                }
            }
            return restClient.Execute(request);
        }

        /// <summary>
        /// Demo for above method
        /// </summary>
        public void CallMe()
        {
            RequestObject requestObject = new RequestObject();
            requestObject.Parameters.Add("dd", "dd");
            RestClientRequest(requestObject);
        }

        public AccountType GetAccountTypeByEmailId(string emailId)
        {
            return _iAccount.GetAccountTypeByEmailId(emailId);
        }

        /// <summary>
        /// Make the List out of EXL and the save it to database.
        /// </summary>
        /// <param name="subject">subject</param>
        /// <param name="body">body</param>
        /// <param name="res">res</param>
        /// <param name="fileName">fileName</param>
        /// <param name="emailFrom">emailFrom</param>
        /// <param name="filePath">filePath</param>
        /// <returns>ResponseModel</returns>
        public async Task<ResponseModel> PushMailInDB(string subject, string body, DataSet res, string fileName, string emailFrom, string filePath)
        {
            ResponseModel response = new ResponseModel();
            var a = res.Tables[0].Rows.Count;
            //list for send data to database
            List<EmailFormat> emailList = new List<EmailFormat>();
            var Url = "http://lead411chromeex.azurewebsites.net/FileUpload/";
            //var Url = "http://stagingwin.com/lead411/FileUpload/";
            //list for sending data to Queue
            List<MessageText> mailListForQueue = new List<MessageText>();

            var OnlyFileName = filePath;//filepath contains URL+fileName(with timestamp)
            for (int i = 0; i < a; i++)
            {
                //body = body.Replace("firstname", Convert.ToString(res.Tables[0].Rows[i]["Firstname"]));
                EmailFormat email = new EmailFormat();
                email.EmailFrom = emailFrom;
                email.EmailTo = Convert.ToString(res.Tables[0].Rows[i]["Email"]);
                email.EmailSubject = subject;
                //email.EmailBody = "<h3> Hi " + Convert.ToString(res.Tables[0].Rows[i]["Firstname"]) + ",</h3> <br>" + body;
                email.EmailBody = body;
                email.EmployeCode = Convert.ToString(res.Tables[0].Rows[i]["Employee Id"]);
                email.FirstName = Convert.ToString(res.Tables[0].Rows[i]["Firstname"]);
                email.IsBounce = false;
                email.UserMembershipId = 1;
                email.FileName = OnlyFileName;
                emailList.Add(email);
                //var mail = email.UserMembershipId + "akjshdyt45lkh" + email.EmailFrom + "akjshdyt45lkh" + email.EmailTo + "akjshdyt45lkh" + email.EmailSubject + "akjshdyt45lkh" + email.EmailBody;
                //mailcListForQueue.Add(mail);
            }
            #region Code to save in AzureDB and create list to push in Azure Queue
            //response = await AddUpdateEmailDetail(emailList);
            EmailTemplets emailTemplet = new EmailTemplets();
            emailTemplet = await _iAccount.AddEmailDetail(emailList, subject, body, fileName);

            if (emailTemplet.EmailDetails.Count > 0)
            {
                foreach (var email in emailTemplet.EmailDetails)
                {
                    MessageText txt = new MessageText();
                    var emailbody = emailTemplet.EmailBody;
                    emailbody = emailbody.Replace("firstname", email.FirstName);
                    txt.msgtxt = email.EmailDetailsId + "akjshdyt45lkh" + email.EmailTempletId + "akjshdyt45lkh" + email.UserMembershipId + "akjshdyt45lkh" + email.EmailFrom + "akjshdyt45lkh" + email.EmailTo + "akjshdyt45lkh" + emailTemplet.EmailSubject + "akjshdyt45lkh" + emailbody + "akjshdyt45lkh" + email.FirstName;
                    mailListForQueue.Add(txt);
                }

                #region code to Push in Queue
                Task.Run(() => PushMailInQueue(mailListForQueue));

                response.IsSuccess = true;
                response.Message = "record added Successfully";

                #endregion
            }
            else
            {
                response.IsSuccess = false;
                response.Message = "Sorry! Unable to save the records.";
            }
            #endregion
            return response;

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

        /// <summary>
        /// for encoding the mail to send
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string Base64UrlEncode(string input)
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(inputBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        /// <summary>
        /// Get List Of Files uploaded with the status of mail send and bounce 
        /// </summary>
        /// <param name="emailId">string</param>
        /// <returns>ResponseModel</returns>
        public ResponseModel GetListOfFile(string emailId)
        {
            ResponseModel response = new ResponseModel();
            List<FileDetails> fileDetails = new List<FileDetails>();
            response = _iAccount.GetFilesByEmailId(emailId);
            //response.Content = fileDetails;
            return response;
        }

        /// <summary>
        /// Get List Of email by the status of mail send and bounce
        /// </summary>
        /// <param name="emailTempletId"></param>
        /// <param name="isBounce"></param>
        /// <returns></returns>
        public List<EmailFormat> GetListOfEmail(long emailTempletId, bool isBounce)
        {
            //ResponseModel response = new ResponseModel();
            List<EmailFormat> EmailList = new List<EmailFormat>();

            EmailList = _iAccount.GetEmailByTempletId(emailTempletId, isBounce);
            return EmailList;
        }

        /// <summary>
        /// Get List Of email for retry
        /// </summary>
        /// <param name="emailTempletId"></param>
        /// <returns>List<MessageText></returns>
        public List<MessageText> GetMessageText(long emailTempletId)
        {
            //ResponseModel response = new ResponseModel();
            List<MessageText> msgtxt = new List<MessageText>();

            msgtxt = _iAccount.GetMessageText(emailTempletId);
            foreach (var email in msgtxt)
            {
                #region--get email detail(split)

                string[] MailDetails = email.msgtxt.Split(new string[] { "akjshdyt45lkh" }, StringSplitOptions.None);

                long EmailDetailsId = Convert.ToInt64(MailDetails[0].ToString());
                Task.Run(() => _iAccount.ChangeStatusOfProcess(EmailDetailsId));

                #endregion
            }
            return msgtxt;
        }

        /// <summary>
        /// Check send mail process status.
        /// </summary>
        /// <param name="emailDetailsId"></param>
        /// <returns></returns>
        public bool CheckProcessStatus(long emailDetailsId)
        {
            var InProcess = _iAccount.GetProcessStatus(emailDetailsId);
            if (InProcess)
            {
                return true;

            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Save and update contact details
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public ResponseModel SaveOrUpdateContact(ContactView contact)
        {
            ResponseModel response = new ResponseModel();
            _iAccount.SaveContact(contact);
            return response;
        }

        /// <summary>
        /// Save and update contact details
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public ResponseModel GetContactDetails(string emailId, string userMailId)
        {
            ResponseModel response = new ResponseModel();
            response = _iAccount.GetContactDetails(emailId, userMailId);
            return response;
        }

        public ResponseModel GetContactList(string emailId)
        {
            ResponseModel response = new ResponseModel();
            response = _iAccount.GetContactList(emailId);
            return response;
        }

        /// <summary>
        /// this method return the dataset and filename from templetId
        /// </summary>
        /// <param name="emailTempletId"></param>
        public ResponseModel ExportToExcel(long emailTempletId)
        {
            var email = _iAccount.ListOfStatus(emailTempletId);

            DataTable table = new DataTable();
            table = ConvertToDataTable(email.RecipientStatusList);
            return ExportStatusToExcel(table, emailTempletId);

        }
        public DataTable ConvertToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();

            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        /// <summary>
        /// This method takes DataSet as input paramenter and it exports the same to excel
        /// </summary>
        /// <param name="table"></param>
        public ResponseModel ExportStatusToExcel(DataTable table, long emailTempletId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                object misValue = System.Reflection.Missing.Value;
                //Creae an Excel application instance
                ExcelExport.Application excelApp = new ExcelExport.Application();
                var url = @"http://lead411testapp.azurewebsites.net/FileUpload/FileStatusForId-" + emailTempletId + ".xls";
                ExcelExport.Workbook excelWorkBook = excelApp.Workbooks.Add(misValue);


                //Add a new worksheet to workbook with the Datatable name
                //ExcelExport.Worksheet excelWorkSheet = excelWorkBook.Sheets.Add();
                ExcelExport.Worksheet excelWorkSheet = (ExcelExport.Worksheet)excelWorkBook.ActiveSheet;
                excelWorkSheet.Name = "Export"; //table.TableName;

                for (int i = 1; i < table.Columns.Count + 1; i++)
                {
                    excelWorkSheet.Cells[1, i] = table.Columns[i - 1].ColumnName;
                }

                for (int j = 0; j < table.Rows.Count; j++)
                {
                    for (int k = 0; k < table.Columns.Count; k++)
                    {
                        excelWorkSheet.Cells[j + 2, k + 1] = table.Rows[j].ItemArray[k].ToString();
                    }
                }
                excelWorkBook.SaveAs(url, ExcelExport.XlFileFormat.xlWorkbookNormal, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, ExcelExport.XlSaveAsAccessMode.xlShared, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value);
                excelWorkBook.Close();
                excelApp.Quit();


                releaseObject(excelWorkSheet);
                releaseObject(excelWorkBook);
                releaseObject(excelApp);
                response.Content = url;
                response.IsSuccess = true;
                return response;
            }
            catch (Exception e)
            {
                response.Content = e.InnerException == null ? "" : e.InnerException.ToString();
                response.Message = e.Message.ToString() == null ? "" : e.Message.ToString();
                response.IsSuccess = false;
                return response;
            }

        }
        public void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
            }
            finally
            {
                GC.Collect();
            }
        }

        public ExportData GetExportData(long emailTempletId)
        {
            ExportData exportdata = new ExportData();
            try
            {
                exportdata.RecipientStatusList = new List<ExportFile>();
                exportdata = _iAccount.ListOfStatus(emailTempletId);

            }
            catch (Exception ex)
            {

            }
            return exportdata;

        }

        public ResponseModel GetContactListToExport(string emailId)
        {
            ResponseModel response = new ResponseModel();
            response = _iAccount.GetContactListToExport(emailId);
            return response;
        }

    }
}
