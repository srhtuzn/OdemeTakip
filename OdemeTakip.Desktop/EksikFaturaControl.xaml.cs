using OdemeTakip.Desktop.ViewModels;
using OdemeTakip.Data;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OdemeTakip.Desktop
{
    public partial class EksikFaturaControl : UserControl
    {
        private readonly ObservableCollection<EksikFaturaViewModel> _faturalar;
        private readonly AppDbContext _db;

        public EksikFaturaControl(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
            _faturalar = new ObservableCollection<EksikFaturaViewModel>();
            dgFaturalar.ItemsSource = _faturalar;
            YukleEksikFaturalar();
        }

        public EksikFaturaControl() : this(App.DbContext) { }

        private void YukleEksikFaturalar()
        {
            var buAy = DateTime.Today;
            var baslangic = new DateTime(buAy.Year, buAy.Month, 1);
            var bitis = baslangic.AddMonths(1).AddDays(-1);

            var eksikFaturalar = _db.DegiskenOdemeler
                .Where(x => x.IsActive &&
                            (x.Tutar == 0 || x.FaturaNo == null || x.FaturaNo == "") &&
                            x.OdemeTarihi >= baslangic && x.OdemeTarihi <= bitis)
                .Select(x => new EksikFaturaViewModel
                {
                    Id = x.Id,
                    GiderTuru = x.GiderTuru,
                    Aciklama = x.Aciklama ?? "",
                    Tarih = x.OdemeTarihi,
                    Tutar = x.Tutar,
                    ParaBirimi = x.ParaBirimi,
                    SirketAdi = x.Company != null ? x.Company.Name : "",
                    CariFirmaAdi = x.CariFirma != null ? x.CariFirma.Name : "",
                    FaturaNo = x.FaturaNo
                })
                .ToList();

            _faturalar.Clear();
            foreach (var item in eksikFaturalar)
                _faturalar.Add(item);
        }


        private void BtnEkle_Click(object sender, RoutedEventArgs e)
        {
            var form = new EksikFaturaForm(_db);
            if (form.ShowDialog() == true && form.Kaydedildi)
            {
                YukleEksikFaturalar();
            }
        }

        private void BtnGuncelle_Click(object sender, RoutedEventArgs e)
        {
            if (dgFaturalar.SelectedItem is EksikFaturaViewModel secili)
            {
                var form = new EksikFaturaForm(_db);
                form.LoadFromViewModel(secili);

                if (form.ShowDialog() == true && form.Kaydedildi)
                {
                    YukleEksikFaturalar();
                }
            }
        }

        private void BtnSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgFaturalar.SelectedItem is EksikFaturaViewModel secili)
            {
                if (MessageBox.Show("Seçili faturayı silmek istiyor musunuz?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    var fatura = _db.DegiskenOdemeler.FirstOrDefault(x => x.Id == secili.Id);
                    if (fatura != null)
                    {
                        _db.DegiskenOdemeler.Remove(fatura);
                        _db.SaveChanges();
                        YukleEksikFaturalar();
                    }
                }
            }
        }
    }
}
