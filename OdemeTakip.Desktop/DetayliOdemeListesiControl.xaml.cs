    using Microsoft.EntityFrameworkCore;
    using OdemeTakip.Data;
    using OdemeTakip.Desktop.ViewModels;
    using OdemeTakip.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    namespace OdemeTakip.Desktop
    {
        public static class PeriyotKurali
        {
            public static bool GosterilsinMi(DateTime tarih)
            {
                var simdi = DateTime.Today;
                return tarih <= simdi; // Sadece geçmiş ve bugünü göster
            }
        }

        public partial class DetayliOdemeListesiControl : UserControl
        {
            private List<OdemeViewModel> tumOdemeler = new();

            public DetayliOdemeListesiControl()
            {
                InitializeComponent();
                Yukle();
            }

            public void YenidenYukle()
            {
                Yukle();
            }

            private void Yukle()
            {
                var db = App.DbContext;

                tumOdemeler = new List<OdemeViewModel>();

                var simdi = DateTime.Today;

                // Sabit Giderler
                var sabitGiderler = db.SabitGiderler
                    .Include(x => x.Company)
                    .Where(x => x.IsActive)
                    .ToList();

                foreach (var gider in sabitGiderler)
                {
                    if (!gider.OtomatikMi)
                    {
                        if (!gider.OdendiMi)
                        {
                            tumOdemeler.Add(new OdemeViewModel
                            {
                                Kod = gider.OdemeKodu ?? "",
                                Aciklama = gider.GiderAdi ?? "",
                                Tarih = gider.BaslangicTarihi,
                                Tutar = gider.Tutar,
                                ParaBirimi = gider.ParaBirimi ?? "TL",
                                KaynakModul = "Sabit Ödeme",
                                OdenmeDurumu = gider.OdendiMi,
                                SirketAdi = gider.Company?.Name ?? "",
                                FaturaNo = gider.FaturaNo
                            });
                        }
                        continue;
                    }

                    var tarih = gider.BaslangicTarihi;
                    while (tarih <= simdi)
                    {
                        var oncekiOdeme = db.SabitGiderler
                            .FirstOrDefault(x => x.OdemeKodu == gider.OdemeKodu && x.BaslangicTarihi == tarih);

                        if (oncekiOdeme == null || !oncekiOdeme.OdendiMi)
                        {
                            tumOdemeler.Add(new OdemeViewModel
                            {
                                Kod = gider.OdemeKodu ?? "",
                                Aciklama = gider.GiderAdi ?? "",
                                Tarih = tarih,
                                Tutar = gider.Tutar,
                                ParaBirimi = gider.ParaBirimi ?? "TL",
                                KaynakModul = "Sabit Ödeme",
                                OdenmeDurumu = false,
                                SirketAdi = gider.Company?.Name ?? "",
                                FaturaNo = gider.FaturaNo
                            });
                        }

                        tarih = gider.Periyot switch
                        {
                            "Aylık" => tarih.AddMonths(1),
                            "3 Aylık" => tarih.AddMonths(3),
                            "Yıllık" => tarih.AddYears(1),
                            _ => tarih.AddMonths(1)
                        };
                    }
                }

                // Genel Ödemeler (sadece ödenmemişler)
                tumOdemeler.AddRange(db.GenelOdemeler
                    .Include(x => x.Company)
                    .Where(x => x.IsActive && !x.IsOdedildiMi)
                    .Select(x => new OdemeViewModel
                    {
                        Kod = x.OdemeKodu ?? "",
                        Aciklama = x.Aciklama ?? "",
                        Tarih = x.OdemeTarihi ?? DateTime.MinValue,
                        Tutar = x.Tutar,
                        ParaBirimi = x.ParaBirimi ?? "TL",
                        KaynakModul = "Genel Ödeme",
                        OdenmeDurumu = x.IsOdedildiMi,
                        SirketAdi = x.Company != null ? x.Company.Name : "",
                        FaturaNo = x.FaturaNo
                    }));

                // Kredi – En yakın ödenmemiş taksit (ödenmemiş ve en erken tarihli)
                var krediTaksitler = db.KrediTaksitler
                    .Include(x => x.Kredi)
                    .Where(x => x.Kredi.IsActive && !x.OdenmeDurumu)
                    .ToList();

                var enYakinTaksitler = krediTaksitler
                    .GroupBy(x => x.KrediKodu)
                    .Select(g => g.OrderBy(x => x.Tarih).First())
                    .ToList();

                foreach (var taksit in enYakinTaksitler)
                {
                    tumOdemeler.Add(new OdemeViewModel
                    {
                        Kod = $"{taksit.KrediKodu}-T{taksit.TaksitNo:D2}",
                        Aciklama = $"{taksit.Kredi.KrediKonusu} (Taksit {taksit.TaksitNo})",
                        Tarih = taksit.Tarih,
                        Tutar = taksit.Tutar,
                        ParaBirimi = taksit.Kredi.ParaBirimi ?? "TL",
                        KaynakModul = "Kredi",
                        OdenmeDurumu = taksit.OdenmeDurumu,
                        OdemeTarihi = taksit.OdenmeTarihi,
                        OdemeBankasi = taksit.OdemeBankasi,
                        SirketAdi = taksit.Kredi.SirketAdi ?? ""
                    });
                }

                // Kredi Kartı – sadece ödenmemişler
                tumOdemeler.AddRange(db.KrediKartiOdemeleri
                    .Where(x => x.IsActive && !x.OdenmeDurumu)
                    .Select(x => new OdemeViewModel
                    {
                        Kod = x.OdemeKodu ?? "",
                        Aciklama = x.Aciklama ?? "",
                        Tarih = x.OdemeTarihi ?? DateTime.MinValue,
                        Tutar = x.Tutar,
                        ParaBirimi = "TL",
                        KaynakModul = "Kredi Kartı",
                        OdenmeDurumu = x.OdenmeDurumu,
                        SirketAdi = x.OwnerName
                    }));

                // Çek – sadece ödenmemişler
                tumOdemeler.AddRange(db.Cekler
                    .Where(x => x.IsActive && !x.OdenmeDurumu)
                    .Select(x => new OdemeViewModel
                    {
                        Kod = x.CekKodu ?? "",
                        Aciklama = $"Çek No: {x.CekNumarasi}",
                        Tarih = x.VadeTarihi,
                        Tutar = x.Tutar,
                        ParaBirimi = x.ParaBirimi ?? "TL",
                        KaynakModul = "Çek",
                        OdenmeDurumu = x.OdenmeDurumu,
                        SirketAdi = x.CariFirma != null ? x.CariFirma.Name : ""
                    }));

                // Değişken Ödemeler – sadece ödenmemişler
                tumOdemeler.AddRange(db.DegiskenOdemeler
                    .Include(x => x.Company)
                    .Where(x => x.IsActive && !x.OdenmeDurumu)
                    .Select(x => new OdemeViewModel
                    {
                        Kod = x.OdemeKodu,
                        Aciklama = x.GiderTuru + (string.IsNullOrWhiteSpace(x.Aciklama) ? "" : $" - {x.Aciklama}"),
                        Tarih = x.OdemeTarihi,
                        Tutar = x.Tutar,
                        ParaBirimi = x.ParaBirimi ?? "TL",
                        KaynakModul = "Değişken S. Ödeme",
                        OdenmeDurumu = x.OdenmeDurumu,
                        OdemeTarihi = x.OdenmeTarihi,
                        OdemeBankasi = x.OdemeBankasi,
                        SirketAdi = x.Company != null ? x.Company.Name : "",
                        FaturaNo = x.FaturaNo
                    }));

                dgOdemeler.ItemsSource = tumOdemeler.OrderBy(x => x.Tarih).ToList();
            }


            private void txtArama_TextChanged(object sender, TextChangedEventArgs e)
            {
                string filtre = txtArama.Text.Trim().ToLower();

                var filtrelenmis = tumOdemeler.Where(x =>
                    x.Kod.ToLower().Contains(filtre) ||
                    x.Aciklama.ToLower().Contains(filtre) ||
                    x.KaynakModul.ToLower().Contains(filtre)
                ).ToList();

                dgOdemeler.ItemsSource = filtrelenmis;
            }

           private void BtnOdenme_Click(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        var secili = btn?.Tag as OdemeViewModel;
        if (secili == null) return;

                var db = App.DbContext;


                // GERİ ALMA
                if (secili.OdenmeDurumu)
        {
            var geriAl = MessageBox.Show(
                $"'{secili.Kod} - {secili.Aciklama}' ödemesi zaten 'Ödendi'. Bu durumu geri almak istiyor musunuz?",
                "Geri Alma Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (geriAl != MessageBoxResult.Yes) return;

            var sifrePopup = new SifreDogrulamaPopup();
            if (sifrePopup.ShowDialog() == true && sifrePopup.SifreDogru)
            {
                switch (secili.KaynakModul)
                {
                    case "Sabit Ödeme":
                        var sabit = db.SabitGiderler.FirstOrDefault(x => x.OdemeKodu == secili.Kod);
                        if (sabit != null)
                        {
                            sabit.OdendiMi = false;
                            db.SaveChanges();
                        }
                        break;

                    case "Genel Ödeme":
                        var genel = db.GenelOdemeler.FirstOrDefault(x => x.OdemeKodu == secili.Kod);
                        if (genel != null)
                        {
                            genel.IsOdedildiMi = false;
                            genel.OdemeTarihi = null;
                            genel.OdemeBankasi = null;
                            db.SaveChanges();
                        }
                        break;

                    case "Kredi Kartı":
                        var kk = db.KrediKartiOdemeleri.FirstOrDefault(x => x.OdemeKodu == secili.Kod);
                        if (kk != null)
                        {
                            kk.OdenmeDurumu = false;
                            kk.OdemeTarihi = null;
                            kk.OdemeBankasi = null;
                            db.SaveChanges();
                        }
                        break;

                    case "Çek":
                        var cek = db.Cekler.FirstOrDefault(x => x.CekKodu == secili.Kod);
                        if (cek != null)
                        {
                            cek.OdenmeDurumu = false;
                            cek.TahsilTarihi = null;
                            cek.OdemeBankasi = null;
                            db.SaveChanges();
                        }
                        break;

                    case "Kredi":
                        // Kod: "K0001-T01" -> krediKodu = "K0001", taksitNo = 1
                        var krediKod = secili.Kod.Split('-')[0];
                        int taksitNo = int.Parse(secili.Kod.Split('T')[1]);
                        var taksitGeri = db.KrediTaksitler.FirstOrDefault(x =>
                            x.KrediKodu == krediKod &&
                            x.TaksitNo == taksitNo &&
                            x.Tarih.Date == secili.Tarih.Date);

                        if (taksitGeri != null)
                        {
                            taksitGeri.OdenmeDurumu = false;
                            taksitGeri.OdenmeTarihi = null;
                            taksitGeri.OdemeBankasi = null;
                            db.SaveChanges();
                        }
                        break;

                    case "Değişken S. Ödeme":
                        var degiskenGeri = db.DegiskenOdemeler.FirstOrDefault(x =>
                            x.OdemeKodu == secili.Kod && x.OdemeTarihi == secili.Tarih);
                        if (degiskenGeri != null)
                        {
                            degiskenGeri.OdenmeDurumu = false;
                            degiskenGeri.OdenmeTarihi = null;
                            degiskenGeri.OdemeBankasi = null;
                            db.SaveChanges();
                        }
                        break;
                }

                secili.OdenmeDurumu = false;
                secili.OdemeTarihi = null;
                secili.OdemeBankasi = null;
                MessageBox.Show("Ödeme durumu başarıyla geri alındı 🔄", "Bilgi", MessageBoxButton.OK);
                dgOdemeler.Items.Refresh();
            }

            return;
        }

        // ÖDEME İŞARETLEME
        var onay = MessageBox.Show(
            $"Bu ödeme '{secili.Kod} - {secili.Aciklama}' için 'ödendi' olarak işaretlensin mi?",
            "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (onay != MessageBoxResult.Yes) return;

        List<BankaHesabi> bankaHesaplari;

        if (secili.KaynakModul == "Kredi Kartı")
        {
            bankaHesaplari = db.BankaHesaplari
                .Include(x => x.Company)
                .Where(x => x.IsActive && x.Company.Name == secili.SirketAdi)
                .ToList();
        }
        else
        {
            bankaHesaplari = db.BankaHesaplari
                .Where(x => x.IsActive)
                .ToList();
        }

        var popup = new OdemeOnayPopup(bankaHesaplari);
        if (popup.ShowDialog() == true)
        {
            secili.OdenmeDurumu = true;
            secili.OdemeTarihi = popup.SecilenTarih;
            secili.OdemeBankasi = popup.SecilenHesapKodu;

            switch (secili.KaynakModul)
            {
                case "Sabit Ödeme":
                    var sabit = db.SabitGiderler.FirstOrDefault(x => x.OdemeKodu == secili.Kod);
                    if (sabit != null)
                    {
                        sabit.OdendiMi = true;
                        db.SaveChanges();
                    }
                    break;

                case "Genel Ödeme":
                    var genel = db.GenelOdemeler.FirstOrDefault(x => x.OdemeKodu == secili.Kod);
                    if (genel != null)
                    {
                        genel.IsOdedildiMi = true;
                        genel.OdemeTarihi = secili.OdemeTarihi;
                        genel.OdemeBankasi = secili.OdemeBankasi;
                        db.SaveChanges();
                    }
                    break;

                case "Kredi Kartı":
                    var kk = db.KrediKartiOdemeleri.FirstOrDefault(x => x.OdemeKodu == secili.Kod);
                    if (kk != null)
                    {
                        kk.OdenmeDurumu = true;
                        kk.OdemeTarihi = secili.OdemeTarihi;
                        kk.OdemeBankasi = secili.OdemeBankasi;
                        db.SaveChanges();
                    }
                    break;

                case "Çek":
                    var cek = db.Cekler.FirstOrDefault(x => x.CekKodu == secili.Kod);
                    if (cek != null)
                    {
                        cek.OdenmeDurumu = true;
                        cek.TahsilTarihi = secili.OdemeTarihi;
                        cek.OdemeBankasi = secili.OdemeBankasi;
                        db.SaveChanges();
                    }
                    break;

                case "Kredi":
                    var krediKod = secili.Kod.Split('-')[0];
                    int taksitNo = int.Parse(secili.Kod.Split('T')[1]);
                    var taksit = db.KrediTaksitler.FirstOrDefault(x =>
                        x.KrediKodu == krediKod &&
                        x.TaksitNo == taksitNo &&
                        x.Tarih.Date == secili.Tarih.Date);

                    if (taksit != null)
                    {
                        taksit.OdenmeDurumu = true;
                        taksit.OdenmeTarihi = secili.OdemeTarihi;
                        taksit.OdemeBankasi = secili.OdemeBankasi;
                        db.SaveChanges();
                    }
                    break;

                case "Değişken S. Ödeme":
                    var degisken = db.DegiskenOdemeler.FirstOrDefault(x =>
                        x.OdemeKodu == secili.Kod && x.OdemeTarihi == secili.Tarih);
                    if (degisken != null)
                    {
                        degisken.OdenmeDurumu = true;
                        degisken.OdenmeTarihi = secili.OdemeTarihi;
                        degisken.OdemeBankasi = secili.OdemeBankasi;
                        db.SaveChanges();
                    }
                    break;
            }

            MessageBox.Show("Ödeme başarıyla işaretlendi ✅", "Başarılı", MessageBoxButton.OK);
            dgOdemeler.Items.Refresh();
            YenidenYukle();
        }
    }



        }
    }
