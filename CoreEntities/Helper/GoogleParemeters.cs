using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreEntities.Helper
{
    public class GoogleParameters
    {
        //For Azure Staging
        //public static string ClientId { get; } = "754756613373-fm0nhitf82m368pm6953et6g6phrld6r.apps.googleusercontent.com";
        //public static string ClientSecret { get; } = "1BV-RnRkWiJgLZUHj6RPiwCa";
        //end///////
        //For Azure
        public static string ClientId { get; } = "754756613373-8t8dhl75gc00qr4861loku9qq24o0crr.apps.googleusercontent.com";
        public static string ClientSecret { get; } = "RYtQnc2jBZkeiU9HU-AZAf_X";

        //end///////
        //For Local
        //public static string ClientId { get; } = "948838070491-0ta8b3dpnpj896emi5vikraveufu9181.apps.googleusercontent.com";
        //public static string ClientSecret { get; } = "7k1idioibd5iocamyuyfcu0k";

        //end///////
        //For 108
        //public static string ClientId { get; } = "948838070491-6s1lve2m63m2ekmjj7np8tq9tfog1gjb.apps.googleusercontent.com";
        //public static string ClientSecret { get; } = "bXyhvaiTyGWyO_yHuxhUH1u4";
        //end///////
        public static string UserInfoV1 { get; } = "https://www.googleapis.com/oauth2/v1/userinfo?alt=json";
        public static string GoogleValidateV4 { get; } = "https://www.googleapis.com/oauth2/v4/token";
        public static string Scope { get; } = "http://mail.google.com email profile https://www.google.com/m8/feeds/";
    }
}