using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using OdemeTakip.Data; // App.DbContext için
using OdemeTakip.Entities; // User için

namespace OdemeTakip.Desktop
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            LoadUserSettings(); // "Beni Hatırla" ayarlarını yükle
        }

        private void LoadUserSettings()
        {
            if (Properties.Settings.Default.RememberMe)
            {
                RememberMeCheckBox.IsChecked = true;
                UsernameTextBox.Text = Properties.Settings.Default.Username;
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
                MessageBox.Show("Kullanıcı adı ve şifre boş bırakılamaz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var username = UsernameTextBox.Text.Trim();
            var password = PasswordBox.Password; // Şifreler boşluk içerebilir, trim etmeyin

            try
            {
                // Kullanıcıyı veritabanından bul
                var user = App.DbContext.Users.FirstOrDefault(u => u.Username == username);

                if (user == null)
                {
                    MessageBox.Show("Kullanıcı adı veya şifre hatalı.", "Giriş Başarısız", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Şifreyi hash'leyip veritabanındaki hash ile karşılaştır
                string hashedInputPassword = HashPassword(password);

                if (user.PasswordHash != hashedInputPassword)
                {
                    MessageBox.Show("Kullanıcı adı veya şifre hatalı.", "Giriş Başarısız", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!user.IsActive) // Kullanıcı aktif değilse
                {
                    MessageBox.Show("Bu kullanıcı hesabı aktif değil. Lütfen sistem yöneticinize başvurun.", "Giriş Engellendi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // ✨ GİRİŞ BAŞARILI: CurrentUser'ı ayarla ✨
                App.SetCurrentUser(user);

                // "Beni Hatırla" ayarlarını kaydet
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

                this.DialogResult = true; // Login penceresini başarılı sonucuyla kapat
                this.Close();
            }
            catch (Exception ex)
            {
                // Veritabanı erişimi veya başka bir hata oluşursa
                MessageBox.Show($"Giriş sırasında bir hata oluştu: {ex.Message}", "Sistem Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static string HashPassword(string password) // Bu metot public kalmalı
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

        private void ForgotPassword_Hyperlink_Click(object sender, RoutedEventArgs e) // Bu metot adını XAML'deki Click ile eşleştirin
        {
            ForgotPasswordWindow forgotPasswordWindow = new () // 'new()' kullanılabilir
            {
                Owner = this
            };
            forgotPasswordWindow.ShowDialog();
        }
    }
}