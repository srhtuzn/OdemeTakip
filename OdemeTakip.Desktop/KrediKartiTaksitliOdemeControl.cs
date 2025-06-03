using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OdemeTakip.Data;
using OdemeTakip.Entities;

namespace OdemeTakip.Desktop
{
    public partial class KrediKartiTaksitliOdemeControl : UserControl
    {
        private readonly AppDbContext _db;
        private KrediKarti? _seciliKart;

        public KrediKartiTaksitliOdemeControl()
        {
            InitializeComponent();
            _db = App.DbContext;
            LoadKrediKartlari();
        }

        private void LoadKrediKartlari()
        {
            var kartlar = _db.KrediKartlari
                .Where(k => k.IsActive)
                .OrderBy(k => k.CardName)
                .ToList();

            cmbKartlar.ItemsSource = kartlar;
        }

        private void CmbKartlar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _seciliKart = cmbKartlar.SelectedItem as KrediKarti;
        }

        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            if (_seciliKart == null)
            {
                MessageBox.Show("Lütfen bir kart seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtTutar.Text, out var toplamTutar))
            {
                MessageBox.Show("Geçerli bir tutar girin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtTaksitSayisi.Text, out var taksitSayisi) || taksitSayisi <= 0)
            {
                MessageBox.Show("Geçerli bir taksit sayısı girin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var baslangicTarihi = new DateTime(DateTime.Today.Year, DateTime.Today.Month, _seciliKart.PaymentDueDate.Day);


            decimal taksitTutari = Math.Round(toplamTutar / taksitSayisi, 2);

            for (int i = 0; i < taksitSayisi; i++)
            {
                var odeme = new KrediKartiOdeme
                {
                    OdemeKodu = KodUret(),
                    KartAdi = _seciliKart.CardName,
                    CompanyId = _seciliKart.CompanyId,
                    Banka = _seciliKart.Banka,
                    Tutar = taksitTutari,
                    OdemeTarihi = baslangicTarihi.AddMonths(i),
                    Aciklama = $"{txtAciklama.Text} (Taksit {i + 1}/{taksitSayisi})",
                    IsActive = true,
                    OdenmeDurumu = false
                };
                _db.KrediKartiOdemeleri.Add(odeme);
            }

            _db.SaveChanges();

            MessageBox.Show($"{taksitSayisi} adet taksitli ödeme kaydedildi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

            ClearForm();
        }

        private void ClearForm()
        {
            cmbKartlar.SelectedItem = null;
            txtTutar.Text = "";
            txtTaksitSayisi.Text = "";
            txtAciklama.Text = "";
        }

        private string KodUret()
        {
            int mevcut = _db.KrediKartiOdemeleri.Count() + 1;
            return $"KKO{mevcut.ToString("D4")}";
        }
    }
}
