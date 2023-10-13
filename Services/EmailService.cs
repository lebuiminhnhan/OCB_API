using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using OCB_API.Models;

namespace OCB_API.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfiguration _emailConfig;

        public EmailService(IOptions<EmailConfiguration> emailConfig)
        {
            _emailConfig = emailConfig.Value;
        }

        public void SendContactEmail(string toName, string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("OCB", _emailConfig.From));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                TextBody = body
            };

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.CheckCertificateRevocation = false;
                client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, false);
                client.Authenticate(_emailConfig.UserName, _emailConfig.Password);
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
