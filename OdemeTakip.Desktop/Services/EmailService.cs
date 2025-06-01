using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows; // MessageBox için

namespace OdemeTakip.Desktop.Services
{
    public class EmailService
    {
        // !!! DİKKAT: BU BİLGİLERİ GÜVENLİ BİR YERDEN OKUYUN (örn: appsettings.json, User Secrets) !!!
        // Örnek olarak JavaScript kodunuzdaki bilgiler kullanılmıştır.
        private const string SmtpHost = "mail.hanholding.tr";
        private const int SmtpPort = 587; // Genellikle 587 STARTTLS için kullanılır
        private const string SmtpUser = "info@hanholding.tr"; // Gönderici e-posta adresi
        private const string SmtpPass = "Yeksun.1288"; // E-posta şifresi
        private const bool EnableSsl = true; // Port 587 için genellikle true (STARTTLS)

        public static async Task SendPasswordResetCodeAsync(string toEmail, string userName, string resetCode)
        {
            try
            {
                using var client = new SmtpClient(SmtpHost, SmtpPort); 
                {
                    client.EnableSsl = EnableSsl;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(SmtpUser, SmtpPass);

                    // TLS 1.2 veya üstünü zorunlu kılmak (güvenlik için önerilir)
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;


                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(SmtpUser, "Ödeme Takip Sistemi"), // Gönderici adı
                        Subject = "Şifre Sıfırlama Kodu",
                        Body = $"Merhaba {userName},\n\nŞifrenizi sıfırlamak için kullanacağınız kod: {resetCode}\n\nBu kod 15 dakika boyunca geçerlidir.\n\nEğer bu işlemi siz başlatmadıysanız, bu e-postayı dikkate almayınız.",
                        IsBodyHtml = false, // Düz metin e-posta
                    };
                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                    MessageBox.Show("Şifre sıfırlama kodu e-posta adresinize başarıyla gönderildi.", "E-posta Gönderildi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (SmtpException smtpEx)
            {
                // SMTP özel hataları
                MessageBox.Show($"E-posta gönderilirken bir SMTP hatası oluştu: {smtpEx.Message}\nStatusCode: {smtpEx.StatusCode}", "E-posta Gönderme Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                // Detaylı loglama yapılabilir: smtpEx.ToString()
            }
            catch (Exception ex)
            {
                // Genel hatalar
                MessageBox.Show($"E-posta gönderilirken bir hata oluştu: {ex.Message}", "E-posta Gönderme Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
