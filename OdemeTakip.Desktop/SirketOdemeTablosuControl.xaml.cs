using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace OdemeTakip.Desktop
{
    public partial class SirketOdemeTablosuControl : UserControl
    {
        public SirketOdemeTablosuControl()
        {
            InitializeComponent();
            Yukle();
        }

        private void Yukle()
        {
            var db = App.DbContext;

            var sabit = db.SabitGiderler.Where(x => x.IsActive).ToList();
            var genel = db.GenelOdemeler.Where(x => x.IsActive).ToList();
            var kart = db.KrediKartiOdemeleri.Where(x => x.IsActive).ToList();
            var kredi = db.Krediler.Where(x => x.IsActive).ToList();
            var cek = db.Cekler.Where(x => x.IsActive).ToList();

            var sirketGruplari = new Dictionary<string, decimal>();

            foreach (var item in sabit)
                Ekle(item.Company?.Name, item.Tutar);

            foreach (var item in genel)
                Ekle(item.Company?.Name, item.Tutar);

            foreach (var item in kart)
                Ekle(item.OwnerName, item.Tutar);

            foreach (var item in kredi)
                Ekle(item.SirketAdi, item.AylikTaksitTutari);

            foreach (var item in cek)
                Ekle(item.SirketAdi, item.Tutar);

            var pieModel = new PlotModel { Title = "Şirket Bazlı Ödeme Dağılımı" };

            var pieSeries = new PieSeries
            {
                StrokeThickness = 1.0,
                InsideLabelPosition = 0.8,
                AngleSpan = 360,
                StartAngle = 0,
            };

            foreach (var kvp in sirketGruplari)
            {
                if (kvp.Value > 0)
                {
                    pieSeries.Slices.Add(new PieSlice(kvp.Key, (double)kvp.Value));
                }
            }

            pieModel.Series.Add(pieSeries);
            plotView.Model = pieModel;

            void Ekle(string? sirketAdi, decimal tutar)
            {
                if (string.IsNullOrWhiteSpace(sirketAdi)) return;
                if (!sirketGruplari.ContainsKey(sirketAdi))
                    sirketGruplari[sirketAdi] = 0;
                sirketGruplari[sirketAdi] += tutar;
            }
        }
    }
}
