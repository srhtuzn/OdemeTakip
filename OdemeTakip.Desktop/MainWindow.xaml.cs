using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using OdemeTakip.Desktop.Helpers;
using OdemeTakip.Entities;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace OdemeTakip.Desktop
{
    public partial class MainWindow : Window
    {
        private readonly AppDbContext _db;

        public MainWindow()
        {
            InitializeComponent();

			_db = App.DbContext;


			Loaded += Window_Loaded;
           
        }
        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
			var db = App.DbContext;

			BankaSeeder.Yukle(db);

            // 💾 Banka havuzu yükle
            BankaSeeder.Yukle(db);

            // 💡 Şablonlar çalıştırılsın
            DegiskenOdemeGenerator.Uygula(db);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabDetayliOdeme.IsSelected && detayliOdemeControl != null)
                detayliOdemeControl.YenidenYukle();

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
                dashboardControl.YenidenYukle();
            }
        }

    }
}
