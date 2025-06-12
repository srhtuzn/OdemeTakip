using OdemeTakip.Desktop.ViewModels;
using System.Collections.Generic;
using System.Windows;

namespace OdemeTakip.Desktop
{
    public partial class TaksitDetayWindow : Window
    {
        public TaksitDetayWindow(string baslik, List<OdemeViewModel> taksitler)
        {
            InitializeComponent();
            txtBaslik.Text = baslik;
            dgTaksitler.ItemsSource = taksitler;
        }

    }
}
