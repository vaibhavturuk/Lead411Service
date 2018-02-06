using CoreEntities.CustomModels;
using System;
using System.Web.Http;
using CoreEntities.enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLayer.Interfaces;
using System.Web;
using System.IO;
using System.Net;
using System.Linq;
using Excel;
using System.Data;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using CoreEntities.Domain;

namespace Lead411.Controllers
{
    [RoutePrefix("api/AccountApi")]
    public class AccountApiController : ApiController
    {
        // GET: api/AccountApi
        //Initilization the variables for accessing the data from services used to declasre variable or object.
        // these objects are used to call the methods of the services and pass the parameters in the methods.

        private readonly ICommonService _iCommonService;
        private readonly IService _googleServiceNew;

        public AccountApiController(
            ICommonService iCommonService,
            IGoogleService googleService
        )
        {
            _iCommonService = iCommonService;
            _googleServiceNew = googleService;
        }

        /// <summary>
        /// To recieve request from ionic application
        /// </summary>
        /// <param name="authorizedCode"></param>
        /// <param name="accountType"></param>
        /// <param name="platform"></param>
        /// <param name="uuid"></param>
        /// <param name="model"></param>
        /// <param name="reqType"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAccessToken")]
        [AllowAnonymous]
        public IHttpActionResult GetAccessToken(
            string authorizedCode,
            AccountType accountType,
            string platform,
            string uuid,
            string model,
            RequestType reqType)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                if (!string.IsNullOrEmpty(authorizedCode) && !string.IsNullOrEmpty(accountType.ToString()) &&
                    !string.IsNullOrEmpty(platform) && !string.IsNullOrEmpty(uuid) && !string.IsNullOrEmpty(model))
                {
                    var deviceInfo = new DeviceInfo() { Model = model, Platform = platform, Uuid = uuid };
                    if (accountType == AccountType.Gmail)
                    {
                        //Updated code to call authentication process 
                        responseModel = _googleServiceNew.UserAuthentication(authorizedCode, accountType, deviceInfo, reqType);
                    }
                }
                else
                {
                    responseModel.Content = "";
                    responseModel.Message = CoreEntities.Helper.Messages.ProcessFailed;
                    responseModel.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                responseModel.Content = "";
                responseModel.Message = ex.Message+" Innerexception: "+ex.InnerException;
                responseModel.IsSuccess = false;
            }
            return Ok(responseModel);
        }

