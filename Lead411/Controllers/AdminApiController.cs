using CoreEntities.CustomModels;
using System;
using System.Web.Http;
using ServiceLayer.Interfaces.AdminPanel;
using CoreEntities.CustomModels.AdminPanel;
using CoreEntities.Helper;

namespace Lead411.Controllers
{
    public class AdminApiController : ApiController
    {
        private readonly IUserService _iUserService;

        public AdminApiController(IUserService iUserService)
        {
            _iUserService = iUserService;
        }

        [HttpGet]
        public IHttpActionResult GetUserList(int pageNumber, int pageSize, string sortBy, bool reverse, string search)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                responseModel.Content = _iUserService.GetUserList(pageNumber, pageSize, sortBy, reverse, search);
                if (responseModel.Content != null)
                {
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
            catch (Exception ex)
            {
                responseModel.Content = "";
                responseModel.Message = ex.Message;
                responseModel.IsSuccess = false;
            }
            return Ok(responseModel);
        }

        [HttpGet]
        public IHttpActionResult GetUserDetails(long userMembershipId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                responseModel.Content = _iUserService.GetUserDetails(userMembershipId);
                if (responseModel.Content != null)
                {
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
            catch (Exception ex)
            {
                responseModel.Content = "";
                responseModel.Message = ex.Message;
                responseModel.IsSuccess = false;
            }
            return Ok(responseModel);
        }

        [HttpPost]
        public IHttpActionResult SendEmail(EmailUser emailUser)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                string[] emails = emailUser.EmailId.Split(',');
                for (int i = 0; i < emails.Length; i++)
                {
                    EmailHelper.SendEmail(emails[i], emailUser.Body, emailUser.Subject);
                }
                responseModel.IsSuccess = true;
                responseModel.Message = CoreEntities.Helper.Messages.ProcessCompeleted;
            }
            catch (Exception ex)
            {
                responseModel.Content = "";
                responseModel.Message = ex.Message;
                responseModel.IsSuccess = false;
            }
            return Ok(responseModel);
        }
    }
}
