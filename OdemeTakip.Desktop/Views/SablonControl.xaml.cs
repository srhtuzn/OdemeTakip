// OdemeTakip.Desktop/SablonControl.xaml.cs
using System.Windows.Controls;
using OdemeTakip.Desktop.ViewModels; // SablonListViewModel'i kullanmak için
using OdemeTakip.Data; // AppDbContext için hala gerekli

namespace OdemeTakip.Desktop
{
    public partial class SablonControl : UserControl
    {
        public SablonControl()
        {
            InitializeComponent();
            // UserControl'ün DataContext'ini SablonListViewModel'e bağlıyoruz.
            // Bu, XAML'deki tüm bağlamaların (Binding) bu ViewModel üzerinden çalışmasını sağlar.
            // App.DbContext'in uygulamanın yaşam döngüsü boyunca erişilebilir olması önemlidir.
            this.DataContext = new SablonListViewModel(App.DbContext); // BURASI DEĞİŞTİ
        }

        // ... diğer kodlar (önceden kaldırdığımız kısımlar) ...
    }
}