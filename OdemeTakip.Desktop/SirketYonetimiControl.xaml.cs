using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using OdemeTakip.Entities;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OdemeTakip.Desktop
{
    public partial class SirketYonetimiControl : UserControl
    {
        private readonly AppDbContext _db;

        public SirketYonetimiControl()
        {
            InitializeComponent();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer("Server=DEVSQL;Database=OdemeTakip;User Id=sa;Password=Yeksun.1288;TrustServerCertificate=True;")
                .Options;

            _db = App.DbContext;

            LoadCompanies();
        }

        private void LoadCompanies()
        {
            var list = _db.Companies.ToList();
            dgCompanies.ItemsSource = list;
        }

        private void BtnEkle_Click(object sender, RoutedEventArgs e)
        {
            var form = new CompanyForm(_db);
            if (form.ShowDialog() == true)
            {
                LoadCompanies();
            }
        }

        private void BtnGuncelle_Click(object sender, RoutedEventArgs e)
        {
            if (dgCompanies.SelectedItem is Company seciliFirma)
            {
                var form = new CompanyForm(_db, seciliFirma);
                if (form.ShowDialog() == true)
                {
                    LoadCompanies();
                }
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek için bir şirket seçin.");
            }
        }

        private void BtnSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgCompanies.SelectedItem is Company seciliFirma)
            {
                var sonuc = MessageBox.Show("Bu şirketi silmek istediğinize emin misiniz?", "Onay", MessageBoxButton.YesNo);
                if (sonuc == MessageBoxResult.Yes)
                {
                    _db.Companies.Remove(seciliFirma);
                    _db.SaveChanges();
                    LoadCompanies();
                    MessageBox.Show("Şirket silindi.");
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir şirket seçin.");
            }
        }
    }
}
