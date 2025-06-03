using OdemeTakip.Data;
using OdemeTakip.Entities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OdemeTakip.Desktop
{
    public partial class DegiskenOdemeForm : Window
    {
        private readonly AppDbContext _db;
        private readonly DegiskenOdeme _odeme;
        private readonly bool _isEdit;

        public DegiskenOdemeForm(AppDbContext db, DegiskenOdeme? odeme = null)
        {
            InitializeComponent();

            _db = db;
            _odeme = odeme ?? new DegiskenOdeme();
            _isEdit = odeme != null;

            SirketleriYukle();
            CariFirmalariYukle();  // 🔥 Cari Firmaları Yükle
            cmbParaBirimi.SelectedIndex = 0;

            if (_isEdit)
            {
                txtFaturaNo.Text = _odeme.FaturaNo;
                txtTutar.Text = _odeme.Tutar.ToString("N2");
                txtAciklama.Text = _odeme.Aciklama;
                dpTarih.SelectedDate = _odeme.OdemeTarihi;
                cmbGiderTuru.Text = _odeme.GiderTuru;
                cmbParaBirimi.Text = _odeme.ParaBirimi;
                cmbSirket.SelectedValue = _odeme.CompanyId;
                cmbCariFirma.SelectedValue = _odeme.CariFirmaId; // 🔥 Cari Firma Eski Seçim
            }
        }

        private void SirketleriYukle()
        {
            cmbSirket.ItemsSource = _db.Companies
                .Where(x => x.IsActive)
                .ToList();

            cmbSirket.DisplayMemberPath = "Name";
            cmbSirket.SelectedValuePath = "Id";
        }

        private void CariFirmalariYukle()
        {
            cmbCariFirma.ItemsSource = _db.CariFirmalar
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToList();

            cmbCariFirma.DisplayMemberPath = "Name";
            cmbCariFirma.SelectedValuePath = "Id";
        }

        private string KodUret()
        {
            int adet = _db.DegiskenOdemeler.Count() + 1;
            return $"SDO{adet.ToString("D4")}";
        }

        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            _odeme.GiderTuru = cmbGiderTuru.Text.Trim();
            _odeme.FaturaNo = txtFaturaNo.Text.Trim();
            _odeme.Aciklama = txtAciklama.Text.Trim();
            _odeme.ParaBirimi = cmbParaBirimi.Text;
            _odeme.IsActive = true;

            if (decimal.TryParse(txtTutar.Text, out var tutar))
                _odeme.Tutar = tutar;

            if (dpTarih.SelectedDate != null)
                _odeme.OdemeTarihi = dpTarih.SelectedDate.Value;
            else
            {
                MessageBox.Show("Lütfen tarih seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbSirket.SelectedValue is int id)
                _odeme.CompanyId = id;

            if (cmbCariFirma.SelectedValue is int cariFirmaId)  // 🔥 Cari Firma Kaydı
                _odeme.CariFirmaId = cariFirmaId;

            if (!_isEdit)
                _odeme.OdemeKodu = KodUret();

            if (_isEdit)
                _db.DegiskenOdemeler.Update(_odeme);
            else
                _db.DegiskenOdemeler.Add(_odeme);

            _db.SaveChanges();
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
