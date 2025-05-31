using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace OdemeTakip.Desktop
{
    public partial class EnYakinOdemelerControl : UserControl
    {
        public EnYakinOdemelerControl()
        {
            InitializeComponent();
            Yukle();
        }

        public void Yukle()
        {
            var db = App.DbContext;
            var bugun = DateTime.Today;

            var sabit = db.SabitGiderler
    .Where(x => x.IsActive && !x.OdendiMi && x.BaslangicTarihi >= bugun)
    .Select(x => new OdemeView
    {
        Tarih = x.BaslangicTarihi,
        Kod = x.OdemeKodu,
        Aciklama = x.GiderAdi,
        Tutar = $"{x.Tutar:N0} {x.ParaBirimi}"
    }).ToList(); // ⬅ Buraya dikkat

            var genel = db.GenelOdemeler
                .Where(x => x.IsActive && !x.IsOdedildiMi && x.OdemeTarihi >= bugun)
                .Select(x => new OdemeView
                {
                    Tarih = x.OdemeTarihi ?? DateTime.MinValue,
                    Kod = x.OdemeKodu,
                    Aciklama = x.Aciklama,
                    Tutar = $"{x.Tutar:N0} {x.ParaBirimi}"
                }).ToList();

            var krediKarti = db.KrediKartiOdemeleri
                .Where(x => x.IsActive && !x.OdenmeDurumu && x.OdemeTarihi >= bugun)
                .Select(x => new OdemeView
                {
                    Tarih = x.OdemeTarihi ?? DateTime.MinValue,
                    Kod = x.OdemeKodu,
                    Aciklama = x.Aciklama,
                    Tutar = $"{x.Tutar:N0} TL"
                }).ToList();

            var cek = db.Cekler
                .Where(x => x.IsActive && !x.OdenmeDurumu && x.VadeTarihi >= bugun)
                .Select(x => new OdemeView
                {
                    Tarih = x.VadeTarihi,
                    Kod = x.CekKodu,
                    Aciklama = $"Çek: {x.CekNumarasi}",
                    Tutar = $"{x.Tutar:N0} {x.ParaBirimi}"
                }).ToList();

            var kredi = db.Krediler
                .Where(x => x.IsActive && !x.OdenmeDurumu && x.BaslangicTarihi >= bugun)
                .Select(x => new OdemeView
                {
                    Tarih = x.BaslangicTarihi,
                    Kod = x.KrediKodu,
                    Aciklama = x.KrediKonusu,
                    Tutar = $"{x.AylikTaksitTutari:N0} {x.ParaBirimi}"
                }).ToList();

            var degisken = db.DegiskenOdemeler
                .Where(x => x.IsActive && !x.OdenmeDurumu && x.OdemeTarihi >= bugun)
                .Select(x => new OdemeView
                {
                    Tarih = x.OdemeTarihi,
                    Kod = x.OdemeKodu,
                    Aciklama = x.GiderTuru + (!string.IsNullOrEmpty(x.Aciklama) ? $" - {x.Aciklama}" : ""),
                    Tutar = $"{x.Tutar:N0} {x.ParaBirimi}"
                }).ToList();

            // 🔁 Tümünü birleştir (artık RAM'de)
            var odemeler = sabit
                .Concat(genel)
                .Concat(krediKarti)
                .Concat(cek)
                .Concat(kredi)
                .Concat(degisken)
                .OrderBy(x => x.Tarih)
                .Take(5)
                .ToList();


            odemeler.ForEach(x => x.TarihStr = x.Tarih.ToString("dd.MM.yyyy"));

            lstOdemeler.ItemsSource = odemeler;
        }

        private class OdemeView
        {
            public DateTime Tarih { get; set; }
            public string? TarihStr { get; set; }
            public string? Kod { get; set; }
            public string? Aciklama { get; set; }
            public string? Tutar { get; set; }
        }
    }
}
