using CoreEntities.CustomModels;
using CoreEntities.enums;
using Lead411.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace TestCases.Lead411WebService
{
    [TestClass]
    public class AccountControllerTest
    {
        [TestMethod]
        public void IsUserExistsTestMethod()
        {
            var kernel = new StandardKernel(new NinjectModel());
            var accountApiController = kernel.Get<AccountApiController>();
            accountApiController.Request = new HttpRequestMessage();

            //Office365 test
            //accountApiController.Request.Headers.Add("Authorization", "Bearer EwAQA+l3BAAUWm1xSeJRIJK6txKjBez4GzapzqMAAciL2/PzyduPI0iix0SWN1Lu6xFi3av3J4iiIlXIS5Zsn2ysRdL4W4DIx7CmiXsVZArkuwxhPfY6RF7j7I4EfdSY+w8r1vMZCfeOP5l90DRSer3U3KdZl3QJIKlf//q6A6BSejOvQWvgM6HvzSQlXURosBAHZeCGWrhvYXV60eMmtJ3ChCaCAV1+KAV51l1H2yUaua++iqWh+8+YaU/hzMHgbWurdmgMaUzb2ttm38LOfnA8WKpRcMHOgHxVsaxQwajiGpu6qKGy6lQc8V+ip9jZ/ad3nKlsDPUzW2WAUm8hjeAyPLFomcft0SHEuzAqqRExMvFGrqo21qEALGLYAYMDZgAACL0waIY8fhwm4AEzg2I7K1RIkrOiWLVOuXiJnWWITYXcwC93eyAo0ESC9nN9JTNMpitNCmoIUEFaKCwe4KnMSZ2GaYLR5Y7s6OeoSE1lpMhtO/mj+yRaVDRHhSyFvp3X3kkXSPViQlcGuOSyA10+/vjAxyF2E9d87M5gGhGXNrGFdk9YKxCaG/lwcFnYFWuvavet5qR/OpI+cInjvkTgMq3FUkUFcOPHUWhB5fJbn5+zy/4dHwGyY1viDEbFyjO7SYCUxahkSHe1qrvOm2pKe1whXrFfcu/XY5nBxCopcDcNvFQ24L/UpAcMDczahrsShqU0XKtR1NhZUNVKu4Xb5v8rzIXZJufPD7Xtnwq02ax05X/nz8mQv9YoqZaRO16vMhzmK5OUMJtZOnYEbrTAoY9JjIdSRhCJ2+hdGAx85CwNbhWINFZV2b1Du9HZMVOTsxQ4uGCfxo0eCo3aU0QX4+goxcA68dxljFYl/vppft2tUFzlzT/s9ZQz9JXeGwiavBJ0+m7WhtV01GHpGEyVqJKJ/w60kvKyhzRDZB6u19PTyV7YE0Amx4MfdCc8JfJgY8guVFfget+uh5HM6GxIh+zh2H8ytdWhByi13ZkFg0DV0qR0jY7fIV7T/KTw5h5y32JTlfQ/gMHdOsYcAg==");
            //IHttpActionResult iHttpActionResult = accountApiController.IsUserExists(12, "XSD", "IDG-98i9", "MyModel", RequestType.web);

            //var contentResult = iHttpActionResult as OkNegotiatedContentResult<ResponseModel>;
            //Assert.IsTrue(contentResult != null && contentResult.Content.IsSuccess);
            //Assert.IsNotNull(contentResult.Content.Content);
            //Assert.IsNotNull(((Lead411UserInfo)contentResult.Content.Content).AccessToken);
            //Assert.IsNotNull(((Lead411UserInfo)contentResult.Content.Content).Email);
            //Assert.IsNotNull(((Lead411UserInfo)contentResult.Content.Content).Provider);
            //Assert.IsNotNull(((Lead411UserInfo)contentResult.Content.Content).RefreshToken);
            //Assert.IsNotNull(((Lead411UserInfo)contentResult.Content.Content).UserMembershipId);

            //Google test
            accountApiController.Request = new HttpRequestMessage();
            //accountApiController.Request.Headers.Add("Authorization", "Bearer ya29.Ci_AA6hlMLQ40okMRykijzLMlilaJtGFvsVK6BzrV_r28_buyf8zHjU9A0POfHhv9Q");
            accountApiController.Request.Headers.Add("Authorization", "Bearer AccessToken");
            var iHttpActionResult = accountApiController.IsUserExists(25, "XSD", "IDG-98i9", "MyModel", RequestType.web);
            var contentResult = iHttpActionResult as OkNegotiatedContentResult<ResponseModel>;
            Assert.IsTrue(contentResult != null && contentResult.Content.IsSuccess);
            Assert.IsNotNull(contentResult.Content.Content);
            Assert.IsNotNull(((Lead411UserInfo)contentResult.Content.Content).AccessToken);
            Assert.IsNotNull(((Lead411UserInfo)contentResult.Content.Content).Email);
            Assert.IsNotNull(((Lead411UserInfo)contentResult.Content.Content).Provider);
            Assert.IsNotNull(((Lead411UserInfo)contentResult.Content.Content).RefreshToken);
            Assert.IsNotNull(((Lead411UserInfo)contentResult.Content.Content).UserMembershipId);
        }

        [TestMethod]
        public async Task ProcessEmailTestMethod()
        {
            var kernel = new StandardKernel(new NinjectModel());
            var accountApiController = kernel.Get<AccountApiController>();
            //For valid email id
            //Gmail
            IHttpActionResult iHttpActionResult = await accountApiController.ProcessEmail("jaspreet@Lead411.com");
            var contentResult = iHttpActionResult as OkNegotiatedContentResult<ResponseModel>;
            Assert.IsTrue(contentResult != null && contentResult.Content.IsSuccess);

            //Outlook
            //iHttpActionResult = await accountApiController.ProcessEmail("kartikbhave.sdn@hotmail.com");
            //contentResult = iHttpActionResult as OkNegotiatedContentResult<ResponseModel>;
            //Assert.IsTrue(contentResult != null && contentResult.Content.IsSuccess);

            //For invalid email id
            //IHttpActionResult iHttpActionResult = await accountApiController.ProcessEmail("kartikbhave.s@gmail.com");
            //var contentResult = iHttpActionResult as OkNegotiatedContentResult<ResponseModel>;
            //Assert.IsFalse(contentResult != null && contentResult.Content.IsSuccess);
        }

        [TestMethod]
        public void GetNextMailToBeProcessedTestMethod()
        {
            var kernel = new StandardKernel(new NinjectModel());
            var accountApiController = kernel.Get<AccountApiController>();
            IHttpActionResult iHttpActionResult = accountApiController.GetNextMailToBeProcessed();
            var contentResult = iHttpActionResult as OkNegotiatedContentResult<ResponseModel>;
            Assert.IsTrue(contentResult != null && contentResult.Content.IsSuccess);
        }

        [TestMethod]
        public async Task AccountSettingTestMethod()
        {
            var kernel = new StandardKernel(new NinjectModel());
            var accountApiController = kernel.Get<AccountApiController>();
            //For Outlook
            IHttpActionResult iHttpActionResult = await accountApiController.AccountSetting(5, SettingType.resetindexing, false, RequestType.web);
            var contentResult = iHttpActionResult as OkNegotiatedContentResult<ResponseModel>;
            Assert.IsTrue(contentResult != null && contentResult.Content.IsSuccess);

            //For gmail
            iHttpActionResult = await accountApiController.AccountSetting(1, SettingType.resetindexing, false, RequestType.web);
            contentResult = iHttpActionResult as OkNegotiatedContentResult<ResponseModel>;
            Assert.IsTrue(contentResult != null && contentResult.Content.IsSuccess);
        }

        [TestMethod]
        public void GetAccessTokenTestMethod()
        {
            string accessCodeGoogle = "4/wi6tzpKpY84kvsDseCb8lOqK4QF1jmirRYp0qljwKpA";//"4/BqxbQ0sAYJiK_N9x9IyBda6eKtMZ79xsfvsl1hWyF90";//Must be new
            string accessCodeOutlook = "";// "M301f64e4-9f87-6039-8694-e1ce82526e13";
            var kernel = new StandardKernel(new NinjectModel());
            var accountApiController = kernel.Get<AccountApiController>();
            //For Google
            if (!string.IsNullOrEmpty(accessCodeGoogle))
            {
                IHttpActionResult iHttpActionResult = accountApiController.GetAccessToken(accessCodeGoogle, AccountType.Gmail, "Test", "Test", "Test", RequestType.web);
                var contentResult = iHttpActionResult as OkNegotiatedContentResult<ResponseModel>;
                ValidateResponse_GetAccessToken(contentResult);
            }
            //For Outlook
            if (!string.IsNullOrEmpty(accessCodeOutlook))
            {
                IHttpActionResult iHttpActionResult = accountApiController.GetAccessToken(accessCodeOutlook, AccountType.Office365, "Test", "Test", "Test", RequestType.web);
                var contentResult = iHttpActionResult as OkNegotiatedContentResult<ResponseModel>;
                ValidateResponse_GetAccessToken(contentResult);
            }
        }

        public void ValidateResponse_GetAccessToken(OkNegotiatedContentResult<ResponseModel> response)
        {
            Assert.IsTrue(response.Content.IsSuccess);
            Assert.IsNotNull(response.Content.Content);
            Assert.IsNotNull(((SuccessResponseModel)response.Content.Content).AccessToken);
            Assert.IsNotNull(((SuccessResponseModel)response.Content.Content).IsOldUser);
            Assert.IsNotNull(((SuccessResponseModel)response.Content.Content).UserId);
        }

    }
}
