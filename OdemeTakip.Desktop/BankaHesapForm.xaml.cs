using OdemeTakip.Data;
using OdemeTakip.Entities;
using System.Windows;

namespace OdemeTakip.Desktop
{
    public partial class BankaHesapForm : Window
    {
        public BankaHesabi Hesap { get; private set; }
        private readonly bool _isEdit;

        public BankaHesapForm(BankaHesabi? hesap = null)
        {
            InitializeComponent();

            Hesap = hesap ?? new BankaHesabi();
            _isEdit = hesap != null;
            BankalariYukle();

            if (_isEdit)
            {
                cmbBankaAdi.Text = Hesap.BankaAdi;
                txtIban.Text = Hesap.Iban;
                txtHesapSahibi.Text = Hesap.HesapSahibi;
                txtKod.Text = Hesap.HesapKodu;
            }
            else
            {
                using var db = new AppDbContextFactory().CreateDbContext([]);
                int sayi = db.BankaHesaplari.Count() + 1;
                Hesap.HesapKodu = $"BH{sayi:D3}";
                txtKod.Text = Hesap.HesapKodu;
            }
        }

        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            Hesap.BankaAdi = cmbBankaAdi.Text.Trim();
            Hesap.Iban = txtIban.Text.Trim();
            Hesap.HesapSahibi = txtHesapSahibi.Text.Trim();
            // HesapKodu zaten atandı, tekrar yazmaya gerek yok

            DialogResult = true;
            Close();
        }

        private void BankalariYukle()
        {
            using var db = new AppDbContextFactory().CreateDbContext([]);
            cmbBankaAdi.ItemsSource = db.Bankalar
                .Where(b => b.IsActive)
                .Select(b => b.Adi)
                .ToList();
        }
    }
}
