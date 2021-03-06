using MSOCore;
using MSOOrganiser.UIUtilities;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;

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

        public LoginWindow(string loginName)
        {
            InitializeComponent();

            // TODO - turn into proper bound VM
            usernameBox.Text = loginName;
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            using (new SpinnyCursor())
            {
                // this is not good - should pass all stuff out of dialog and 
                DataEntitiesProvider.IsProduction = !testData.IsChecked.Value;

                var context = DataEntitiesProvider.Provide();
                var user = context.Users.FirstOrDefault(x => x.Name == usernameBox.Text);
                if (user != null && VerifyPassword(context, user, passwordBox.Password))
                {
                    UserId = user.PIN;
                    UserName = user.Name;
                    var login = new UserLogin()
                    {
                        LogInDate = DateTime.UtcNow,
                        User = user,
                        Application = Assembly.GetExecutingAssembly().GetName().Name,
                        Version = Assembly.GetExecutingAssembly().GetName().Version.ToString()
                    };
                    user.UserLogins.Add(login);
                    context.SaveChanges();
                    UserLoginId = login.Id;
                    UseTestData = testData.IsChecked.Value;
                }
                else
                {
                    UserId = 0;
                    UserName = "";
                }

                this.DialogResult = true;
                this.Close();
            }
        }

        public int UserId { get; set; }
        public string UserName { get; set; }
        public int UserLoginId { get; set; }
        public bool UseTestData { get; set; }

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
