using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using OdemeTakip.Entities;
using System;
using System.Linq;
using System.Windows;

namespace OdemeTakip.Desktop
{
    public partial class KrediKartiHarcamaForm : Window
    {
        private readonly AppDbContext _db;
        private readonly KrediKartiHarcama _harcama;
        private readonly bool _isEdit;

        public KrediKartiHarcamaForm(AppDbContext db, KrediKarti seciliKart, KrediKartiHarcama? harcama = null)
        {
            InitializeComponent();
            _db = db;

            if (harcama == null)
            {
                _harcama = new KrediKartiHarcama
                {
                    KrediKartiId = seciliKart.Id
                };
            }
            else
            {
                _harcama = harcama;
            }

            _isEdit = harcama != null;

            YukleKartlar();

            if (_isEdit)
                FormuDoldur();
            else
                dpHarcamaTarihi.SelectedDate = DateTime.Today;
        }

        private void TaksitleriOluştur(KrediKartiHarcama harcama)
        {
            if (harcama.TaksitSayisi <= 1)
                return;

            // Önce mevcut aktif taksitleri soft-delete et
            var eskiTaksitler = _db.KrediKartiOdemeleri
                .Where(o => o.KrediKartiHarcamaId == harcama.Id && o.IsActive)
                .ToList();

            foreach (var taksit in eskiTaksitler)
            {
                taksit.IsActive = false;
            }

            // Kredi kartını veritabanından çekiyoruz
            var krediKarti = _db.KrediKartlari
                .AsNoTracking()
                .FirstOrDefault(k => k.Id == harcama.KrediKartiId);

            if (krediKarti == null)
                throw new Exception("İlgili kredi kartı bulunamadı.");

            // Ödeme tarihlerini kredi kartının son ödeme tarihinden başlatıyoruz
            DateTime baslangicTarihi = krediKarti.PaymentDueDate;

            var taksitTutari = Math.Round(harcama.Tutar / harcama.TaksitSayisi, 2);
            decimal bakiye = harcama.Tutar;

            for (int i = 0; i < harcama.TaksitSayisi; i++)
            {
                decimal odemeTutari = (i == harcama.TaksitSayisi - 1) ? bakiye : taksitTutari;
                bakiye -= odemeTutari;

                var taksit = new KrediKartiOdeme
                {
                    KrediKartiId = harcama.KrediKartiId,
                    KrediKartiHarcamaId = harcama.Id,
                    OdemeTarihi = baslangicTarihi.AddMonths(i),
                    Tutar = odemeTutari,
                    IsActive = true,
                    OdenmeDurumu = false,
                    ToplamTaksit = harcama.TaksitSayisi,
                    TaksitNo = i + 1,
                };

                _db.KrediKartiOdemeleri.Add(taksit);
            }
        }


        private void YukleKartlar()
        {
            cmbKrediKartlari.ItemsSource = _db.KrediKartlari.Where(k => k.IsActive).OrderBy(k => k.CardName).ToList();
        }

        private void FormuDoldur()
        {
            cmbKrediKartlari.SelectedValue = _harcama.KrediKartiId;
            txtAciklama.Text = _harcama.Aciklama;
            txtTutar.Text = _harcama.Tutar.ToString("N2");
            txtTaksitSayisi.Text = _harcama.TaksitSayisi.ToString();
            dpHarcamaTarihi.SelectedDate = _harcama.HarcamaTarihi;
        }

        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            // Kredi kartı seçimi kontrolü
            if (cmbKrediKartlari.SelectedValue is not int krediKartiId)
            {
                MessageBox.Show("Lütfen bir kredi kartı seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Tutar kontrolü
            if (!decimal.TryParse(txtTutar.Text, out decimal tutar) || tutar <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir tutar girin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Taksit sayısı kontrolü
            if (!int.TryParse(txtTaksitSayisi.Text, out int taksitSayisi) || taksitSayisi < 1)
            {
                MessageBox.Show("Taksit sayısı en az 1 olmalıdır.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Harcama tarihi kontrolü
            if (dpHarcamaTarihi.SelectedDate is not DateTime harcamaTarihi)
            {
                MessageBox.Show("Lütfen bir harcama tarihi seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Model güncelle
            _harcama.KrediKartiId = krediKartiId;
            _harcama.Aciklama = txtAciklama.Text.Trim();
            _harcama.Tutar = tutar;
            _harcama.TaksitSayisi = taksitSayisi;
            _harcama.HarcamaTarihi = harcamaTarihi;

            if (_isEdit)
            {
                _db.KrediKartiHarcamalari.Update(_harcama);

                // Eski aktif taksitleri soft-delete yap
                var mevcutTaksitler = _db.KrediKartiOdemeleri
                    .Where(o => o.KrediKartiHarcamaId == _harcama.Id && o.IsActive)
                    .ToList();

                foreach (var taksit in mevcutTaksitler)
                {
                    taksit.IsActive = false;
                }
            }
            else
            {
                _db.KrediKartiHarcamalari.Add(_harcama);
            }


            _db.SaveChanges();

            // Taksitleri oluştur, eski aktif taksitler soft-delete edilir
            TaksitleriOluştur(_harcama);

            _db.SaveChanges();

            MessageBox.Show("Harcama başarıyla kaydedildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }


        private void BtnIptal_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
