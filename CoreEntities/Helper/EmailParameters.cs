using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreEntities.Helper
{
    public class EmailParameters
    {
        public static int SmtpDefaultPort { get; } = 587;
        public static bool SmtpDefaultEnableSsl { get; } = true;
        public static string SmtpDefaultEmail { get; } = "devacc12345@gmail.com";
        public static string SmtpDefaultHost { get; } = "smtp.gmail.com";
        public static string SmtpDefaultPassword { get; } = "Java@8410";
    }
}
