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
    public partial class PasswordChangeWindow : Window
    {
        public PasswordChangeWindow()
        {
            InitializeComponent();
        }

        private void changeButton_Click(object sender, RoutedEventArgs e)
        {
            if (newPasswordBox.Password != newPasswordBox2.Password)
            {
                MessageBox.Show("New passwords don't match");
            }
            var context = DataEntitiesProvider.Provide();
            var user = context.Users.FirstOrDefault(x => x.PIN == UserId);
            if (user != null)
            {
                UpdatePassword(context, user, newPasswordBox.Password);
            }

            this.DialogResult = true;
            this.Close();
        }

        public int UserId { get; set; }

        private void UpdatePassword(DataEntities context, User user, string enteredPassword)
        {
            var hashAlgorithm = new SHA256Managed();

            var inputBytes1 = System.Text.Encoding.UTF8.GetBytes(user.Salt + enteredPassword);
            var outputBytes1 = hashAlgorithm.ComputeHash(inputBytes1);
            var computedHash = Convert.ToBase64String(outputBytes1);

            user.Hash = computedHash;
            context.SaveChanges();
        }
    }
}
