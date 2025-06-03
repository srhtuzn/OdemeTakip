using OdemeTakip.Data;
using OdemeTakip.Entities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OdemeTakip.Desktop
{
    public partial class DegiskenOdemeSablonuForm : Window
    {
        private readonly AppDbContext _db;
        private readonly DegiskenOdemeSablonu _sablon;
        private readonly bool _isEdit;

        public DegiskenOdemeSablonuForm(AppDbContext db, DegiskenOdemeSablonu? sablon = null)
        {
            InitializeComponent();

            _db = db;
            _sablon = sablon ?? new DegiskenOdemeSablonu();
            _isEdit = sablon != null;

            SirketleriYukle();
            CariFirmalariYukle();  // 🔥 Cari firmaları da yükle
            cmbParaBirimi.SelectedIndex = 0;

            if (_isEdit)
            {
                txtGiderTuru.Text = _sablon.GiderTuru;
                txtAciklama.Text = _sablon.Aciklama;
                cmbParaBirimi.Text = _sablon.ParaBirimi;
                txtGun.Text = _sablon.Gun.ToString();
                cmbSirket.SelectedValue = _sablon.CompanyId;
                cmbCariFirma.SelectedValue = _sablon.CariFirmaId;  // 🔥 Cari Firma Seçimi
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

        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            _sablon.GiderTuru = txtGiderTuru.Text.Trim();
            _sablon.Aciklama = txtAciklama.Text.Trim();
            _sablon.ParaBirimi = cmbParaBirimi.Text;
            _sablon.IsActive = true;

            if (int.TryParse(txtGun.Text, out var gun))
            {
                if (gun < 1 || gun > 28)
                {
                    MessageBox.Show("Gün değeri 1 ile 28 arasında olmalı.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                _sablon.Gun = gun;
            }
            else
            {
                MessageBox.Show("Lütfen geçerli bir gün değeri girin (1-28).", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbSirket.SelectedValue is int companyId)
                _sablon.CompanyId = companyId;

            if (cmbCariFirma.SelectedValue is int cariFirmaId)  // 🔥 Cari Firma Kaydı
                _sablon.CariFirmaId = cariFirmaId;
            else
                _sablon.CariFirmaId = null;  // Seçilmezse null'a çekiyoruz

            if (_isEdit)
            {
                // 🔥🔥 Eğer şablon güncelleniyorsa, geçmiş ödemeleri de güncelleyelim:
                var eskiOdemeler = _db.DegiskenOdemeler
                    .Where(x => x.GiderTuru == _sablon.GiderTuru && x.CompanyId == _sablon.CompanyId)
                    .ToList();

                foreach (var odeme in eskiOdemeler)
                {
                    odeme.CariFirmaId = _sablon.CariFirmaId;
                }

                _db.DegiskenOdemeSablonlari.Update(_sablon);
            }
            else
            {
                _db.DegiskenOdemeSablonlari.Add(_sablon);
            }

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
