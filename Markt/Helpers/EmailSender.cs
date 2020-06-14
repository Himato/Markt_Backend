using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;

namespace Markt.Helpers
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private const string Body = "<div bgcolor=\"#ffffff\"> <table width=\"500px\" align=\"center\"> <tbody> <tr> <td> <p align=\"center\"> </p> <p>Thanks for trusting us at Markt. This is your code:</p> <h3> @@@ </h3> <p>We are really happy for you joining our community. <p>Regards, <br> <strong>The Markt Team</strong> <br> </p> </td> </tr> </tbody> </table></div>";

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string code)
        {
            return Execute(email, subject, code);
        }

        public async Task Execute(string destination, string subject, string code)
        {
            try
            {
                using (var mail = new MailMessage())
                {
                    var email = _configuration["AdminDetails:Email"];
                    var password = _configuration["AdminDetails:Password"];

                    var loginInfo = new NetworkCredential(email, password);

                    mail.From = new MailAddress(email);
                    mail.To.Add(new MailAddress(destination));
                    mail.Subject = subject;
                    mail.Body = Body.Replace("@@@", code);
                    mail.IsBodyHtml = true;

                    //mail.AlternateViews.Add(GetAlternateView(message.Body));

                    try
                    {
                        using (var smtpClient = new SmtpClient(_configuration["AdminDetails:OutlookSmtp"], Convert.ToInt32(_configuration["AdminDetails:OutlookPort"])))
                        {
                            smtpClient.EnableSsl = true;
                            smtpClient.UseDefaultCredentials = false;
                            smtpClient.Credentials = loginInfo;
                            await smtpClient.SendMailAsync(mail);
                        }
                    }
                    finally
                    {
                        //dispose the client
                        mail.Dispose();
                    }
                }
            }
            catch (SmtpFailedRecipientsException ex)
            {
                foreach (var t in ex.InnerExceptions)
                {
                    var status = t.StatusCode;
                    if (status == SmtpStatusCode.MailboxBusy ||
                        status == SmtpStatusCode.MailboxUnavailable)
                    {
                        throw new ArgumentException("Delivery failed - retrying in 5 seconds.");
                    }
                    else
                    {
                        throw new ArgumentException("Failed to deliver message to {0}", t.FailedRecipient);
                    }
                }
            }
            catch (SmtpException e)
            {
                // handle exception here
                throw new ArgumentException(e.ToString());
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.ToString());
            }
        }
    }
}
