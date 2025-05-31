using System.Windows;

namespace OdemeTakip.Desktop
{
    public partial class SifreDogrulamaPopup : Window
    {
        private const string YoneticiSifresi = "Yeksun.1288"; // İLERİDE DB'den çekilebilir
        public bool SifreDogru { get; private set; } = false;

        public SifreDogrulamaPopup()
        {
            InitializeComponent();
        }

        private void BtnIptal_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnOnayla_Click(object sender, RoutedEventArgs e)
        {
            if (txtSifre.Password == YoneticiSifresi)
            {
                SifreDogru = true;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Şifre hatalı!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
