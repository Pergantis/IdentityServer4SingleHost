using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace IdentityServer4SingleHost.Infrastructure.Emails
{
    public class MailProvider
    {
        private readonly EmailSettings _emailSettings;

        public MailProvider(EmailSettings emailSettings)
        {
            _emailSettings = emailSettings;
        }

        public async Task SendEmailAsync(string email, string subject, string message, bool sendBccEmail = false)
        {
            string toEmail = string.IsNullOrEmpty(email)
                ? _emailSettings.ToEmail
                : email;

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress
            (_emailSettings.FromEmail,
                _emailSettings.UsernameEmail
            ));
            mimeMessage.To.Add(new MailboxAddress
            (
                toEmail
            ));
            mimeMessage.Subject = "IdentityServer4SingleHost Mobile App - " + subject; //Subject
            mimeMessage.Body = new TextPart("html") { Text = message };

            if(sendBccEmail)
                mimeMessage.Bcc.Add(new MailboxAddress(_emailSettings.BccEmail));

            using var smtp = new SmtpClient();

            smtp.Connect(_emailSettings.PrimaryDomain, _emailSettings.PrimaryPort, SecureSocketOptions.None);
            smtp.Authenticate(_emailSettings.UsernameEmail, _emailSettings.UsernamePassword);

            await smtp.SendAsync(mimeMessage);
            await smtp.DisconnectAsync(true);
        }
    }
}

