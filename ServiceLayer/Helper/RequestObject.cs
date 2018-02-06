using RestSharp;
using System.Collections.Generic;

namespace ServiceLayer.Helper
{
    public class RequestObject
    {
        public string Url { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public Method RequestType { get; set; } = Method.POST;
    }
}