        /// <summary>
        /// When authenticated user is coming back on application then by this method we can check whether user is valid or not.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="platform"></param>
        /// <param name="uuid"></param>
        /// <param name="model"></param>
        /// <param name="reqType"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("IsUserExists")]
        public IHttpActionResult IsUserExists(
            long userId,
            string platform,
            string uuid,
            string model,
            RequestType reqType)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                if (userId > 0 && !string.IsNullOrEmpty(platform) && !string.IsNullOrEmpty(uuid) &&
                    !string.IsNullOrEmpty(model))
                {
                    string accessToken = Request.Headers.Authorization.Parameter;
                    var deviceInfo = new DeviceInfo() { Model = model, Platform = platform, Uuid = uuid };
                    var Lead411UserInfoList = _iCommonService.GetUserInfoByUserId(userId, deviceInfo, accessToken,
                        reqType);

                    if (Lead411UserInfoList.Provider == AccountType.Gmail.ToString())
                    {
                        Lead411UserInfoList = _googleServiceNew.GetUserInfoByUserId(Lead411UserInfoList, reqType);
                    }

                    if (Lead411UserInfoList != null)
                    {
                        responseModel.Content = Lead411UserInfoList;
                        responseModel.Message = CoreEntities.Helper.Messages.ProcessCompeleted;
                        responseModel.IsSuccess = true;
                    }
                    else
                    {
                        responseModel.Content = "";
                        responseModel.Message = CoreEntities.Helper.Messages.ProcessFailed;
                        responseModel.IsSuccess = false;
                    }
                }
                else
                {
                    responseModel.Content = "";
                    responseModel.Message = CoreEntities.Helper.Messages.ProcessFailed;
                    responseModel.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                responseModel.Content = "";
                responseModel.Message = ex.Message;
                responseModel.IsSuccess = false;
            }
            return Ok(responseModel);
        }

        /// <summary>
        /// To recieve request from ionic application
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetUserDetails")]
        [AllowAnonymous]
        public IHttpActionResult GetUserDetailsByToken(
            string accessToken)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                if (!string.IsNullOrEmpty(accessToken))
                {
                    responseModel = _googleServiceNew.GetUserInfoByToken(accessToken);
                    accessToken = responseModel.AccessToken;

                }
                else
                {
                    responseModel.Content = "";
                    responseModel.Message = "Not able to get the access code.";
                    responseModel.IsSuccess = false;
                    responseModel.AccessToken = accessToken;
                }

            }
            catch (Exception exec)
            {
                responseModel.Content = "";
                responseModel.Message = exec.Message;
                responseModel.IsSuccess = false;
                responseModel.AccessToken = accessToken;
            }
            return Ok(responseModel);

        }
        

        /// <summary>
        /// Process mail from webjobs
        /// </summary>
        /// <param name="emailId"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("ProcessEmail")]
        public async Task<IHttpActionResult> ProcessEmail(string emailId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var accountType = _iCommonService.GetAccountTypeByEmailId(emailId);

                if (accountType == AccountType.Gmail)
                {
                    await _googleServiceNew.ProcessSingleEmail(emailId);
                }


                response.Content = null;
                response.Message = CoreEntities.Helper.Messages.ProcessCompeleted;
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.Content = null;
                response.Message = ex.Message;
                response.IsSuccess = false;
            }
            return Ok(response);
        }

        /// <summary>
        /// Get batch of mail process
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult GetNextMailToBeProcessed()
        {
            ResponseModel response = new ResponseModel();
            try
            {
                List<Lead411UserInfo> mailList = _iCommonService.GetNextMailToBeProcessed();

                response.Content = mailList;
                response.Message = CoreEntities.Helper.Messages.ProcessCompeleted;
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.Content = null;
                response.Message = ex.Message;
                response.IsSuccess = false;
            }
            return Ok(response);
        }

        /// <summary>
        /// Delete or reset account
        /// </summary>
        /// <param name="userMembershipId"></param>
        /// <param name="settingType"></param>
        /// <param name="deleteFolder"></param>
        /// <param name="reqType"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IHttpActionResult> AccountSetting(
            long userMembershipId,
            SettingType settingType,
            bool deleteFolder,
            RequestType reqType)
        {
            ResponseModel responseModel = new ResponseModel();
            bool success = false;
            try
            {
                if (userMembershipId > 0) // && !string.IsNullOrEmpty(settingType))
                {
                    switch (settingType)
                    {
                        case SettingType.resetindexing:
                            success = _iCommonService.ResetIndexing(userMembershipId);
                            if (success)
                                responseModel.Message = CoreEntities.Helper.Messages.ResetSuccessfully;
                            break;

                        case SettingType.deleteaccount:
                            bool isSuccess = _iCommonService.DeleteAccount(userMembershipId);

                            if (isSuccess && deleteFolder)
                            {
                                Lead411UserInfo Lead411UserInfo =
                                   _iCommonService.GetUserSessionUserMembershipWise(userMembershipId);

                                if (Lead411UserInfo.Provider == AccountType.Gmail.ToString())
                                {
                                    success = await _googleServiceNew.DeleteContactFolder(Lead411UserInfo, reqType);
                                }

                            }

                            if (success)
                                responseModel.Message = CoreEntities.Helper.Messages.DeletedSuccessfully;

                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(settingType), settingType, null);
                    }

                    responseModel.Content = success;
                    responseModel.IsSuccess = success;
                }
                else
                {
                    responseModel.Content = "";
                    responseModel.Message = CoreEntities.Helper.Messages.ProcessFailed;
                    responseModel.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                responseModel.Content = "";
                responseModel.Message = ex.Message;
                responseModel.IsSuccess = false;
            }
            return Ok(responseModel);
        }


        
        /// <summary>
        /// Save the Uploaded excel file to folder and data to database and send the email in Queue
        /// </summary>
        /// <param name="email">string</param>
        /// <returns>ResponseModel</returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("UploadFile")]
        public IHttpActionResult UploadFile()
        {

            ResponseModel response = new ResponseModel();
            #region Save the Uploaded document
            string accessToken = Request.Headers.Authorization.Parameter;
            if (_googleServiceNew.CheckTokenExpired(accessToken))
            {
                response = _googleServiceNew.GetNewAccessToken(accessToken);
                accessToken = response.AccessToken;
            }
                response.IsSuccess = true;
                var httpRequest = System.Web.HttpContext.Current.Request;
                var emailSubject = httpRequest.Form["emailSubject"];
                var emailFrom = httpRequest.Form["emailFrom"];
                var emailBody = httpRequest.Unvalidated.Form.Get("emailBody");
                if (emailBody.Contains("<s"))
                {
                    if (emailBody.Contains("<sc") && emailBody.Contains("t>"))
                    {
                        response.IsSuccess = false;
                        response.Message = "<script> tags are not allowed. Please remove <Script> tags from Email Body.";
                        response.Content = "";
                        response.AccessToken = accessToken;
                        return Ok(response);
                    }
                }
                if (httpRequest.Files.Count > 0)
                {
                    var docfiles = new List<string>();
                    foreach (string file in httpRequest.Files)
                    {
                        var postedFile = httpRequest.Files[file];
                        var yourFileName = postedFile.FileName;
                        string[] primePath = yourFileName.Split('.');
                        yourFileName = primePath[0].ToString() + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "." + primePath[1];
                        string targetFolder = HttpContext.Current.Server.MapPath("~/FileUpload");
                        string targetPath = Path.Combine(targetFolder, yourFileName);
                        postedFile.SaveAs(targetPath);
                        //var filePath = HttpContext.Current.Server.MapPath("~/" + postedFile.FileName);
                        var filePath = HttpContext.Current.Server.MapPath("FileUpload") + "\\" + postedFile.FileName;
                        var Url = "http://lead411chromeex.azurewebsites.net/FileUpload/";
                        //var Url = "http://stagingwin.com/lead411/FileUpload/";
                        var fileName = Url + postedFile.FileName;
                        Stream stream = postedFile.InputStream;
                        IExcelDataReader reader = null;
                        DataSet res = new DataSet();

                        if (postedFile.FileName.EndsWith(".xls"))
                        {
                            //reads the excel file with .xls extension
                            reader = ExcelReaderFactory.CreateBinaryReader(stream);
                        }
                        else if (postedFile.FileName.EndsWith(".xlsx"))
                        {
                            //reads excel file with .xlsx extension
                            reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                        }
                        else if (postedFile.FileName.EndsWith(".csv"))
                        {
                            string[] allLines = File.ReadAllLines(@targetPath);
                            DataSet ds = new DataSet();
                            var tbl = new DataTable();
                            DataColumn column;
                            DataRow row;
                            //
                            column = new DataColumn();
                            column.DataType = System.Type.GetType("System.String");
                            column.ColumnName = "Employee Id";
                            tbl.Columns.Add(column);
                            //
                            column = new DataColumn();
                            column.DataType = Type.GetType("System.String");
                            column.ColumnName = "Email";
                            tbl.Columns.Add(column);
                            //
                            column = new DataColumn();
                            column.DataType = Type.GetType("System.String");
                            column.ColumnName = "Firstname";
                            tbl.Columns.Add(column);


                            for (int i = 1; i < allLines.Count(); i++)
                            {
                                string aline = allLines[i].ToString();
                                string[] data = aline.Split(',');
                                row = tbl.NewRow();
                                row["Employee Id"] = data[0];
                                row["Email"] = data[1];
                                row["Firstname"] = data[2];
                                tbl.Rows.Add(row);
                            }
                            ds.Tables.Add(tbl);

                            res = ds;

                            //rer
                        }
                        else
                        {
                            //Shows error if uploaded file is not Excel file
                            return BadRequest("This file format is not supported");
                        }

                        if ((postedFile.FileName.EndsWith(".xlsx")) || (postedFile.FileName.EndsWith(".xls")))
                        {
                            //treats the first row of excel file as Coluymn Names
                            reader.IsFirstRowAsColumnNames = true;
                            //Adding reader data to DataSet()
                            res = reader.AsDataSet();
                            reader.Close();

                        }
                        fileName = postedFile.FileName;
                        var filepath = Url + yourFileName;
                        #endregion

                        #region Code to save in Local DB

                        response = Task.Run(() => _iCommonService.PushMailInDB(emailSubject, emailBody, res, fileName, emailFrom, filepath)).Result;

                    #endregion
                    response.AccessToken = accessToken;
                        return Ok(response);
                    }
                }
                else
                {
                    return BadRequest("No file Selected");
                }
            response.AccessToken = accessToken;
            return Ok(response);

            
        }


        /// <summary>
        /// Get the list of files uploaded by the User
        /// </summary>
        /// <param name="email">string</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("GetFileList")]
        public IHttpActionResult GetFileList(string email)
        {
            ResponseModel response = new ResponseModel();

            string accessToken = Request.Headers.Authorization.Parameter;
            if (_googleServiceNew.CheckTokenExpired(accessToken))
            {
                response = _googleServiceNew.GetNewAccessToken(accessToken);
                accessToken = response.AccessToken;
            }
            response = _iCommonService.GetListOfFile(email);
            response.AccessToken = accessToken;
            return Ok(response);
        }

        /// <summary>
        /// Get email list for respective file.
        /// </summary>
        /// <param name="contact">ContactView</param>
        /// <returns>response</returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("GetEmailList")]
        public IHttpActionResult GetEmailList(long emailTempletId, bool isBounce)
        {
            ResponseModel response = new ResponseModel();

            string accessToken = Request.Headers.Authorization.Parameter;
            if (_googleServiceNew.CheckTokenExpired(accessToken))
            {
                response = _googleServiceNew.GetNewAccessToken(accessToken);
                accessToken = response.AccessToken;

            }
            else
            {
                response.IsSuccess = true;
                response.Content = _iCommonService.GetListOfEmail(emailTempletId, isBounce);
                response.AccessToken = accessToken;
            }
            return Ok(response);
        }


        /// <summary>
        /// ReProcess the mails which caused exceptions 
        /// </summary>
        /// <param name="emailTempletId"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("ReProcesseEmails")]
        public IHttpActionResult ReProcesseEmails(long emailTempletId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                string accessToken = Request.Headers.Authorization.Parameter;
                string softBounce = Request.Headers.Authorization.Parameter; ;
                if (_googleServiceNew.CheckTokenExpired(accessToken))
                {
                    response = _googleServiceNew.GetNewAccessToken(accessToken);
                    accessToken = response.AccessToken;
                }
               
                    if(_iCommonService.CheckProcessStatus(emailTempletId))
                    {
                        response.IsSuccess = true;
                        response.Message = "Sorry! Emails are already in Process.";
                    }
                    else
                    {
                        //get the details from database
                        var emails = _iCommonService.GetMessageText(emailTempletId);
                        //cal method to push the msg in Queue.
                        var resp = Task.Run(() => _iCommonService.PushMailInQueue(emails)); //_iCommonService.PushMailInQueue(emails);
                        response.IsSuccess = true;
                        response.Message = "Reprocess request submitted successfully";
                    }
                response.AccessToken = accessToken;
                return Ok(response);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
            
        }

        /// <summary>
        /// Create and Edit Contact for individual User
        /// </summary>
        /// <param name="contact">ContactView</param>
        /// <returns>response</returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("GetContactDetails")]
        public IHttpActionResult GetContactDetails(string emailId, string userMailId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                string accessToken = Request.Headers.Authorization.Parameter;
                if (_googleServiceNew.CheckTokenExpired(accessToken))
                {
                    response = _googleServiceNew.GetNewAccessToken(accessToken);
                    accessToken = response.AccessToken;

                }
                response = _iCommonService.GetContactDetails(emailId, userMailId);
                response.AccessToken = accessToken;
                return Ok(response);
            }
            catch(Exception ex)
            {
                var exec = ex.Message;
                return BadRequest("Something went wrong while fetching the contact.Please try again.");
            }
            
        }

        /// <summary>
        /// Create and Edit Contact for individual User
        /// </summary>
        /// <param name="contact">ContactView</param>
        /// <returns>response</returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("CreateContact")]
        public IHttpActionResult CreateContact(ContactView contact)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                string accessToken = Request.Headers.Authorization.Parameter;
                if (_googleServiceNew.CheckTokenExpired(accessToken))
                {
                    response = _googleServiceNew.GetNewAccessToken(accessToken);
                }
               
                    response = _iCommonService.SaveOrUpdateContact(contact);
                    response.IsSuccess = true;
                    response.Message = "Saved";
                    response.Content = contact;
               
                return Ok(response);
            }
            catch
            {
                return BadRequest("Something went wrong!Please try again.");

            }

        }

        /// <summary>
        /// Get Contact List by emailId 
        /// </summary>
        /// <param name="contact">ContactView</param>
        /// <returns>response</returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("GetContactList")]
        public IHttpActionResult GetContactList(string email)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                string accessToken = Request.Headers.Authorization.Parameter;
                if (_googleServiceNew.CheckTokenExpired(accessToken))
                {
                    response = _googleServiceNew.GetNewAccessToken(accessToken);
                    accessToken = response.AccessToken;
                }

                response = _iCommonService.GetContactList(email);
                response.AccessToken = accessToken;
                return Ok(response);
            }
            catch (Exception ex)
            {
                var exec = ex.Message;
                return BadRequest("Something went wrong while fetching Contact list.Please try again.");
            }

        }


        /// <summary>
        /// Export the excel with email and its status
        /// </summary>
        /// <param name="contact">ContactView</param>
        /// <returns>response</returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("ExportStatusFile")]
        public IHttpActionResult ExportStatusFile(long emailTempletId)
        {
            ResponseModel response = new ResponseModel();

            string accessToken = Request.Headers.Authorization.Parameter;
            if (_googleServiceNew.CheckTokenExpired(accessToken))
            {
                response = _googleServiceNew.GetNewAccessToken(accessToken);
                accessToken = response.AccessToken;

            }
            response.Content=_iCommonService.ExportToExcel(emailTempletId);
            response.AccessToken = accessToken;
                response.IsSuccess = true;
            return Ok(response);
        }


        /// <summary>
        /// Get list of email and its status for a file
        /// </summary>
        /// <param name="emailTempletId">long</param>
        /// <returns>response</returns>
        [HttpGet]
        [AllowAnonymous]

        [Route("GetStatusFileInfo")]
        public IHttpActionResult GetStatusFileInfo(long emailTempletId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                string accessToken = Request.Headers.Authorization.Parameter;
                if (_googleServiceNew.CheckTokenExpired(accessToken))
                {
                    response = _googleServiceNew.GetNewAccessToken(accessToken);
                    accessToken = response.AccessToken;

                }

                var exportdata = _iCommonService.GetExportData(emailTempletId);
                    if (exportdata != null)
                    {
                        response.IsSuccess = true;
                        response.Message = "Status information for templete: " + emailTempletId;
                        response.Content = exportdata;
                    }
                    else
                    {
                        response.IsSuccess = true;
                        response.Message = "Sorry! No data available Or A   n exception Occured.";
                        response.Content="";
                        

                }
                response.IsSuccess = true;
                response.AccessToken = accessToken;
                return Ok(response);
            }
            catch(Exception e)
            {
                response.IsSuccess = false;
                response.Message = "Something went wrong while exporting data.";
                response.AccessToken = Request.Headers.Authorization.Parameter;

                return BadRequest(e.Message.ToString());

            }
           
        }

        /// <summary>
        /// Get Contact List by emailId in last 24hrs
        /// </summary>
        /// <param name="contact">ContactView</param>
        /// <returns>response</returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("GetContactListToExport")]
        public IHttpActionResult GetContactListToExport(string email, bool dummy = false)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                string accessToken = Request.Headers.Authorization.Parameter;
                if (_googleServiceNew.CheckTokenExpired(accessToken))
                {
                    response = _googleServiceNew.GetNewAccessToken(accessToken);
                    accessToken = response.AccessToken;
                }

                response = _iCommonService.GetContactListToExport(email);
                response.AccessToken = accessToken;
                return Ok(response);
            }
            catch (Exception ex)
            {
                var exec = ex.Message;
                return BadRequest("Something went wrong while fetching Contact list.Please try again.");
            }

        }
    }
}