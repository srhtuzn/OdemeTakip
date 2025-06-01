using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using OdemeTakip.Desktop.Helpers;
using OdemeTakip.Entities;         // User ve UserRole için
using OdemeTakip.Desktop.ViewModels;   // UserManagementPanelView için (namespace'i kendi yapınıza göre ayarlayın)
using System.Linq;                // FirstOrDefault gibi LINQ metotları için
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace OdemeTakip.Desktop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser != null && App.CurrentUser.Role == UserRole.Admin)
            {
                UserManagementPanelView umView = new(); // IDE0090 Düzeltmesi
                TabItem userManagementTab = new()      // IDE0090 Düzeltmesi (nesne başlatıcı ile birleşebilir)
                {
                    Name = "UserManagementTab",
                    Header = "👤 Kullanıcı Yönetimi",
                    Content = umView
                };
                MainTabControl.Items.Add(userManagementTab);
            }

            var db = App.DbContext;
            BankaSeeder.Yukle(db);
            DegiskenOdemeGenerator.Uygula(db);
        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Oturumu kapatmak istediğinizden emin misiniz?",
                                                      "Oturumu Kapat",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                LogoutAndShowLogin(); // Bu metodu daha önce tanımlamıştık
            }
        }

        // ✨ BU METODUN DAHA ÖNCE TANIMLANMIŞ OLMASI GEREKİR ✨
        // (Bir önceki cevabımda bu metodu sağlamıştım)
        private void LogoutAndShowLogin()
        {

            App.SetCurrentUser(null!); // Mevcut kullanıcı bilgisini temizle

            LoginWindow loginWindow = new ();
            loginWindow.Show(); // Yeni bir login penceresi aç

            this.Close(); // Mevcut MainWindow'u kapat
        }

        private void UserManagementButton_Click(object sender, RoutedEventArgs e)
        {
            TabItem? umTab = MainTabControl.Items.OfType<TabItem>().FirstOrDefault(ti => ti.Name == "UserManagementTab");

            if (umTab == null)
            {
                UserManagementPanelView umView = new(); // IDE0090 Düzeltmesi
                TabItem newUserManagementTab = new()  // IDE0090 Düzeltmesi (nesne başlatıcı ile birleşebilir)
                {
                    Name = "UserManagementTab",
                    Header = "👤 Kullanıcı Yönetimi",
                    Content = umView
                };
                MainTabControl.Items.Add(newUserManagementTab);
                MainTabControl.SelectedItem = newUserManagementTab;
            }
            else
            {
                MainTabControl.SelectedItem = umTab;
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl tabControl && tabControl.Name == "MainTabControl")
            {
                if (tabDetayliOdeme.IsSelected && detayliOdemeListesiControl != null)
                    detayliOdemeListesiControl.YenidenYukle();

                if (genelOdemeControl != null && genelOdemeControl.IsVisible)
                    genelOdemeControl.YenidenYukle();

                if (sabitGiderControl != null && sabitGiderControl.IsVisible)
                    sabitGiderControl.YenidenYukle();

                if (krediControl != null && krediControl.IsVisible)
                    krediControl.YenidenYukle();

                if (cekControl != null && cekControl.IsVisible)
                    cekControl.YenidenYukle();

                if (krediKartiOdemeControl != null && krediKartiOdemeControl.IsVisible)
                    krediKartiOdemeControl.YenidenYukle();

                if (tabDashboard.IsSelected && dashboardControl != null)
                {
                    dashboardControl.YenidenYukle(); // Satır 102 (IDE0059 için kontrol edin)
                }

                if (MainTabControl.SelectedItem is TabItem selectedTab && selectedTab.Name == "UserManagementTab")
                {
                    if (selectedTab.Content is UserManagementPanelView umView)
                    {
                         umView.YenidenYukle(); 
                    }
                }
            }
        }
    }
}