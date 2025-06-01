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
        public static User? CurrentUser { get; private set; } // Giriş yapan kullanıcıyı tutar

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Bağlantı ayarları ve DbContext oluşturma (Bu kısım doğru görünüyor)
            string localIp = "192.168.1.75";
            string vpnIp = "10.0.0.1";
            string localConnection = "Server=192.168.1.75,1434;Database=OdemeTakip;User Id=sa;Password=Yeksun.1288;TrustServerCertificate=True";
            string vpnConnection = "Server=10.0.0.1,1434;Database=OdemeTakip;User Id=sa;Password=Yeksun.1288;TrustServerCertificate=True";
            string selectedConnection;

            if (TestPingWithCmd(vpnIp))
            {
                selectedConnection = vpnConnection;
            }
            else if (TestPingWithCmd(localIp))
            {
                selectedConnection = localConnection;
            }
            else
            {
                MessageBox.Show("Veritabanı sunucusuna bağlantı sağlanamadı. Program sonlandırılıyor.", "Bağlantı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
                return;
            }

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(selectedConnection)
                .Options;
            try
            {
                DbContext = new AppDbContext(options);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veritabanı bağlantısı kurulurken bir hata oluştu: {ex.Message}\nProgram sonlandırılıyor.", "Veritabanı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
                return;
            }

            // LoginWindow'u göster ve sonucu al
            try
            {
                LoginWindow loginWindow = new(); // Parantezleri ekledim
                bool? loginResult = loginWindow.ShowDialog();

                // LoginWindow kapandıktan sonra CurrentUser'ın durumunu kontrol et
                if (loginResult == true && App.CurrentUser != null)
                {
                    // Giriş başarılı ve CurrentUser set edilmiş
                    MainWindow mainWindow = new (); // Parantezleri ekledim
                    Application.Current.MainWindow = mainWindow;
                    mainWindow.Show();
                }
                else
                {
                    // Giriş başarısız, iptal edildi veya CurrentUser set edilmedi
                    if (loginResult != true) { /* Belki bir mesaj veya loglama */ }
                    else if (App.CurrentUser == null)
                    {
                        MessageBox.Show("Oturum bilgileri alınamadı. Uygulama kapatılıyor.", "Kritik Oturum Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Uygulama ana penceresi oluşturulurken bir hata oluştu: {ex.Message}", "Uygulama Başlatma Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        // Bu metot LoginWindow'dan çağrılacak
        public static void SetCurrentUser(User? user) // Nullable User kabul etmeli
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
                        Arguments = $"/c ping -n 1 -w 1000 {host}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}