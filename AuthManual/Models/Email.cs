using MailKit.Net.Smtp;
using MimeKit;

namespace AuthManual.Models
{
    public class Email
    {
        public string FromAddress { get; set; }
        public string FromName { get; set; }
        public string ToAddress { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string SmtpConnection { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPassword { get; set; }

        public void SendMail()
        {
            var mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress(FromName, FromAddress));
            mailMessage.To.Add(new MailboxAddress("Otovınn.com", ToAddress));
            mailMessage.Subject = Subject;
            var bodyBuilder = new BodyBuilder();

            bodyBuilder.HtmlBody = Body;
            mailMessage.Body = bodyBuilder.ToMessageBody();

            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Connect(SmtpConnection, SmtpPort, false);
                smtpClient.Authenticate(SmtpUser, SmtpPassword);
                try
                {
                    smtpClient.Send(mailMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Mail gönderilirken bir hata oluştu: {ex.Message}");
                }
                
                smtpClient.Disconnect(true);
            }
        }
    }
}
