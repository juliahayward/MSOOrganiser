using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MSOAgent
{
    public class AutoMailer
    {
        private static Timer mailTimer;
        private static double FIVE_MINS = 5 * 60 * 1000;
        private static DateTime lastMailRun = DateTime.UtcNow.Date;

        public AutoMailer()
        {
            if (!EventLog.SourceExists("MSO Agent Service"))
                EventLog.CreateEventSource("MSO Agent Service", "Application");

            EventLog.WriteEntry("MSO Agent Service", "Starting mailer", EventLogEntryType.Information);

            mailTimer = new Timer() { Interval = FIVE_MINS };
            mailTimer.Elapsed += MailTimer_Elapsed;
            mailTimer.AutoReset = true;
            mailTimer.Start();
        }

        private void MailTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DateTime oneDayAfterLast = lastMailRun.AddDays(1);
            if (DateTime.UtcNow > oneDayAfterLast && DateTime.UtcNow.Hour > 7 && DateTime.UtcNow.Hour < 8)
            {
                try
                {
                    EventLog.WriteEntry("MSO Agent Service", "mailing", EventLogEntryType.Information);

                    var mailsender = ConfigurationManager.AppSettings["EmailUser"];
                    var password = ConfigurationManager.AppSettings["EmailPassword"];

                    var client = new SmtpClient();
                    client.Host = "smtp.office365.com";
                    client.Port = 587;
                    client.UseDefaultCredentials = false;
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(mailsender, password);

                    string body = $@"Today's MSO update email";

                    using (MailMessage message = new MailMessage(
                        new MailAddress(mailsender, "Mind Sports Olympiad"),
                        new MailAddress(mailsender)))
                    {
                        message.Body = body;
                        message.Subject = "MSO: Daily mail";
                        message.IsBodyHtml = true;

                        client.Send(message);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry("MSO Agent Service", ex.Message, EventLogEntryType.Error);
                }
                lastMailRun = DateTime.UtcNow.Date;
            }
        }

        public void Stop()
        {
            mailTimer.Stop();
        }
    }
}
