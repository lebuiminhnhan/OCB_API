namespace OCB_API.Models
{
    public class EmailConfiguration
    {
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string From { get; set; }
    }

    public interface IEmailService
    {
        void SendContactEmail(string toName, string toEmail, string subject, string body);
    }
}
