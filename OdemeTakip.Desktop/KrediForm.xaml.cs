using OdemeTakip.Data;
using OdemeTakip.Entities;
using System;
using System.Windows;
using System.Windows.Controls;

namespace OdemeTakip.Desktop
{
    public partial class KrediForm : Window
    {
        private readonly AppDbContext _db;
        private readonly Kredi _kredi;
        private readonly bool _isEdit;

        public KrediForm(AppDbContext db, Kredi? kredi = null)
        {
            InitializeComponent();

            _db = db;
            _kredi = kredi ?? new Kredi();
            _isEdit = kredi != null;
            BankalariYukle();
            SirketleriYukle();

            if (_isEdit)
            {
                CmbSirketAdi.Text = _kredi.SirketAdi;
                txtKonu.Text = _kredi.KrediKonusu;
                txtToplamTutar.Text = _kredi.ToplamTutar.ToString();
                txtTaksitSayisi.Text = _kredi.TaksitSayisi.ToString();
                txtAylikTaksit.Text = _kredi.AylikTaksitTutari.ToString();
                txtOdenen.Text = _kredi.OdenenTutar.ToString();
                dpBaslangic.SelectedDate = _kredi.BaslangicTarihi;
                txtNot.Text = _kredi.Notlar;
                cmbBanka.Text = _kredi.Banka;
            }
        }

        private void SirketleriYukle()
        {
            CmbSirketAdi.ItemsSource = _db.Companies
                .Where(c => c.IsActive)
                .Select(c => c.Name)
                .ToList();
        }

        private void BankalariYukle()
        {
            cmbBanka.ItemsSource = _db.Bankalar.Where(b => b.IsActive).Select(b => b.Adi).ToList();
        }

        private string KrediKoduUret()
        {
            int mevcutSayi = _db.Krediler.Count() + 1;
            return $"K{mevcutSayi.ToString("D4")}"; // Örnek: K0001
        }

        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            _kredi.SirketAdi = CmbSirketAdi.Text.Trim();
            _kredi.KrediKonusu = txtKonu.Text.Trim();
            _kredi.Notlar = txtNot.Text.Trim();
            _kredi.Banka = cmbBanka.Text.Trim();
            _kredi.IsActive = true;
            if (dpBaslangic.SelectedDate == null)
            {
                MessageBox.Show("Lütfen başlangıç tarihi seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedCurrency = (cmbParaBirimi.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (string.IsNullOrWhiteSpace(selectedCurrency))
            {
                MessageBox.Show("Lütfen para birimi seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _kredi.ParaBirimi = selectedCurrency;

            if (decimal.TryParse(txtToplamTutar.Text, out var toplam))
                _kredi.ToplamTutar = toplam;

            if (int.TryParse(txtTaksitSayisi.Text, out var taksit))
                _kredi.TaksitSayisi = taksit;

            if (decimal.TryParse(txtAylikTaksit.Text, out var aylik))
                _kredi.AylikTaksitTutari = aylik;

            if (decimal.TryParse(txtOdenen.Text, out var odenen))
                _kredi.OdenenTutar = odenen;

            if (dpBaslangic.SelectedDate != null)
                _kredi.BaslangicTarihi = dpBaslangic.SelectedDate.Value;

            _kredi.KalanTutar = _kredi.ToplamTutar - _kredi.OdenenTutar;

            if (!_isEdit)
            {
                _kredi.KrediKodu = KrediKoduUret();
                _db.Krediler.Add(_kredi);
                _db.SaveChanges(); // ID alınmalı

                // 🔄 Taksitleri otomatik oluştur (ilişki artık kuruluyor)
                for (int i = 0; i < _kredi.TaksitSayisi; i++)
                {
                    var taksitKaydi = new KrediTaksit
                    {
                        KrediKodu = _kredi.KrediKodu,
                        KrediId = _kredi.Id, // ✅ İLİŞKİ burada kuruluyor
                        TaksitNo = i + 1,
                        Tarih = _kredi.BaslangicTarihi.AddMonths(i),
                        Tutar = _kredi.AylikTaksitTutari,
                        OdenmeDurumu = false
                    };

                    _db.KrediTaksitler.Add(taksitKaydi);
                }

                _db.SaveChanges();
            }

            DialogResult = true;
            Close();
        }

    }
}
