using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using OdemeTakip.Entities;

namespace OdemeTakip.Desktop
{
    public partial class CekControl : UserControl
    {
        private readonly AppDbContext _db;

        public CekControl()
        {
            InitializeComponent();
			_db = App.DbContext;
			LoadCekler();
        }

        private void LoadCekler()
        {
            var cekler = _db.Cekler
                .Where(x => x.IsActive)
                .OrderBy(x => x.VadeTarihi)
                .ToList();

            dgCekler.ItemsSource = cekler;
        }

        public void YenidenYukle()
        {
            LoadCekler();
        }


        private void BtnEkle_Click(object sender, RoutedEventArgs e)
        {
            var form = new CekForm(_db);
            if (form.ShowDialog() == true)
            {
                LoadCekler();
            }
        }

        private void BtnGuncelle_Click(object sender, RoutedEventArgs e)
        {
            if (dgCekler.SelectedItem is Cek secili)
            {
                var form = new CekForm(_db, secili);
                if (form.ShowDialog() == true)
                {
                    LoadCekler();
                }
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek için bir çek seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgCekler.SelectedItem is Cek secili)
            {
                var result = MessageBox.Show("Bu çeki silmek istediğinize emin misiniz?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _db.Cekler.Remove(secili);
                    _db.SaveChanges();
                    LoadCekler();
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir çek seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
    