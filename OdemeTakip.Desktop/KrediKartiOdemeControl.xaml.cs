using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using OdemeTakip.Entities;

namespace OdemeTakip.Desktop
{
    public partial class KrediKartiOdemeControl : UserControl
    {
        private readonly AppDbContext _db;

        public KrediKartiOdemeControl()
        {
            InitializeComponent();

            _db = App.DbContext;

            Yukle();
        }

        private void LoadKrediKartiOdemeleri()
        {
            var odemeler = _db.KrediKartiOdemeleri
                .Where(x => x.IsActive)
                .OrderBy(x => x.OdemeTarihi)
                .ToList();

            dgKrediKartiOdemeleri.ItemsSource = odemeler;
        }

        public void YenidenYukle()
        {
            LoadKrediKartiOdemeleri();
        }

        private void Yukle()
        {
            dgKrediKartiOdemeleri.ItemsSource = _db.KrediKartiOdemeleri
                .Where(o => o.IsActive)
                .OrderByDescending(o => o.OdemeTarihi)
                .ToList();
        }

        private void BtnYeni_Click(object sender, RoutedEventArgs e)
        {
            var form = new KrediKartiOdemeForm(_db);
            if (form.ShowDialog() == true) Yukle();
        }

        private void BtnDuzenle_Click(object sender, RoutedEventArgs e)
        {
            if (dgKrediKartiOdemeleri.SelectedItem is KrediKartiOdeme secili)
            {
                var form = new KrediKartiOdemeForm(_db, secili);
                if (form.ShowDialog() == true) Yukle();
            }
        }

        private void BtnSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgKrediKartiOdemeleri.SelectedItem is KrediKartiOdeme secili)
            {
                var sonuc = MessageBox.Show("Bu ödemeyi silmek istiyor musunuz?", "Onay", MessageBoxButton.YesNo);
                if (sonuc == MessageBoxResult.Yes)
                {
                    _db.KrediKartiOdemeleri.Remove(secili);
                    _db.SaveChanges();
                    Yukle();
                }
            }
        }
    }
}
