using CoreEntities.CustomModels;
using RestSharp;
using ServiceLayer.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Lead411.Controllers
{
    public class DefaultController : ApiController
    {
        private readonly ServiceLayer.Interfaces.ICommonService _iCommonService;

        public DefaultController(ServiceLayer.Interfaces.ICommonService iCommonService)
        {
            _iCommonService = iCommonService;
        }

        /// <summary>
        /// Test Method
        /// </summary>
        /// <returns></returns>
        public Object Get()
        {
            RequestObject requestObject = DataServices.DemoUrl;
            IRestResponse resp = _iCommonService.RestClientRequest(requestObject);
            return resp;
        }
    }
}
