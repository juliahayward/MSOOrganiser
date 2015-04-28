using System;
using System.Collections.Generic;
using System.Linq;
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
            var user = context.Users.FirstOrDefault(x => x.Name == usernameBox.Text
                    && x.Password == passwordBox.Password);
            if (user != null)
            {
                UserId = user.PIN;
                UserName = user.Name;
            }
            else
                UserId = 0;

            this.DialogResult = true;
            this.Close();
        }

        public int UserId { get; set; }
        public string UserName { get; set; }

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
    }
}
