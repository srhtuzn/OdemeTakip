using System;
using System.Windows;
using System.Security.Cryptography;
using System.Text;
using OdemeTakip.Data;
using OdemeTakip.Entities;

namespace OdemeTakip.Desktop
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Tüm alanları doldurun.", "Eksik Bilgi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Şifreler eşleşmiyor.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Kullanıcı adı benzersiz mi kontrolü
            var existingUser = App.DbContext.Users.FirstOrDefault(u => u.Username == username);
            if (existingUser != null)
            {
                MessageBox.Show("Bu kullanıcı adı zaten kullanılıyor.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Şifreyi hashle
            string passwordHash = HashPassword(password);

            // Kullanıcı oluştur
            var newUser = new User
            {
                FullName = fullName,
                Email = email,
                Username = username,
                PasswordHash = passwordHash,
                IsActive = true
            };

            App.DbContext.Users.Add(newUser);
            App.DbContext.SaveChanges();

            MessageBox.Show("Kayıt başarılı!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

            this.DialogResult = true;
            this.Close();
        }

        private static string HashPassword(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
