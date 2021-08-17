using MSOCore;
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
        private static DateTime lastMailRun = DateTime.UtcNow.Date.AddDays(-1);

        public AutoMailer()
        {
            if (!EventLog.SourceExists("MSO Agent Service"))
                EventLog.CreateEventSource("MSO Agent Service", "Application");

            EventLog.WriteEntry("MSO Agent Service", "Starting mailer", EventLogEntryType.Information);

            mailTimer = new Timer() { Interval = FIVE_MINS };
            mailTimer.Elapsed += MailTimer_Elapsed;
            mailTimer.AutoReset = true;
            mailTimer.Enabled = true;
            mailTimer.Start();
        }

        private void MailTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DateTime oneDayAfterLast = lastMailRun.AddDays(1);
            var now = DateTime.UtcNow;
            if (now > oneDayAfterLast && now.Hour >= 8)
            {
                SendContestantEmails();
                lastMailRun = DateTime.UtcNow.Date;
            }
            if (now > oneDayAfterLast && now.Hour >= 6)
            {
                SendDataIntegrityEmails();
                lastMailRun = DateTime.UtcNow.Date;
            }
        }

        private void SendContestantEmails()
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
                    message.Body = GetMessageBody();
                    message.Subject = "MSO: Daily mail";
                    message.IsBodyHtml = true;

                    client.Send(message);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("MSO Agent Service", ex.Message, EventLogEntryType.Error);
            }
            
        }

        private void SendDataIntegrityEmails()
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

                string body = GetDataIntegrityMessageBody();
                if (string.IsNullOrEmpty(body)) return;

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

        }

        private string GetMessageBody()
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.First(x => x.Current).Id;
            var me = context.Contestants.First(x => x.Firstname == "Julia" && x.Lastname == "Hayward");
            StringBuilder body = new StringBuilder();
            body.Append("Your events today:\r\n");
            foreach (var e in me.Entrants.Where(x =>
                x.Event.Start > DateTime.Now && x.Event.Start < DateTime.Now.AddHours(24)))
            {
                body.Append(e.Event.Code + "\r\n");
            }

            body.Append("Coming up:\r\n");
            foreach (var e in me.Entrants.Where(x =>
                x.Event.Start >= DateTime.Now.AddHours(24)))
            {
                body.Append(e.Event.Code + "\r\n");
            }

            body.Append("Your Pentamind scores:\r\n");
            foreach (var e in me.Entrants.Where(x => x.OlympiadId.Value == currentOlympiad && x.Penta_Score != null))
            {
                body.Append(e.Event.Code + " got penta points: " + e.Penta_Score + "\r\n");
            }

            return body.ToString();
        }

        private string GetDataIntegrityMessageBody()
        {
            StringBuilder body = new StringBuilder();
            body.Append("Delete this when all seems OK...\r\n\r\n");
            var context = DataEntitiesProvider.Provide();
            var nationalities = context.Nationalities.Select(x => x.Name).ToList();
            var contestantNats = context.Contestants.Where(x => x.Nationality != null).Select(x => x.Nationality).ToList();
            var bad = contestantNats.Count(x => !string.IsNullOrEmpty(x) && !nationalities.Contains(x));
            body.Append($"{bad} contestants have unknown nationalities");

            return body.ToString();
        }


        public void Stop()
        {
            mailTimer.Stop();
        }
    }
}
