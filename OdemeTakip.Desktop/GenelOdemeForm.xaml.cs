using OdemeTakip.Data;
using OdemeTakip.Entities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OdemeTakip.Desktop
{
    public partial class GenelOdemeForm : Window
    {
        private readonly AppDbContext _db;
        private readonly GenelOdeme _odeme;
        private readonly bool _isEdit;

        public GenelOdemeForm(AppDbContext db, GenelOdeme? odeme = null)
        {
            InitializeComponent();

            _db = db;
            _odeme = odeme ?? new GenelOdeme();
            _isEdit = odeme != null;

            SirketleriYukle();
            CariFirmalariYukle(); // 🔥 Cari Firmalar

            if (_isEdit)
            {
                txtFaturaNo.Text = _odeme.FaturaNo;
                txtOdemeAdi.Text = _odeme.OdemeAdi;
                txtAciklama.Text = _odeme.Aciklama;
                txtTutar.Text = _odeme.Tutar.ToString("N2");
                dpTarih.SelectedDate = _odeme.OdemeTarihi;

                cmbSirketAdi.SelectedValue = _odeme.CompanyId;
                cmbCariFirma.SelectedValue = _odeme.CariFirmaId; // 🔥 Cari Firma Seçim

                foreach (ComboBoxItem item in cmbParaBirimi.Items)
                {
                    if (item.Content?.ToString() == _odeme.ParaBirimi)
                    {
                        cmbParaBirimi.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void SirketleriYukle()
        {
            cmbSirketAdi.ItemsSource = _db.Companies
                .Where(c => c.IsActive)
                .ToList();
            cmbSirketAdi.DisplayMemberPath = "Name";
            cmbSirketAdi.SelectedValuePath = "Id";
        }

        private void CariFirmalariYukle()
        {
            cmbCariFirma.ItemsSource = _db.CariFirmalar
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToList();
            cmbCariFirma.DisplayMemberPath = "Name";
            cmbCariFirma.SelectedValuePath = "Id";
        }

        private string GenelOdemeKoduUret()
        {
            int mevcut = _db.GenelOdemeler.Count() + 1;
            return $"GO{mevcut.ToString("D4")}";
        }

        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            _odeme.FaturaNo = txtFaturaNo.Text.Trim();
            _odeme.OdemeAdi = txtOdemeAdi.Text.Trim();
            _odeme.Aciklama = txtAciklama.Text.Trim();

            if (cmbSirketAdi.SelectedValue is int selectedId)
                _odeme.CompanyId = selectedId;

            if (cmbCariFirma.SelectedValue is int cariFirmaId) // 🔥 Cari Firma Kaydet
                _odeme.CariFirmaId = cariFirmaId;

            if (decimal.TryParse(txtTutar.Text, out var tutar))
                _odeme.Tutar = tutar;

            if (dpTarih.SelectedDate != null)
                _odeme.OdemeTarihi = dpTarih.SelectedDate.Value;

            _odeme.ParaBirimi = (cmbParaBirimi.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (!_isEdit)
                _odeme.OdemeKodu = GenelOdemeKoduUret();

            if (_isEdit)
                _db.GenelOdemeler.Update(_odeme);
            else
                _db.GenelOdemeler.Add(_odeme);

            _db.SaveChanges();
            DialogResult = true;
            Close();
        }
    }
}
