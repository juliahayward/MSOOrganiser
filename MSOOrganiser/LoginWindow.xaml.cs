using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MSOCore;

namespace MSOOrganiser
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            var context = new DataEntities();
            var user = context.Users.FirstOrDefault(x => x.Name == usernameBox.Text);
            if (user != null && VerifyPassword(context, user, passwordBox.Password))
            {
                UserId = user.PIN;
                UserName = user.Name;
                var login = new UserLogin() { LogInDate = DateTime.UtcNow, User = user };
                user.UserLogins.Add(login);
                context.SaveChanges();
                UserLoginId = login.Id;
            }
            else
                UserId = 0;

            this.DialogResult = true;
            this.Close();
        }

        public int UserId { get; set; }
        public string UserName { get; set; }
        public int UserLoginId { get; set; }

        private void usernameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                loginButton_Click(sender, null);
            }
        }

        private void passwordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                loginButton_Click(sender, null);
            }
        }

        private bool VerifyPassword(DataEntities context, User user, string enteredPassword)
        {
            var hashAlgorithm = new SHA256Managed();
            // Prepare any users added manually
            if (user.Salt == null)
            {
                var saltBytes = new byte[8];
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                rng.GetNonZeroBytes(saltBytes);
                user.Salt = Convert.ToBase64String(saltBytes);

                var inputBytes = System.Text.Encoding.UTF8.GetBytes(user.Salt + user.Password);
                var outputBytes = hashAlgorithm.ComputeHash(inputBytes);
                user.Hash = Convert.ToBase64String(outputBytes);

                user.Password = null;

                context.SaveChanges();
            }

            var inputBytes1 = System.Text.Encoding.UTF8.GetBytes(user.Salt + enteredPassword);
            var outputBytes1 = hashAlgorithm.ComputeHash(inputBytes1);
            var computedHash = Convert.ToBase64String(outputBytes1);

            return (computedHash == user.Hash);
        }
    }
}
