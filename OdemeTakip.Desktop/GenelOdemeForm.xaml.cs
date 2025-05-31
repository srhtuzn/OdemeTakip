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

            if (_isEdit)
            {
                txtFaturaNo.Text = _odeme.FaturaNo;
                txtOdemeAdi.Text = _odeme.OdemeAdi;
                txtAciklama.Text = _odeme.Aciklama;
                txtTutar.Text = _odeme.Tutar.ToString("N2");
                dpTarih.SelectedDate = _odeme.OdemeTarihi;
                chkOdedildi.IsChecked = _odeme.IsOdedildiMi;

                cmbSirketAdi.SelectedValue = _odeme.CompanyId;

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
                .ToList(); // Şirket listesi direkt nesne olarak bağlandı
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
            _odeme.IsOdedildiMi = chkOdedildi.IsChecked == true;

            if (cmbSirketAdi.SelectedValue is int selectedId)
                _odeme.CompanyId = selectedId;

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
