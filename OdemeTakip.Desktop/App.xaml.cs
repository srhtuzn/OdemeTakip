using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using OdemeTakip.Entities; // User sınıfınızın namespace'i
using System.Diagnostics;
using System.Windows;

namespace OdemeTakip.Desktop
{
    public partial class App : Application
    {
        public static AppDbContext DbContext { get; private set; } = null!;

        // ✨ CurrentUser özelliğini nullable (User?) yapın ✨
        public static User? CurrentUser { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string localIp = "192.168.1.75"; // Bu IP adreslerini yapılandırma dosyasından okumak daha iyi bir pratik olabilir
            string vpnIp = "10.0.0.1";

            // Bağlantı cümlelerindeki şifreleri doğrudan koda yazmak güvenlik riski oluşturur.
            // Güvenli bir yapılandırma yöntemi (örn: User Secrets, Azure Key Vault) kullanmayı düşünün.
            string localConnection = "Server=192.168.1.75,1434;Database=OdemeTakip;User Id=sa;Password=Yeksun.1288;TrustServerCertificate=True";
            string vpnConnection = "Server=10.0.0.1,1434;Database=OdemeTakip;User Id=sa;Password=Yeksun.1288;TrustServerCertificate=True";

            string selectedConnection;

            if (TestPingWithCmd(vpnIp))
            {
                selectedConnection = vpnConnection;
                // Geliştirme aşamasında MessageBox'lar faydalı olabilir, ancak canlıda kaldırılmalı veya loglanmalı.
                // MessageBox.Show("Ofis dışında olduğunuz için VPN modu aktif edildi.", "Bağlantı Bilgisi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (TestPingWithCmd(localIp))
            {
                selectedConnection = localConnection;
                // MessageBox.Show("Ofiste olduğunuz için yerel mod aktif edildi.", "Bağlantı Bilgisi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Veritabanı sunucusuna bağlantı sağlanamadı. Lütfen ağ bağlantınızı kontrol edin veya sistem yöneticinize başvurun. Program sonlandırılıyor.", "Bağlantı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
                return;
            }

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(selectedConnection)
                .Options;

            try
            {
                DbContext = new AppDbContext(options);
                // Opsiyonel: Veritabanı bağlantısını test etmek için küçük bir sorgu çalıştırılabilir.
                // DbContext.Database.CanConnect(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veritabanı bağlantısı kurulurken bir hata oluştu: {ex.Message}\nProgram sonlandırılıyor.", "Veritabanı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
                return;
            }

            // --- TEST AMAÇLI KULLANICI ROL GÜNCELLEME KODU BURADAYSA KALDIRILMALI VEYA YORUMLANMALI ---
            // (Bir önceki mesajlarda "sero" kullanıcısını admin yapma kodundan bahsetmiştik,
            //  eğer kalıcı bir çözüm uygulandıysa bu kod burada olmamalıdır.)
            // --- TEST KODU SONU ---

            try
            {
                LoginWindow loginWindow = new ();
                bool? loginResult = loginWindow.ShowDialog();

                // ✨ LoginWindow kapandıktan sonra CurrentUser'ın durumunu kontrol et ✨
                if (loginResult == true && App.CurrentUser != null)
                {
                    // Giriş başarılı ve CurrentUser set edilmiş
                    var mainWindow = new MainWindow();
                    Application.Current.MainWindow = mainWindow; // Ana pencereyi ayarla
                    mainWindow.Show();
                }
                else
                {
                    // Giriş başarısız, iptal edildi veya CurrentUser set edilmedi (LoginWindow'da bir eksiklik olabilir)
                    if (loginResult != true)
                    {
                        // MessageBox.Show("Giriş işlemi iptal edildi veya başarısız oldu. Uygulama kapatılıyor.", "Giriş Sonucu", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (App.CurrentUser == null)
                    {
                        // Bu durum beklenmemeli, LoginWindow'da App.SetCurrentUser çağrılmamış demektir.
                        MessageBox.Show("Oturum bilgileri alınamadı. Uygulama kapatılıyor.", "Kritik Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Uygulama ana penceresi oluşturulurken bir hata oluştu: {ex.Message}", "Uygulama Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        // ✨ SetCurrentUser metodunun parametresini nullable (User?) yapın ✨
        public static void SetCurrentUser(User? user)
        {
            CurrentUser = user;
        }

        private static bool TestPingWithCmd(string host)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c ping -n 1 -w 1000 {host}", // 1 paket, 1000ms timeout
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit(); // İşlemin bitmesini bekle
                return process.ExitCode == 0;
            }
            catch
            {
                // Ping işlemi sırasında bir hata oluşursa (örn: yetki sorunu, cmd bulunamaması) false dön
                return false;
            }
        }
    }
}