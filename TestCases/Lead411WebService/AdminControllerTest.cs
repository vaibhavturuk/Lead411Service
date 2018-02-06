using CoreEntities.CustomModels;
using CoreEntities.CustomModels.AdminPanel;
using Lead411.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace TestCases.Lead411WebService
{
    [TestClass]
    public class AdminControllerTest
    {
        [TestMethod]
        public void GetUserListTestMethod()
        {
            var kernel = new StandardKernel(new NinjectModel());
            var adminApiController = kernel.Get<AdminApiController>();
            IHttpActionResult iHttpActionResult = adminApiController.GetUserList(2,5, "lastName",true,"");
            var contentResult = iHttpActionResult as OkNegotiatedContentResult<ResponseModel>;
            Assert.IsTrue(contentResult != null && contentResult.Content.IsSuccess);
            Assert.IsNotNull(contentResult.Content.Content);
        }

        [TestMethod]
        public void GetUserDetailsTestMethod()
        {
            var kernel = new StandardKernel(new NinjectModel());
            var adminApiController = kernel.Get<AdminApiController>();
            IHttpActionResult iHttpActionResult = adminApiController.GetUserDetails(15);
            var contentResult = iHttpActionResult as OkNegotiatedContentResult<ResponseModel>;
            Assert.IsTrue(contentResult != null && contentResult.Content.IsSuccess);
            Assert.IsNotNull(contentResult.Content.Content);
        }

    }
}
