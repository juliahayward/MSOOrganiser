using System;
using System.Collections.Generic;
using System.Linq;
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
            var hashAlgorithm = new SHA256Managed();

            var inputBytes1 = System.Text.Encoding.UTF8.GetBytes(user.Salt + enteredPassword);
            var outputBytes1 = hashAlgorithm.ComputeHash(inputBytes1);
            var computedHash = Convert.ToBase64String(outputBytes1);

            return (computedHash == user.Hash);
        }
    }
}
