using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;

namespace MSOCore.ApiLogic
{
    public class UserLogic
    {
        public User GetUserForLogin(string username, string password)
        {
            var context = DataEntitiesProvider.Provide();
            var user = context.Users.FirstOrDefault(x => x.Name == username);
            if (user == null) return null;

            if (!VerifyPassword(context, user, password)) return null;

            // Log the login
            var login = new UserLogin()
            {
                LogInDate = DateTime.UtcNow,
                User = user,
                Application = "Website",
                Version = Assembly.GetExecutingAssembly().GetName().Version.ToString()
            };
            user.UserLogins.Add(login);
            context.SaveChanges();
            return user;
        }

        private bool VerifyPassword(DataEntities context, User user, string enteredPassword)
        {
            var computedHash = GetHash(user.Salt + enteredPassword);
            return (computedHash == user.Hash);
        }

        private string GetHash(string input)
        {
            var hashAlgorithm = new SHA256Managed();

            var inputBytes1 = System.Text.Encoding.UTF8.GetBytes(input);
            var outputBytes1 = hashAlgorithm.ComputeHash(inputBytes1);
            return Convert.ToBase64String(outputBytes1);
        }

        public void SendUserPasswordResetLink(string userName, string url)
        {
            var context = DataEntitiesProvider.Provide();
            var user = context.Users.FirstOrDefault(x => x.Name == userName);
            if (user == null)
                return;
            string token = PasswordResetToken(user);

            var client = new SmtpClient();
            client.Host = "smtp.office365.com";
            client.Port = 587;
            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("", "");

            string body = $@"A request to reset the MSO password for user '{userName}' has been received.
 If this is expected, then go to {url}?userId={user.PIN}&token={token} where you can enter a new password.

If you did not expect this email, or this user is not you, then please ignore and delete it.";

            // Might be better to do a trello board?
            using (MailMessage message = new MailMessage(
                        new MailAddress("julia.hayward@btconnect.com", "Mind Sports Olympiad"),
                        new MailAddress(user.Email)))
            {
                message.Body = body;
                message.Subject = "MSO: Password reset";

                client.Send(message);
            }
        }

        private string PasswordResetToken(User user)
        {
            return GetHash(user.PIN + ":" + user.Salt + ":" + DateTime.UtcNow.ToString("yyyy-MM-dd"));
        }

        public void UpdateUserPassword(int userId, string token, string password)
        {
            var context = DataEntitiesProvider.Provide();
            var user = context.Users.FirstOrDefault(x => x.PIN == userId);
            if (user == null)
                throw new ArgumentOutOfRangeException("Email link was invalid.");

            string expectedToken = PasswordResetToken(user);
            if (token != expectedToken)
                throw new ArgumentOutOfRangeException("Email link invalid or expired.");

            user.Hash = GetHash(user.Salt + password);
            context.SaveChanges();
        }
    }
}
