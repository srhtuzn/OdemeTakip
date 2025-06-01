using OdemeTakip.Desktop.Services; // EmailService için (eğer static değilse)
using OdemeTakip.Entities;        // User entity için
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;         // Brushes için
// using System.Threading.Tasks; // Eğer SendPasswordResetCodeAsync static ise _emailService üzerinden çağrılmayacak

namespace OdemeTakip.Desktop
{
    public partial class ForgotPasswordWindow : Window
    {
        // Eğer EmailService.SendPasswordResetCodeAsync static ise _emailService alanına gerek yok.
        // private readonly EmailService _emailService; 
        private User? _userToResetPassword; // Şifresi sıfırlanacak kullanıcıyı saklamak için

        public ForgotPasswordWindow()
        {
            InitializeComponent();
            // Eğer EmailService.SendPasswordResetCodeAsync static ise _emailService oluşturmaya gerek yok.
            // _emailService = new EmailService(); 
        }

        private async void SendCodeButton_Click(object sender, RoutedEventArgs e)
        {
            InfoTextBlockRequest.Text = ""; // Önceki mesajları temizle
            string emailOrUsername = EmailOrUsernameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(emailOrUsername))
            {
                InfoTextBlockRequest.Text = "Lütfen e-posta adresinizi veya kullanıcı adınızı girin.";
                InfoTextBlockRequest.Foreground = Brushes.Red;
                return;
            }

            _userToResetPassword = App.DbContext.Users
                                    .FirstOrDefault(u => u.Email == emailOrUsername || u.Username == emailOrUsername);

            if (_userToResetPassword == null)
            {
                InfoTextBlockRequest.Text = "Bu bilgilere sahip bir kullanıcı bulunamadı.";
                InfoTextBlockRequest.Foreground = Brushes.Red;
                return;
            }

            if (string.IsNullOrWhiteSpace(_userToResetPassword.Email))
            {
                InfoTextBlockRequest.Text = "Bu kullanıcı için kayıtlı bir e-posta adresi bulunmuyor. Lütfen sistem yöneticinizle iletişime geçin.";
                InfoTextBlockRequest.Foreground = Brushes.Red;
                _userToResetPassword = null;
                return;
            }

            var random = new Random();
            string resetCode = random.Next(100000, 999999).ToString();

            _userToResetPassword.PasswordResetCode = resetCode;
            _userToResetPassword.PasswordResetCodeExpiry = DateTime.UtcNow.AddMinutes(15);

            try
            {
                App.DbContext.SaveChanges();

                SendCodeButton.IsEnabled = false;
                InfoTextBlockRequest.Text = "Sıfırlama kodu gönderiliyor, lütfen bekleyin...";
                InfoTextBlockRequest.Foreground = Brushes.Blue;

                // EmailService.SendPasswordResetCodeAsync static ise doğrudan çağırın:
                await EmailService.SendPasswordResetCodeAsync(_userToResetPassword.Email, _userToResetPassword.Username, resetCode);
                // Eğer static değilse eski haliyle kalır:
                // await _emailService.SendPasswordResetCodeAsync(_userToResetPassword.Email, _userToResetPassword.Username, resetCode);

                RequestCodePanel.Visibility = Visibility.Collapsed;
                ResetPasswordPanel.Visibility = Visibility.Visible;
                InfoTextBlockReset.Text = $"'{_userToResetPassword.Email}' adresine bir sıfırlama kodu gönderildi. Lütfen kodu ve yeni şifrenizi girin.";
                InfoTextBlockReset.Foreground = Brushes.Green;
            }
            catch (Exception ex)
            {
                InfoTextBlockRequest.Text = $"Bir hata oluştu: {ex.Message}";
                InfoTextBlockRequest.Foreground = Brushes.Red;
                _userToResetPassword = null;
            }
            finally
            {
                SendCodeButton.IsEnabled = true;
            }
        }

        private void ResetPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            InfoTextBlockReset.Text = ""; // Önceki mesajları temizle
            string code = ResetCodeTextBox.Text.Trim();
            string newPassword = NewPasswordBox.Password;
            string confirmNewPassword = ConfirmNewPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmNewPassword))
            {
                InfoTextBlockReset.Text = "Lütfen tüm alanları doldurun.";
                InfoTextBlockReset.Foreground = Brushes.Red;
                return;
            }

            if (newPassword != confirmNewPassword)
            {
                InfoTextBlockReset.Text = "Yeni şifreler eşleşmiyor.";
                InfoTextBlockReset.Foreground = Brushes.Red;
                return;
            }

            // Şifre karmaşıklığı kontrolü eklenebilir (uzunluk, özel karakter vs.)

            if (_userToResetPassword == null)
            {
                InfoTextBlockReset.Text = "Kullanıcı bilgisi bulunamadı. Lütfen işlemi baştan başlatın.";
                InfoTextBlockReset.Foreground = Brushes.Red;
                // Belki de kullanıcıyı ilk adıma geri yönlendirmek daha iyi olur.
                ResetPasswordPanel.Visibility = Visibility.Collapsed;
                RequestCodePanel.Visibility = Visibility.Visible;
                EmailOrUsernameTextBox.Text = "";
                InfoTextBlockRequest.Text = "Bir sorun oluştu, lütfen tekrar deneyin.";
                InfoTextBlockRequest.Foreground = Brushes.Red;
                return;
            }

            if (_userToResetPassword.PasswordResetCode != code)
            {
                InfoTextBlockReset.Text = "Girilen sıfırlama kodu yanlış.";
                InfoTextBlockReset.Foreground = Brushes.Red;
                return;
            }

            if (_userToResetPassword.PasswordResetCodeExpiry < DateTime.UtcNow)
            {
                InfoTextBlockReset.Text = "Sıfırlama kodunun süresi dolmuş. Lütfen yeni bir kod isteyin.";
                InfoTextBlockReset.Foreground = Brushes.Red;
                _userToResetPassword.PasswordResetCode = null; // Süresi dolan kodu temizle
                _userToResetPassword.PasswordResetCodeExpiry = null;
                App.DbContext.SaveChanges();
                // Kullanıcıyı ilk adıma geri yönlendir
                ResetPasswordPanel.Visibility = Visibility.Collapsed;
                RequestCodePanel.Visibility = Visibility.Visible;
                EmailOrUsernameTextBox.Text = "";
                InfoTextBlockRequest.Text = "Kodun süresi doldu, lütfen yeni kod isteyin.";
                InfoTextBlockRequest.Foreground = Brushes.OrangeRed;
                return;
            }

            // Şifreyi güncelle
            _userToResetPassword.PasswordHash = LoginWindow.HashPassword(newPassword); // LoginWindow'daki public static metodu kullan
            _userToResetPassword.PasswordResetCode = null; // Kodu temizle
            _userToResetPassword.PasswordResetCodeExpiry = null; // Geçerlilik süresini temizle

            try
            {
                App.DbContext.SaveChanges();
                MessageBox.Show("Şifreniz başarıyla sıfırlandı. Şimdi yeni şifrenizle giriş yapabilirsiniz.", "Şifre Sıfırlandı", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true; // Pencereyi başarılı sonucuyla kapat
                this.Close();
            }
            catch (Exception ex)
            {
                InfoTextBlockReset.Text = $"Şifre güncellenirken bir hata oluştu: {ex.Message}";
                InfoTextBlockReset.Foreground = Brushes.Red;
                // Loglama yapılabilir
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}