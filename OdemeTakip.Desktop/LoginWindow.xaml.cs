using System.Windows;
using System.Linq;
using OdemeTakip.Data;
using Microsoft.EntityFrameworkCore;
using OdemeTakip.Entities; // User entity burada

namespace OdemeTakip.Desktop
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            // Beni Hatırla varsa doldur
            if (Properties.Settings.Default.RememberMe && !string.IsNullOrEmpty(Properties.Settings.Default.Username))
            {
                UsernameTextBox.Text = Properties.Settings.Default.Username;
                RememberMeCheckBox.IsChecked = true;
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Kullanıcı adı ve şifre giriniz.", "Eksik Bilgi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Kullanıcı kontrolü
            var user = App.DbContext.Users.FirstOrDefault(u => u.Username == username);

            if (user == null || user.PasswordHash != HashPassword(password))
            {
                MessageBox.Show("Kullanıcı adı veya şifre hatalı.", "Giriş Başarısız", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Beni hatırla kaydet
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

            // Giriş başarılı
            this.DialogResult = true;
            this.Close();
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Parola sıfırlama için sistem yöneticinizle iletişime geçin.", "Parolamı Unuttum", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
