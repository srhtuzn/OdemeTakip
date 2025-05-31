using OdemeTakip.Data;
using OdemeTakip.Entities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OdemeTakip.Desktop
{
    public partial class SabitGiderForm : Window
    {
        private readonly AppDbContext _db;
        private readonly SabitGider _gider;
        private readonly bool _isEdit;

        public SabitGiderForm(AppDbContext db, SabitGider? gider = null)
        {
            InitializeComponent();

            _db = db;
            _gider = gider ?? new SabitGider();
            _isEdit = gider != null;

            SirketleriYukle();

            if (_isEdit)
            {
                txtFaturaNo.Text = _gider.FaturaNo;
                txtGiderAdi.Text = _gider.GiderAdi;
                txtAciklama.Text = _gider.Aciklama;
                txtTutar.Text = _gider.Tutar.ToString("N2");
                dpBaslangic.SelectedDate = _gider.BaslangicTarihi;

                cmbSirketAdi.SelectedValue = _gider.CompanyId;

                foreach (ComboBoxItem item in cmbParaBirimi.Items)
                {
                    if (item.Content?.ToString() == _gider.ParaBirimi)
                    {
                        cmbParaBirimi.SelectedItem = item;
                        break;
                    }
                }

                foreach (ComboBoxItem item in cmbPeriyot.Items)
                {
                    if (item.Content?.ToString() == _gider.Periyot)
                    {
                        cmbPeriyot.SelectedItem = item;
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
        }

        private string SabitGiderKoduUret()
        {
            int mevcut = _db.SabitGiderler.Count() + 1;
            return $"SO{mevcut.ToString("D4")}";
        }

        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            _gider.FaturaNo = txtFaturaNo.Text.Trim();
            _gider.GiderAdi = txtGiderAdi.Text.Trim();
            _gider.Aciklama = txtAciklama.Text.Trim();

            var periyot = (cmbPeriyot.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (string.IsNullOrEmpty(periyot))
            {
                MessageBox.Show("Lütfen periyot seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _gider.Periyot = periyot;

            _gider.ParaBirimi = (cmbParaBirimi.SelectedItem as ComboBoxItem)?.Content.ToString();
            _gider.IsActive = true;
            _gider.OtomatikMi = true; // 👈 Sistem için otomatik tekrar aktif

            if (decimal.TryParse(txtTutar.Text, out var tutar))
                _gider.Tutar = tutar;

            if (dpBaslangic.SelectedDate != null)
                _gider.BaslangicTarihi = dpBaslangic.SelectedDate.Value;

            if (cmbSirketAdi.SelectedValue is int selectedId)
                _gider.CompanyId = selectedId;

            if (!_isEdit)
                _gider.OdemeKodu = SabitGiderKoduUret();

            if (_isEdit)
                _db.SabitGiderler.Update(_gider);
            else
                _db.SabitGiderler.Add(_gider);

            _db.SaveChanges();
            DialogResult = true;
            Close();
        }
    }
}
