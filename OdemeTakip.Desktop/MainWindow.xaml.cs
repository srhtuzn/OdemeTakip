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
            {
                if (App.CurrentUser != null)
                {
                    // Kullanıcı bilgilerini MainWindow'daki TextBlock'lara yükle
                    // Bu TextBlock'ların MainWindow.xaml'de tanımlı olduğundan emin olun:
                    // x:Name="LoggedInUserFullNameTextBlock" ve x:Name="LoggedInUserRoleTextBlock"
                    // Eğer bu TextBlock'lar yoksa, XAML'e eklemeniz veya bu satırları kaldırmanız gerekir.
                    // Önceki XAML önerilerimde bu TextBlock'lar vardı.
                    if (this.FindName("LoggedInUserFullNameTextBlock") is TextBlock fullNameTextBlock)
                    {
                        fullNameTextBlock.Text = App.CurrentUser.FullName ?? App.CurrentUser.Username;
                    }
                    if (this.FindName("LoggedInUserRoleTextBlock") is TextBlock roleTextBlock)
                    {
                        roleTextBlock.Text = App.CurrentUser.Role.ToString();
                    }

                    // Kullanıcı rolüne göre Kullanıcı Yönetimi butonunun/sekmesinin görünürlüğünü ayarla
                    if (App.CurrentUser.Role == UserRole.Admin)
                    {
                        // Eğer buton kullanıyorsanız (XAML'de UserManagementButton varsa):
                        if (this.FindName("UserManagementButton") is Button umButton)
                        {
                            umButton.Visibility = Visibility.Visible;
                        }
                        // VEYA doğrudan sekme ekliyorsanız:
                        AddUserManagementTabIfNeeded();
                    }
                    else
                    {
                        if (this.FindName("UserManagementButton") is Button umButton)
                        {
                            umButton.Visibility = Visibility.Collapsed;
                        }
                        // Admin değilse Kullanıcı Yönetimi sekmesi eklenmez.
                    }
                }
                else
                {
                    // Bu durum OnStartup'taki kontrolle engellenmeli, ancak bir güvenlik önlemi olarak:
                    MessageBox.Show("Oturum bilgileri yüklenemedi. Lütfen tekrar giriş yapın.", "Oturum Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                    LogoutAndShowLogin(); // Oturumu kapat ve login ekranını göster
                    return;
                }

                // Mevcut diğer Window_Loaded işlemleriniz
                var db = App.DbContext;
                BankaSeeder.Yukle(db);
                DegiskenOdemeGenerator.Uygula(db);
            }
        }
        private void AddUserManagementTabIfNeeded()
        {
            string userManagementTabName = "UserManagementTab";
            // Sekmenin zaten ekli olup olmadığını kontrol et
            if (MainTabControl.Items.OfType<TabItem>().All(ti => ti.Name != userManagementTabName))
            {
                // UserManagementPanelView'ın namespace'inin doğru olduğundan emin olun
                // Örneğin: using OdemeTakip.Desktop.ViewModels;
                UserManagementPanelView umView = new ();
                TabItem userManagementTab = new ()
                {
                    Name = userManagementTabName,
                    Header = "👤 Kullanıcı Yönetimi",
                    Content = umView
                };
                MainTabControl.Items.Add(userManagementTab);
            }
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
            App.SetCurrentUser(null!); // Kullanıcıyı temizle

            // Login ekranını aç
            LoginWindow loginWindow = new();
            bool? result = loginWindow.ShowDialog(); // Show değil ShowDialog

            if (result == true && App.CurrentUser != null)
            {
                // Başarıyla yeni kullanıcı ile giriş yapıldıysa:
                MainWindow newMainWindow = new();  // Yeni bir MainWindow oluştur
                Application.Current.MainWindow = newMainWindow;
                newMainWindow.Show();
            }

            // Eski MainWindow'u kapat
            this.Close();
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
                // TumOdemelerView için
                // Eğer tabTumOdemeler seçiliyse ve kontrol mevcutsa, ViewModel'indeki LoadPaymentsCommand'ı çağır.
                // Bu, DataContext'in TumOdemelerListViewModel olduğundan emin olduğumuz için güvenlidir.
                if (tabTumOdemeler.IsSelected && tumOdemelerViewControl != null)
                {
                    // tumOdemelerViewControl'un DataContext'i TumOdemelerListViewModel olmalı.
                    if (tumOdemelerViewControl.DataContext is TumOdemelerListViewModel tumOdemelerViewModel)
                    {
                        tumOdemelerViewModel.LoadPaymentsCommand.Execute(null);
                    }
                }

                // Diğer kontroller için de benzer şekilde ViewModel'deki YenidenYukle komutlarını çağırın.
                // Eğer bu kontroller de MVVM'ye taşındıysa, kendi ViewModel'leri üzerinden komutları olmalıdır.
                // Şimdilik sadece IsVisible kontrolünü kaldırıyorum, çünkü YenidenYukle() çağrısı genelde yeterlidir.
                if (genelOdemeControl != null /* && genelOdemeControl.IsVisible */)
                    genelOdemeControl.YenidenYukle(); // Bu metodun hala View'de olduğunu varsayarak

                if (sabitGiderControl != null /* && sabitGiderControl.IsVisible */)
                    sabitGiderControl.YenidenYukle(); // Bu metodun hala View'de olduğunu varsayarak

                if (krediControl != null /* && krediControl.IsVisible */)
                    krediControl.YenidenYukle(); // Bu metodun hala View'de olduğunu varsayarak

                if (cekControl != null /* && cekControl.IsVisible */)
                    cekControl.YenidenYukle(); // Bu metodun hala View'de olduğunu varsayarak

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