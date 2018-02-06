using System.Net;
using System.Net.Mail;

namespace CoreEntities.Helper
{
    public class EmailHelper
    {
        public static void SendEmail(string emailId, string body, string subject)
        {
            var fromAddress = new MailAddress(EmailParameters.SmtpDefaultEmail);
            var toAddress = new MailAddress(emailId);
            string fromPassword = EmailParameters.SmtpDefaultPassword;

            var smtp = new SmtpClient
            {
                Host = EmailParameters.SmtpDefaultHost,
                Port = EmailParameters.SmtpDefaultPort,
                EnableSsl = EmailParameters.SmtpDefaultEnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                Timeout = 20000
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }
}
