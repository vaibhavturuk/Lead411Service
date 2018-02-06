using CoreEntities.CustomModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Helper
{
    public class DataServices
    {
        private static string HostUrl { get; } = "http://localhost:59167/";
        public static RequestObject DemoUrl { get; } = new RequestObject { Url = HostUrl + "api/Home", RequestType = RestSharp.Method.GET };
    }
}
