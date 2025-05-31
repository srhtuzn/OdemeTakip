using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using OdemeTakip.Entities;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OdemeTakip.Desktop
{
    public partial class DegiskenOdemeSablonuControl : UserControl
    {
        private readonly AppDbContext _db;

        public DegiskenOdemeSablonuControl()
        {
            InitializeComponent();

            _db = App.DbContext;
            Yukle();
        }

        private void Yukle()
        {
            dgSablonlar.ItemsSource = _db.DegiskenOdemeSablonlari
                .Include(x => x.Company)
                .Where(x => x.IsActive)
                .ToList();
        }

        private void BtnYeni_Click(object sender, RoutedEventArgs e)
        {
            var form = new DegiskenOdemeSablonuForm(_db);
            if (form.ShowDialog() == true)
                Yukle();
        }

        private void BtnDuzenle_Click(object sender, RoutedEventArgs e)
        {
            if (dgSablonlar.SelectedItem is DegiskenOdemeSablonu secili)
            {
                var form = new DegiskenOdemeSablonuForm(_db, secili);
                if (form.ShowDialog() == true)
                    Yukle();
            }
        }

        private void BtnSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgSablonlar.SelectedItem is DegiskenOdemeSablonu secili)
            {
                if (MessageBox.Show("Bu şablonu silmek istiyor musunuz?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _db.DegiskenOdemeSablonlari.Remove(secili);
                    _db.SaveChanges();
                    Yukle();
                }
            }
        }
    }
}
