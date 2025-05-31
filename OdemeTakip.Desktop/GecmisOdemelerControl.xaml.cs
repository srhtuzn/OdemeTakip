using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using OdemeTakip.Desktop.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OdemeTakip.Desktop
{
    public partial class GecmisOdemelerControl : UserControl
    {
        private List<OdemeViewModel> odemeler = new();

        public GecmisOdemelerControl()
        {
            InitializeComponent();
            Yukle();
        }

        private void Yukle()
        {
            var db = App.DbContext;

            odemeler = new();

            // Genel Ödemeler (ödendi olanlar)
            odemeler.AddRange(db.GenelOdemeler
                .Include(x => x.Company)
                .Where(x => x.IsActive && x.IsOdedildiMi)
                .Select(x => new OdemeViewModel
                {
                    Kod = x.OdemeKodu ?? "",
                    Aciklama = x.Aciklama ?? "",
                    Tarih = x.OdemeTarihi ?? DateTime.MinValue,
                    Tutar = x.Tutar,
                    ParaBirimi = x.ParaBirimi ?? "TL",
                    KaynakModul = "Genel Ödeme",
                    OdenmeDurumu = true,
                    OdemeTarihi = x.OdemeTarihi,
                    OdemeBankasi = x.OdemeBankasi,
                    SirketAdi = x.Company != null ? x.Company.Name : ""
                }));

            // Kredi Taksitleri (ödendi olanlar)
            odemeler.AddRange(db.KrediTaksitler
                .Include(x => x.Kredi)
                .Where(x => x.OdenmeDurumu)
                .Select(x => new OdemeViewModel
                {
                    Kod = $"{x.KrediKodu}-T{x.TaksitNo:D2}",
                    Aciklama = $"{x.Kredi.KrediKonusu} (Taksit {x.TaksitNo})",
                    Tarih = x.Tarih,
                    Tutar = x.Tutar,
                    ParaBirimi = x.Kredi.ParaBirimi ?? "TL",
                    KaynakModul = "Kredi",
                    OdenmeDurumu = true,
                    OdemeTarihi = x.OdenmeTarihi,
                    OdemeBankasi = x.OdemeBankasi,
                    SirketAdi = x.Kredi.SirketAdi ?? ""
                }));

            // Kredi Kartı Ödemeleri
            odemeler.AddRange(db.KrediKartiOdemeleri
                .Where(x => x.OdenmeDurumu)
                .Select(x => new OdemeViewModel
                {
                    Kod = x.OdemeKodu ?? "",
                    Aciklama = x.Aciklama ?? "",
                    Tarih = x.OdemeTarihi ?? DateTime.MinValue,
                    Tutar = x.Tutar,
                    ParaBirimi = "TL",
                    KaynakModul = "Kredi Kartı",
                    OdenmeDurumu = true,
                    OdemeTarihi = x.OdemeTarihi,
                    OdemeBankasi = x.OdemeBankasi,
                    SirketAdi = x.OwnerName
                }));

            // Çekler
            odemeler.AddRange(db.Cekler
                .Where(x => x.OdenmeDurumu)
                .Select(x => new OdemeViewModel
                {
                    Kod = x.CekKodu ?? "",
                    Aciklama = $"Çek No: {x.CekNumarasi}",
                    Tarih = x.VadeTarihi,
                    Tutar = x.Tutar,
                    ParaBirimi = x.ParaBirimi ?? "TL",
                    KaynakModul = "Çek",
                    OdenmeDurumu = true,
                    OdemeTarihi = x.TahsilTarihi,
                    OdemeBankasi = x.OdemeBankasi,
                    SirketAdi = x.CariFirma != null ? x.CariFirma.Name : ""
                }));

            // Değişken Ödemeler
            odemeler.AddRange(db.DegiskenOdemeler
                .Include(x => x.Company)
                .Where(x => x.OdenmeDurumu)
                .Select(x => new OdemeViewModel
                {
                    Kod = x.OdemeKodu,
                    Aciklama = x.GiderTuru + (string.IsNullOrWhiteSpace(x.Aciklama) ? "" : $" - {x.Aciklama}"),
                    Tarih = x.OdemeTarihi,
                    Tutar = x.Tutar,
                    ParaBirimi = x.ParaBirimi ?? "TL",
                    KaynakModul = "Değişken S. Ödeme",
                    OdenmeDurumu = true,
                    OdemeTarihi = x.OdenmeTarihi,
                    OdemeBankasi = x.OdemeBankasi,
                    SirketAdi = x.Company != null ? x.Company.Name : ""
                }));

            dgOdemeler.ItemsSource = odemeler.OrderByDescending(x => x.OdemeTarihi).ToList();
        }


        // Arama fonksiyonu aynı DetayliOdemeListesiControl gibi
        private void txtArama_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string filtre = txtArama.Text.Trim().ToLower();

            var filtrelenmis = odemeler.Where(x =>
                x.Kod.ToLower().Contains(filtre) ||
                x.Aciklama.ToLower().Contains(filtre) ||
                x.KaynakModul.ToLower().Contains(filtre)
            ).ToList();

            dgOdemeler.ItemsSource = filtrelenmis;
        }

        // Ödeme durumu buton işlevi aynen DetayliOdemeListesiControl'deki gibi
        private void BtnOdenme_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            var secili = btn?.Tag as OdemeViewModel;
            if (secili == null) return;

            // Ödeme durumu değiştirme işlemi burada yapılmaz (geçmiş liste sadece gösterim amaçlıdır)
            System.Windows.MessageBox.Show("Geçmiş ödemeler listesi sadece görüntüleme içindir.", "Bilgi", System.Windows.MessageBoxButton.OK);
        }

    }
}
