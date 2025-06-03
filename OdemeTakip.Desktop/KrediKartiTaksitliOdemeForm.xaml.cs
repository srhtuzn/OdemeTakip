using System;
using System.Linq;
using System.Windows;
using OdemeTakip.Data;
using OdemeTakip.Entities;

namespace OdemeTakip.Desktop
{
    public partial class KrediKartiTaksitliOdemeForm : Window
    {
        private readonly AppDbContext _db;

        public KrediKartiTaksitliOdemeForm(AppDbContext db)
        {
            InitializeComponent();
            _db = db;

            // Kartları Yükle
            cmbKart.ItemsSource = _db.KrediKartlari.Where(k => k.IsActive).ToList();
        }

        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            if (cmbKart.SelectedItem is not KrediKarti seciliKart)
            {
                MessageBox.Show("Kart seçin.");
                return;
            }

            if (!decimal.TryParse(txtToplamTutar.Text, out var toplamTutar))
            {
                MessageBox.Show("Geçersiz tutar.");
                return;
            }

            if (!int.TryParse(txtTaksitSayisi.Text, out var taksitSayisi) || taksitSayisi <= 0)
            {
                MessageBox.Show("Geçersiz taksit sayısı.");
                return;
            }

            if (dpIlkOdemeTarihi.SelectedDate is not DateTime ilkOdemeTarihi)
            {
                MessageBox.Show("İlk ödeme tarihini seçin.");
                return;
            }

            decimal taksitTutar = Math.Round(toplamTutar / taksitSayisi, 2);

            for (int i = 0; i < taksitSayisi; i++)
            {
                var odeme = new KrediKartiOdeme
                {
                    OdemeKodu = $"KKO{_db.KrediKartiOdemeleri.Count() + 1 + i:D4}",
                    KartAdi = seciliKart.CardName,
                    Banka = seciliKart.Banka,
                    CompanyId = seciliKart.CompanyId,
                    Aciklama = $"{taksitSayisi} Taksitli Harcama - {i + 1}. Taksit",
                    Tutar = taksitTutar,
                    OdemeTarihi = ilkOdemeTarihi.AddMonths(i),
                    IsActive = true,
                    TaksitNo = i + 1,
                    ToplamTaksit = taksitSayisi,
                    IlkOdemeTarihi = ilkOdemeTarihi
                };

                _db.KrediKartiOdemeleri.Add(odeme);
            }

            _db.SaveChanges();
            MessageBox.Show("Taksitli ödeme başarıyla eklendi!");
            DialogResult = true;
            Close();
        }
    }
}
