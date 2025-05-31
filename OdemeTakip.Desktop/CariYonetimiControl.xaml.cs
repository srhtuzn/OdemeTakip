using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OdemeTakip.Desktop
{
    public partial class CariYonetimiControl : UserControl
    {
        private readonly AppDbContext _db;

        public CariYonetimiControl()
        {
            InitializeComponent();

            _db = App.DbContext;

            LoadCariFirmalar();
        }

        private void LoadCariFirmalar()
        {
            var list = _db.CariFirmalar.ToList();
            dgCariFirmalar.ItemsSource = list;
        }
        private void BtnEkle_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var form = new CariFirmaForm(_db);
            if (form.ShowDialog() == true)
            {
                LoadCariFirmalar();
            }
        }
        private void BtnGuncelle_Click(object sender, RoutedEventArgs e)
        {
            if (dgCariFirmalar.SelectedItem is CariFirma seciliFirma)
            {
                var form = new CariFirmaForm(_db, seciliFirma);
                if (form.ShowDialog() == true)
                {
                    LoadCariFirmalar();
                }
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek için bir firma seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void BtnSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgCariFirmalar.SelectedItem is CariFirma seciliFirma)
            {
                var sonuc = MessageBox.Show("Bu firmayı silmek istediğinize emin misiniz?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (sonuc == MessageBoxResult.Yes)
                {
                    _db.CariFirmalar.Remove(seciliFirma);
                    _db.SaveChanges();
                    LoadCariFirmalar();
                    MessageBox.Show("Firma silindi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir firma seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



    }
}