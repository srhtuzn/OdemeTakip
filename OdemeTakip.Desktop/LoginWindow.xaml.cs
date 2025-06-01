
// LoginWindow.xaml.cs
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using OdemeTakip.Data;
using OdemeTakip.Entities;

namespace OdemeTakip.Desktop
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            LoadUserSettings();
        }
        private void LoadUserSettings()
        {
            if (Properties.Settings.Default.RememberMe)
            {
                RememberMeCheckBox.IsChecked = true;
                UsernameTextBox.Text = Properties.Settings.Default.Username;
                // İsteğe bağlı: Kullanıcı adı doluysa şifre kutusuna odaklan
                if (!string.IsNullOrEmpty(UsernameTextBox.Text))
                {
                    PasswordBox.Focus();
                }
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text) || string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                return;
            }

            var username = UsernameTextBox.Text.Trim();
            var password = PasswordBox.Password.Trim();

            var user = App.DbContext.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                return;
            }

            string hashedInputPassword = HashPassword(password);

            if (user.PasswordHash != hashedInputPassword)
            {
                return;
            }
            App.SetCurrentUser(user);

            if (RememberMeCheckBox.IsChecked == true)
            {
                Properties.Settings.Default.RememberMe = true;
                Properties.Settings.Default.Username = username;
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.RememberMe = false;
                Properties.Settings.Default.Username = string.Empty;
                Properties.Settings.Default.Save();
            }

            this.DialogResult = true;
            this.Close();
        }

        public static string HashPassword(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

        private void ForgotPassword_Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            ForgotPasswordWindow forgotPasswordWindow = new() // IDE0090 da uygulanmış
            {
                Owner = this
                // , SomeOtherProperty = value
            };
            forgotPasswordWindow.ShowDialog();
        }
    }
}