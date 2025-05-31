using OdemeTakip.Data;
using OdemeTakip.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;


namespace OdemeTakip.Desktop
{
    public partial class CompanyForm : Window
    {
        private readonly AppDbContext _db;
        private readonly Company _company;
        private readonly bool _isEdit;

        public CompanyForm(AppDbContext db, Company? company = null)
        {
            InitializeComponent();

            _db = db;
            _company = company ?? new Company();
            _isEdit = company != null;

            if (_isEdit)
            {
                txtName.Text = _company.Name;
                txtType.Text = _company.Type;
                txtTax.Text = _company.TaxNumber;
                txtOffice.Text = _company.TaxOffice;
                txtPhone.Text = _company.Phone;
                txtEmail.Text = _company.Email;
                txtAddress.Text = _company.Address;
                txtAuthorized.Text = _company.AuthorizedPerson;
                txtNotes.Text = _company.Notes;

                YenileBankaHesaplari(); // ✅ EKLEMEN GEREKEN SATIR
            }
        }


        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            // Girişleri al ve boşlukları temizle
            _company.Name = txtName.Text.Trim();
            _company.Type = txtType.Text.Trim();
            _company.TaxNumber = txtTax.Text.Trim();
            _company.TaxOffice = txtOffice.Text.Trim();
            _company.Phone = txtPhone.Text.Trim();
            _company.Email = txtEmail.Text.Trim();
            _company.Address = txtAddress.Text.Trim();
            _company.AuthorizedPerson = txtAuthorized.Text.Trim();
            _company.Notes = txtNotes.Text.Trim();
            _company.IsActive = true;

            try
            {
                if (!_isEdit)
                    _company.SirketKodu = SirketKoduUret();

                if (_isEdit)
                {
                    _db.Companies.Update(_company);

                    // Yeni eklenen banka hesaplarını ayrı ekle
                    foreach (var hesap in _company.BankaHesaplari)
                    {
                        if (hesap.Id == 0)
                        {
                            hesap.CompanyId = _company.Id;
                            _db.BankaHesaplari.Add(hesap);
                        }
                    }
                }
                else
                {
                    _db.Companies.Add(_company);
                }

                _db.SaveChanges();

                MessageBox.Show("Şirket başarıyla kaydedildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void BtnBankaEkle_Click(object sender, RoutedEventArgs e)
        {
            var form = new BankaHesapForm(); // birazdan yapacağız
            if (form.ShowDialog() == true)
            {
                var yeniHesap = form.Hesap;
                yeniHesap.CompanyId = _company.Id; // not: yeni kayıtsa 0 olacak
                _company.BankaHesaplari.Add(yeniHesap);
                YenileBankaHesaplari();
            }
        }
        private string SirketKoduUret()
        {
            int mevcut = _db.Companies.Count() + 1;
            return $"S{mevcut.ToString("D4")}";
        }

        private void BtnBankaGuncelle_Click(object sender, RoutedEventArgs e)
        {
            if (dgBankaHesaplari.SelectedItem is BankaHesabi secili)
            {
                var form = new BankaHesapForm(secili);
                if (form.ShowDialog() == true)
                {
                    YenileBankaHesaplari();
                }
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek için bir hesap seçin.");
            }
        }
        private void BtnBankaSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgBankaHesaplari.SelectedItem is BankaHesabi secili)
            {
                var sonuc = MessageBox.Show("Bu hesabı silmek istiyor musunuz?", "Onay", MessageBoxButton.YesNo);
                if (sonuc == MessageBoxResult.Yes)
                {
                    _company.BankaHesaplari.Remove(secili);
                    if (secili.Id != 0)
                        _db.BankaHesaplari.Remove(secili);

                    YenileBankaHesaplari();
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir hesap seçin.");
            }
        }

        private void YenileBankaHesaplari()
        {
            dgBankaHesaplari.ItemsSource = null;
            dgBankaHesaplari.ItemsSource = _company.BankaHesaplari.ToList();
        }



    }
}