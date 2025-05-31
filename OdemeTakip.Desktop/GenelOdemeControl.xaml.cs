using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using OdemeTakip.Entities;

namespace OdemeTakip.Desktop
{
    public partial class GenelOdemeControl : UserControl
    {
        private AppDbContext _db;

        public GenelOdemeControl()
        {
            InitializeComponent();

            _db = App.DbContext;

            LoadGenelOdemeler();
        }

        public void YenidenYukle()
        {
            LoadGenelOdemeler();
        }

        private void LoadGenelOdemeler()
        {
            var list = _db.GenelOdemeler
                .Where(x => x.IsActive)
                .OrderBy(o => o.OdemeTarihi)
                .ToList();

            dgGenelOdemeler.ItemsSource = list;
        }

        private void BtnEkle_Click(object sender, RoutedEventArgs e)
        {
            var form = new GenelOdemeForm(_db);
            if (form.ShowDialog() == true)
                LoadGenelOdemeler();
        }

        private void BtnGuncelle_Click(object sender, RoutedEventArgs e)
        {
            if (dgGenelOdemeler.SelectedItem is GenelOdeme secili)
            {
                var form = new GenelOdemeForm(_db, secili);
                if (form.ShowDialog() == true)
                    LoadGenelOdemeler();
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek için bir kayıt seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgGenelOdemeler.SelectedItem is GenelOdeme secili)
            {
                var result = MessageBox.Show("Bu genel ödemeyi silmek istediğinize emin misiniz?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _db.GenelOdemeler.Remove(secili);
                    _db.SaveChanges();
                    LoadGenelOdemeler();
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir kayıt seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
