using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using OdemeTakip.Entities;

namespace OdemeTakip.Desktop
{
    public partial class KrediControl : UserControl
    {
        private AppDbContext _db;

        public KrediControl()
        {
            InitializeComponent();

            _db = App.DbContext;


            LoadKrediler();
        }

        public void YenidenYukle()
        {
            LoadKrediler();
        }

        private void LoadKrediler()
        {
            var list = _db.Krediler.ToList();
            dgKrediler.ItemsSource = list;
        }

        private void BtnEkle_Click(object sender, RoutedEventArgs e)
        {
            var form = new KrediForm(_db);
            if (form.ShowDialog() == true)
            {
                LoadKrediler();
            }
        }

        private void BtnGuncelle_Click(object sender, RoutedEventArgs e)
        {
            if (dgKrediler.SelectedItem is Kredi secili)
            {
                var form = new KrediForm(_db, secili);
                if (form.ShowDialog() == true)
                {
                    LoadKrediler();
                }
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek için bir kayıt seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgKrediler.SelectedItem is Kredi secili)
            {
                var result = MessageBox.Show("Bu krediyi silmek istediğinize emin misiniz?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _db.Krediler.Remove(secili);
                    _db.SaveChanges();
                    LoadKrediler();
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir kayıt seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
