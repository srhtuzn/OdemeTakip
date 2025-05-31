using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using OdemeTakip.Data;

namespace OdemeTakip.Desktop
{
    public partial class App : Application
    {
        public static AppDbContext DbContext { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string localIp = "192.168.1.75";
            string vpnIp = "10.0.0.1";

            string localConnection = "Server=192.168.1.75,1434;Database=OdemeTakip;User Id=sa;Password=Yeksun.1288;TrustServerCertificate=True";
            string vpnConnection = "Server=10.0.0.1,1434;Database=OdemeTakip;User Id=sa;Password=Yeksun.1288;TrustServerCertificate=True";

            string selectedConnection;

            if (TestPingWithCmd(vpnIp))
            {
                selectedConnection = vpnConnection;
                MessageBox.Show("Ofis dışında olduğunuz için VPN modu aktif edildi.", "Bağlantı Algılandı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (TestPingWithCmd(localIp))
            {
                selectedConnection = localConnection;
                MessageBox.Show("Ofiste olduğunuz için yerel mod aktif edildi.", "Bağlantı Algılandı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Bağlantı sağlanamadı. Program sonlandırılıyor.", "Bağlantı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
                return;
            }

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(selectedConnection)
                .Options;

            DbContext = new AppDbContext(options);

            var mainWindow = new MainWindow();
            mainWindow.Show();
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
