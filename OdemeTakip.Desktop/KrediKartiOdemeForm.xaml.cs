using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OdemeTakip.Data;
using OdemeTakip.Entities;

namespace OdemeTakip.Desktop
{
    public partial class KrediKartiOdemeForm : Window
    {
        private readonly AppDbContext _db;
        private readonly KrediKartiOdeme _odeme;
        private readonly bool _isEdit;
        private KrediKarti? _seciliKart;

        public KrediKartiOdemeForm(AppDbContext db, KrediKartiOdeme? odeme = null)
        {
            InitializeComponent();
            _db = db;
            _odeme = odeme ?? new KrediKartiOdeme();
            _isEdit = odeme != null;

            KartlariYukle();

            if (_isEdit)
                FormuDoldur();
        }

        private void KartlariYukle()
        {
            var kartlar = _db.KrediKartlari
                .Where(k => k.IsActive)
                .ToList();
            cmbKartlar.ItemsSource = kartlar;
            cmbKartlar.DisplayMemberPath = "CardName";
            cmbKartlar.SelectedValuePath = "Id";
        }

        private void CmbKartlar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _seciliKart = cmbKartlar.SelectedItem as KrediKarti;
            if (_seciliKart != null)
            {
                txtBanka.Text = _seciliKart.Banka;
                if (!_isEdit)
                    dpOdemeTarihi.SelectedDate = _seciliKart.PaymentDueDate;
            }
        }

        private void FormuDoldur()
        {
            txtAciklama.Text = _odeme.Aciklama;
            txtTutar.Text = _odeme.Tutar.ToString("N2");
            dpOdemeTarihi.SelectedDate = _odeme.OdemeTarihi;
            txtBanka.Text = _odeme.Banka;
            cmbKartlar.SelectedItem = _db.KrediKartlari.FirstOrDefault(k => k.CardName == _odeme.KartAdi);
        }

        private string KrediKartiOdemeKoduUret()
        {
            int mevcut = _db.KrediKartiOdemeleri.Count() + 1;
            return $"KKO{mevcut.ToString("D4")}";
        }

        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            if (_seciliKart == null)
            {
                MessageBox.Show("Lütfen bir kart seçin.");
                return;
            }

            _odeme.KartAdi = _seciliKart.CardName;

            if (_seciliKart.CompanyId.HasValue)
            {
                _odeme.CompanyId = _seciliKart.CompanyId;  // 🔥 Artık CompanyId atıyoruz
            }
            else
            {
                MessageBox.Show("Seçilen karta bağlı bir şirket bulunamadı.");
                return;
            }

            _odeme.Aciklama = txtAciklama.Text.Trim();
            _odeme.Banka = txtBanka.Text.Trim();
            _odeme.IsActive = true;

            if (!decimal.TryParse(txtTutar.Text, out var tutar))
            {
                MessageBox.Show("Tutar geçersiz.");
                return;
            }
            _odeme.Tutar = tutar;

            if (dpOdemeTarihi.SelectedDate == null)
            {
                MessageBox.Show("Tarih seçin.");
                return;
            }
            _odeme.OdemeTarihi = dpOdemeTarihi.SelectedDate.Value;

            if (!_isEdit)
                _odeme.OdemeKodu = KrediKartiOdemeKoduUret();

            if (_isEdit)
                _db.KrediKartiOdemeleri.Update(_odeme);
            else
                _db.KrediKartiOdemeleri.Add(_odeme);

            _db.SaveChanges();
            DialogResult = true;
            Close();
        }
    }
}
