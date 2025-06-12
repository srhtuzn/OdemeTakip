// OdemeTakip.Desktop/KrediKartiAnaView.xaml.cs
using System.Windows.Controls;
using OdemeTakip.Data;
using OdemeTakip.Desktop.ViewModels;
using System.Windows; // RoutedEventArgs için

namespace OdemeTakip.Desktop
{
    /// <summary>
    /// KrediKartiAnaView.xaml için code-behind sınıfı.
    /// MVVM prensiplerine göre sadeleştirilmiştir. Tüm iş mantığı KrediKartiAnaListViewModel'de bulunur.
    /// </summary>
    public partial class KrediKartiAnaView : UserControl
    {
        /// <summary>
        /// KrediKartiAnaView sınıfının yeni bir örneğini başlatır.
        /// Kullanıcı kontrolünün DataContext'ini KrediKartiAnaListViewModel'e ayarlar.
        /// </summary>
        public KrediKartiAnaView()
        {
            InitializeComponent();
            this.DataContext = new KrediKartiAnaListViewModel(App.DbContext);
        }

        /// <summary>
        /// UserControl yüklendiğinde tetiklenen olay işleyicisi.
        /// ViewModel'deki kart listesini yükleme komutunu tetikler.
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is KrediKartiAnaListViewModel viewModel)
            {
                viewModel.LoadKrediKartlariCommand.Execute(null);
            }
        }

       
    }
}