using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OdemeTakip.Desktop
{
    public partial class MiniTakvimControl : UserControl
    {
        public MiniTakvimControl()
        {
            InitializeComponent();
            Yukle();
        }

        private class TarihTutar
        {
            public DateTime Tarih { get; set; }
            public decimal Tutar { get; set; }
        }

        public void Yukle()
        {
            var db = App.DbContext;


            var bugun = DateTime.Today;
            var gunler = Enumerable.Range(0, 30).Select(offset => bugun.AddDays(offset)).ToList(); // 30 günlük görünüm

            var odemeler = new List<TarihTutar>();

            // 📘 Sabit Giderler
            odemeler.AddRange(db.SabitGiderler
                .Where(x => x.IsActive && !x.OdendiMi && x.BaslangicTarihi >= bugun)
                .Select(x => new TarihTutar { Tarih = x.BaslangicTarihi.Date, Tutar = x.Tutar }));

            // 📘 Genel Ödemeler
            odemeler.AddRange(db.GenelOdemeler
                .Where(x => x.IsActive && !x.IsOdedildiMi && x.OdemeTarihi >= bugun)
                .Select(x => new TarihTutar { Tarih = (x.OdemeTarihi ?? DateTime.MinValue).Date,
                    Tutar = x.Tutar }));

            // 📘 Kredi Kartı
            odemeler.AddRange(db.KrediKartiOdemeleri
                .Where(x => x.IsActive && !x.OdenmeDurumu && x.OdemeTarihi >= bugun)
                .Select(x => new TarihTutar { Tarih = (x.OdemeTarihi ?? DateTime.MinValue).Date
,
                    Tutar = x.Tutar }));

            // 📘 Çekler
            odemeler.AddRange(db.Cekler
                .Where(x => x.IsActive && !x.OdenmeDurumu && x.VadeTarihi >= bugun)
                .Select(x => new TarihTutar { Tarih = x.VadeTarihi.Date, Tutar = x.Tutar }));

            // 📘 Krediler
            odemeler.AddRange(db.Krediler
                .Where(x => x.IsActive && !x.OdenmeDurumu && x.BaslangicTarihi >= bugun)
                .Select(x => new TarihTutar { Tarih = x.BaslangicTarihi.Date, Tutar = x.AylikTaksitTutari }));

            // 📘 Değişken Giderler
            odemeler.AddRange(db.DegiskenOdemeler
                .Where(x => x.IsActive && !x.OdenmeDurumu && x.OdemeTarihi >= bugun)
                .Select(x => new TarihTutar { Tarih = x.OdemeTarihi.Date, Tutar = x.Tutar }));

            // 🧱 Kutular oluşturuluyor
            var kutular = new List<Border>();

            foreach (var tarih in gunler)
            {
                var toplam = odemeler
                    .Where(o => o.Tarih == tarih)
                    .Sum(o => o.Tutar);

                var border = new Border
                {
                    Width = 100,
                    Height = 100,
                    Margin = new Thickness(5),
                    Background = tarih == bugun ? Brushes.Orange : Brushes.LightGray,
                    CornerRadius = new CornerRadius(10),
                    Child = new StackPanel
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = tarih.ToString("dd MMM"),
                                FontWeight = FontWeights.Bold,
                                FontSize = 14,
                                TextAlignment = TextAlignment.Center
                            },
                            new TextBlock
                            {
                                Text = toplam > 0 ? $"{toplam:N0} TL" : "-",
                                FontSize = 12,
                                TextAlignment = TextAlignment.Center
                            }
                        }
                    }
                };

                kutular.Add(border);
            }

            lstGunler.ItemsSource = kutular;
        }
    }
}
