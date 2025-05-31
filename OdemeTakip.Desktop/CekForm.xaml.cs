using System;
using System.Windows;
using System.Windows.Controls;
using OdemeTakip.Data;
using OdemeTakip.Entities;

namespace OdemeTakip.Desktop
{
    public partial class CekForm : Window
    {
        private readonly AppDbContext _db;
        private readonly Cek _cek;
        private readonly bool _isEdit;

        public CekForm(AppDbContext db, Cek? cek = null)
        {
            InitializeComponent();

            _db = db;
            _cek = cek ?? new Cek();
            _isEdit = cek != null;
            BankalariYukle();
            SirketleriYukle();
            CarileriYukle();

            if (_isEdit)
            {
                txtCekNo.Text = _cek.CekNumarasi;
                cmbSirket.Text = _cek.SirketAdi;
                cmbCariFirma.Text = _cek.FirmaAdi;
                cmbTur.SelectedIndex = _cek.CekTuru == "Alınan" ? 0 : 1;
                cmbBanka.Text = _cek.Banka; // ✅ ComboBox’a text olarak yazılıyor
                txtTutar.Text = _cek.Tutar.ToString();
                dpVade.SelectedDate = _cek.VadeTarihi;
                dpTahsil.SelectedDate = _cek.TahsilTarihi;
                txtNot.Text = _cek.Notlar;
                chkTahsilEdildi.IsChecked = _cek.TahsilEdildiMi;
            }
        }
        private void BankalariYukle()
        {
            cmbBanka.ItemsSource = _db.Bankalar.Where(b => b.IsActive).Select(b => b.Adi).ToList();
        }
        private string CekKoduUret()
        {
            int mevcut = _db.Cekler.Count() + 1;
            return $"Ç{mevcut.ToString("D4")}";
        }

        private void SirketleriYukle()
        {
            cmbSirket.ItemsSource = _db.Companies
                .Where(c => c.IsActive)
                .Select(c => c.Name)
                .ToList();
        }

        private void CarileriYukle()
        {
            cmbCariFirma.ItemsSource = _db.CariFirmalar
                .Where(c => c.IsActive)
                .Select(c => c.Name)
                .ToList();
        }

        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            _cek.CekNumarasi = txtCekNo.Text.Trim();
            _cek.SirketAdi = cmbSirket.Text.Trim();     // bizim şirket
            _cek.FirmaAdi = cmbCariFirma.Text.Trim();   // karşı taraf
            _cek.CekTuru = (cmbTur.SelectedItem as ComboBoxItem)?.Content.ToString();
            _cek.Banka = cmbBanka.Text.Trim();
            _cek.Notlar = txtNot.Text.Trim();
            _cek.IsActive = true;

            // Para birimi kontrolü
            var selectedCurrency = (cmbParaBirimi.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrWhiteSpace(selectedCurrency))
            {
                MessageBox.Show("Lütfen para birimi seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _cek.ParaBirimi = selectedCurrency;

            if (decimal.TryParse(txtTutar.Text, out var tutar))
                _cek.Tutar = tutar;

            if (dpVade.SelectedDate != null)
                _cek.VadeTarihi = dpVade.SelectedDate.Value;

            _cek.TahsilTarihi = dpTahsil.SelectedDate;
            _cek.TahsilEdildiMi = chkTahsilEdildi.IsChecked == true;
            if (!_isEdit)
                _cek.CekKodu = CekKoduUret();


            if (_isEdit)
                _db.Cekler.Update(_cek);
            else
                _db.Cekler.Add(_cek);

            _db.SaveChanges();
            DialogResult = true;
            Close();
        }

    }
}
