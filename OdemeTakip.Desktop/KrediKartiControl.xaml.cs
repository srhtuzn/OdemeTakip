using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using OdemeTakip.Entities;

namespace OdemeTakip.Desktop
{
    public partial class KrediKartiControl : UserControl
    {
        private AppDbContext _db;

        public KrediKartiControl()
        {
            InitializeComponent();

            _db = App.DbContext;


            LoadKrediKartlari();
        }

        public void YenidenYukle()
        {
            LoadKrediKartlari();
        }

        private void LoadKrediKartlari()
        {
            var list = _db.KrediKartlari.ToList();
            dgKrediKartlari.ItemsSource = list;
        }

        private void BtnEkle_Click(object sender, RoutedEventArgs e)
        {
            var form = new KrediKartiForm(_db);
            if (form.ShowDialog() == true)
            {
                LoadKrediKartlari();
            }
        }

        private void BtnGuncelle_Click(object sender, RoutedEventArgs e)
        {
            if (dgKrediKartlari.SelectedItem is KrediKarti secili)
            {
                var form = new KrediKartiForm(_db, secili);
                if (form.ShowDialog() == true)
                {
                    LoadKrediKartlari();
                }
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek için bir kart seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgKrediKartlari.SelectedItem is KrediKarti secili)
            {
                var sonuc = MessageBox.Show("Bu kredi kartını silmek istediğinize emin misiniz?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (sonuc == MessageBoxResult.Yes)
                {
                    _db.KrediKartlari.Remove(secili);
                    _db.SaveChanges();
                    LoadKrediKartlari();
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir kart seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
