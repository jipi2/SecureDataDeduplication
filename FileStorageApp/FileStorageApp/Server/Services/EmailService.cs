using System.Net;
using System.Net.Mail;

namespace FileStorageApp.Server.Services
{
    public class EmailService
    {
        SmtpClient client;
        private readonly IConfiguration _config;
        public EmailService( IConfiguration config)
        {
            _config = config;

            client = new SmtpClient("smtp-mail.outlook.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(_config.GetSection("email_user").Value, _config.GetSection("email_password").Value),
                EnableSsl = true
            };
        }

        public void SendEmail(string toDest, string subject, string body)
        {
            client.Send(_config.GetSection("email_user").Value, toDest, subject, body);
        }
    }
}
