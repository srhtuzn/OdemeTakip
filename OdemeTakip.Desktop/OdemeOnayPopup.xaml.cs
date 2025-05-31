using OdemeTakip.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace OdemeTakip.Desktop
{
    public partial class OdemeOnayPopup : Window
    {
        public DateTime? SecilenTarih { get; private set; }
        public string? SecilenHesapKodu { get; private set; }

        public OdemeOnayPopup(List<BankaHesabi> hesaplar)
        {
            InitializeComponent();
            dpTarih.SelectedDate = DateTime.Today;

            var liste = hesaplar.Select(h => new BankaItem
            {
                HesapKodu = h.HesapKodu,
                Gosterim = $"{h.HesapKodu} - {h.BankaAdi} - {h.Iban}"
            }).ToList();

            cmbBankaKodu.ItemsSource = liste;
            cmbBankaKodu.DisplayMemberPath = "Gosterim";
            cmbBankaKodu.SelectedValuePath = "HesapKodu";
            cmbBankaKodu.SelectedIndex = 0;
        }

        private void BtnIptal_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnOnayla_Click(object sender, RoutedEventArgs e)
        {
            if (dpTarih.SelectedDate == null || cmbBankaKodu.SelectedItem == null)
            {
                MessageBox.Show("Tarih ve banka hesabı seçilmelidir!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SecilenTarih = dpTarih.SelectedDate;
            SecilenHesapKodu = cmbBankaKodu.SelectedValue?.ToString();

            DialogResult = true;
            Close();
        }

        public class BankaItem
        {
            public string HesapKodu { get; set; } = "";
            public string Gosterim { get; set; } = "";
        }
    }
}
