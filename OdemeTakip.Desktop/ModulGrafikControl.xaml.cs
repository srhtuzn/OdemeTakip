using System.Windows.Controls;
using OxyPlot;
using OxyPlot.Series;
using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;

namespace OdemeTakip.Desktop
{
    public partial class ModulGrafikControl : UserControl
    {
        public ModulGrafikControl()
        {
            InitializeComponent();
            Yukle();
        }

        public void Yukle()
        {
            var db = App.DbContext;

            double sabit = db.SabitGiderler.Where(x => x.IsActive).Sum(x => (double)x.Tutar);
            double genel = db.GenelOdemeler.Where(x => x.IsActive).Sum(x => (double)x.Tutar);
            double degisken = db.DegiskenOdemeler.Where(x => x.IsActive).Sum(x => (double)x.Tutar);
            double kredi = db.Krediler.Where(x => x.IsActive).Sum(x => (double)x.AylikTaksitTutari);
            double krediKarti = db.KrediKartiOdemeleri.Where(x => x.IsActive).Sum(x => (double)x.Tutar);
            double cek = db.Cekler.Where(x => x.IsActive).Sum(x => (double)x.Tutar);

            var pieModel = new PlotModel { Title = "Ödeme Dağılımı" };

            var pieSeries = new PieSeries
            {
                StrokeThickness = 1.0,
                InsideLabelPosition = 0.8,
                AngleSpan = 360,
                StartAngle = 0,
            };

            if (sabit > 0)
                pieSeries.Slices.Add(new PieSlice("Sabit Ödeme", sabit) { IsExploded = true, Fill = OxyColors.SkyBlue });
            if (genel > 0)
                pieSeries.Slices.Add(new PieSlice("Genel Ödeme", genel) { IsExploded = true, Fill = OxyColors.PaleVioletRed });
            if (degisken > 0)
                pieSeries.Slices.Add(new PieSlice("Değişken Ödeme", degisken) { IsExploded = true, Fill = OxyColors.SeaGreen });
            if (kredi > 0)
                pieSeries.Slices.Add(new PieSlice("Kredi", kredi) { IsExploded = true, Fill = OxyColors.MediumPurple });
            if (krediKarti > 0)
                pieSeries.Slices.Add(new PieSlice("Kredi Kartı", krediKarti) { IsExploded = true, Fill = OxyColors.Orange });
            if (cek > 0)
                pieSeries.Slices.Add(new PieSlice("Çek", cek) { IsExploded = true, Fill = OxyColors.LightSlateGray });

            pieModel.Series.Add(pieSeries);

            plotView.Model = pieModel;
        }
    }
}
