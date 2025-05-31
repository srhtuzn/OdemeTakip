using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using System;
using System.Linq;
using System.Windows.Controls;

namespace OdemeTakip.Desktop
{
    public partial class OdemeOzetControl : UserControl
    {
        public OdemeOzetControl()
        {
            InitializeComponent();
            OdemeleriYukle();
        }

        public void OdemeleriYukle()
        {
            var db = App.DbContext;


            var bugun = DateTime.UtcNow.Date;
            var haftaBaslangic = bugun.AddDays(-(int)bugun.DayOfWeek + 1);
            var ayBaslangic = new DateTime(bugun.Year, bugun.Month, 1);
            var aySonu = ayBaslangic.AddMonths(1).AddDays(-1);

            var tum = TumOdemeler(db).ToList();

            decimal toplamBugun = tum
                .Where(o => o.Tarih.UtcDateTime.Date == bugun)
                .Sum(o => o.Tutar);

            decimal toplamHafta = tum
                .Where(o => o.Tarih.UtcDateTime.Date >= haftaBaslangic && o.Tarih.UtcDateTime.Date <= bugun)
                .Sum(o => o.Tutar);

            decimal toplamAy = tum
                .Where(o => o.Tarih.UtcDateTime.Date >= ayBaslangic && o.Tarih.UtcDateTime.Date <= aySonu)
                .Sum(o => o.Tutar);

            txtBugun.Text = $"{toplamBugun:N0} TL";
            txtHafta.Text = $"{toplamHafta:N0} TL";
            txtAy.Text = $"{toplamAy:N0} TL";
        }

        private IEnumerable<OdemeKaydi> TumOdemeler(AppDbContext db)
        {
            var sabit = db.SabitGiderler.Where(x => x.IsActive)
                .Select(x => new OdemeKaydi
                {
                    Tarih = new DateTimeOffset(x.BaslangicTarihi.ToUniversalTime()),
                    Tutar = x.Tutar
                }).ToList();

            var genel = db.GenelOdemeler.Where(x => x.IsActive)
                .Select(x => new OdemeKaydi
                {
                    Tarih = x.OdemeTarihi != null
                        ? new DateTimeOffset(x.OdemeTarihi.Value.ToUniversalTime())
                        : DateTimeOffset.MinValue,
                    Tutar = x.Tutar
                }).ToList();

            var krediKart = db.KrediKartiOdemeleri.Where(x => x.IsActive)
                .Select(x => new OdemeKaydi
                {
                    Tarih = x.OdemeTarihi != null
                        ? new DateTimeOffset(x.OdemeTarihi.Value.ToUniversalTime())
                        : DateTimeOffset.MinValue,
                    Tutar = x.Tutar
                }).ToList();

            var cekler = db.Cekler.Where(x => x.IsActive)
                .Select(x => new OdemeKaydi
                {
                    Tarih = new DateTimeOffset(x.VadeTarihi.ToUniversalTime()),
                    Tutar = x.Tutar
                }).ToList();

            var krediler = db.Krediler.Where(x => x.IsActive)
                .Select(x => new OdemeKaydi
                {
                    Tarih = new DateTimeOffset(x.BaslangicTarihi.ToUniversalTime()),
                    Tutar = x.AylikTaksitTutari
                }).ToList();

            var degisken = db.DegiskenOdemeler.Where(x => x.IsActive)
                .Select(x => new OdemeKaydi
                {
                    Tarih = new DateTimeOffset(x.OdemeTarihi.ToUniversalTime()),
                    Tutar = x.Tutar
                }).ToList();

            return sabit
                .Concat(genel)
                .Concat(krediKart)
                .Concat(cekler)
                .Concat(krediler)
                .Concat(degisken);
        }



        private class OdemeKaydi
        {
            public DateTimeOffset Tarih { get; set; }
            public decimal Tutar { get; set; }
        }
    }
}
