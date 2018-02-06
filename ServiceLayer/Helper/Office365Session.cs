using CoreEntities.Helper;
using Microsoft.Office365.OutlookServices;
using System;
using System.Threading.Tasks;

namespace ServiceLayer.Helper
{
   public class Office365Session
    {
        /// <summary>
        /// Gloable AccessToken generator
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetAccessToken()
        {
            await Task.FromResult(0);
            return AccessToken;
        }

        /// <summary>
        /// Global access token
        /// </summary>
        public string AccessToken { get; set; }

       
    }
}
