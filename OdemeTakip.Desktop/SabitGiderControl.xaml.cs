using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using OdemeTakip.Entities;

namespace OdemeTakip.Desktop
{
    public partial class SabitGiderControl : UserControl
    {
        private readonly AppDbContext _db;

        public SabitGiderControl()
        {
            InitializeComponent();

            _db = App.DbContext;

            LoadSabitGiderler();
        }

        private void LoadSabitGiderler()
        {
            var sabitGiderler = _db.SabitGiderler
                .Include(x => x.Company)
                .Where(x => x.IsActive)
                .OrderBy(x => x.BaslangicTarihi)
                .ToList();

            dgSabitGiderler.ItemsSource = sabitGiderler;
        }

        public void YenidenYukle()
        {
            LoadSabitGiderler();
        }


        private void BtnEkle_Click(object sender, RoutedEventArgs e)
        {
            var form = new SabitGiderForm(_db);
            if (form.ShowDialog() == true)
            {
                LoadSabitGiderler();
            }
        }

        private void BtnGuncelle_Click(object sender, RoutedEventArgs e)
        {
            if (dgSabitGiderler.SelectedItem is SabitGider secili)
            {
                var form = new SabitGiderForm(_db, secili);
                if (form.ShowDialog() == true)
                {
                    LoadSabitGiderler();
                }
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek için bir kayıt seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgSabitGiderler.SelectedItem is SabitGider secili)
            {
                var result = MessageBox.Show("Bu sabit gideri silmek istediğinize emin misiniz?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _db.SabitGiderler.Remove(secili);
                    _db.SaveChanges();
                    LoadSabitGiderler();
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir kayıt seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
