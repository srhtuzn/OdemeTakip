using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using OdemeTakip.Data;
using OdemeTakip.Entities;

namespace OdemeTakip.Desktop
{
    public partial class KrediKartiAnaView : UserControl
    {
        private readonly AppDbContext _db;

        public KrediKartiAnaView()
        {
            InitializeComponent();
            _db = App.DbContext;
            YukleKartlar();
        }

        private void YukleKartlar()
        {
            dgKrediKartlari.ItemsSource = _db.KrediKartlari
                .AsNoTracking()
                .Where(k => k.IsActive)
                .OrderBy(k => k.CardName)
                .ToList();
        }

        private void dgKrediKartlari_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgKrediKartlari.SelectedItem is not KrediKarti seciliKart)
            {
                dgHarcamaListesi.ItemsSource = null;
                dgTaksitliOdemeler.ItemsSource = null;
                return;
            }

            YukleHarcamalar(seciliKart.Id);
            YukleTaksitler(seciliKart.Id);
        }


        private void YukleHarcamalar(int kartId)
        {
            dgHarcamaListesi.ItemsSource = _db.KrediKartiHarcamalari
                .AsNoTracking()
                .Where(h => h.KrediKartiId == kartId && h.IsActive)
                .OrderByDescending(h => h.HarcamaTarihi)
                .ToList();
        }

        private void YukleTaksitler(int kartId)
        {
            dgTaksitliOdemeler.ItemsSource = _db.KrediKartiOdemeleri
                .AsNoTracking()
                .Include(o => o.KrediKartiHarcama)
                .Include(o => o.KrediKarti)
                .Where(o => o.KrediKartiId == kartId && o.IsActive && o.ToplamTaksit.HasValue)
                .OrderBy(o => o.OdemeTarihi)
                .ToList();
        }



        private void BtnKartEkle_Click(object sender, RoutedEventArgs e)
        {
            var form = new KrediKartiForm(_db);
            if (form.ShowDialog() == true)
            {
                YukleKartlar();
            }
        }

        private void BtnKartDuzenle_Click(object sender, RoutedEventArgs e)
        {
            if (dgKrediKartlari.SelectedItem is not KrediKarti seciliKart)
            {
                MessageBox.Show("Lütfen düzenlemek için bir kart seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var form = new KrediKartiForm(_db, seciliKart);
            if (form.ShowDialog() == true)
            {
                YukleKartlar();
            }
        }

        private void BtnKartSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgKrediKartlari.SelectedItem is not KrediKarti seciliKart)
            {
                MessageBox.Show("Lütfen silmek için bir kart seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"'{seciliKart.CardName}' kartını silmek istiyor musunuz?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                seciliKart.IsActive = false;
                _db.KrediKartlari.Update(seciliKart);
                _db.SaveChanges();
                YukleKartlar();
            }
        }

        private void BtnHarcamaEkle_Click(object sender, RoutedEventArgs e)
        {
            if (dgKrediKartlari.SelectedItem is not KrediKarti seciliKart)
            {
                MessageBox.Show("Harcama eklemek için bir kart seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var form = new KrediKartiHarcamaForm(_db, seciliKart);
            if (form.ShowDialog() == true)
            {
                YukleHarcamalar(seciliKart.Id);
                YukleTaksitler(seciliKart.Id);
            }
        }

        private void BtnHarcamaDuzenle_Click(object sender, RoutedEventArgs e)
        {
            if (dgHarcamaListesi.SelectedItem is not KrediKartiHarcama seciliHarcama)
            {
                MessageBox.Show("Düzenlemek için bir harcama seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var form = new KrediKartiHarcamaForm(_db, seciliHarcama.KrediKarti!, seciliHarcama);
            if (form.ShowDialog() == true)
            {
                YukleHarcamalar(seciliHarcama.KrediKartiId);
                YukleTaksitler(seciliHarcama.KrediKartiId);
            }
        }

        private void TxtKartAra_TextChanged(object sender, TextChangedEventArgs e)
        {
            var filtre = txtKartAra.Text.ToLower();
            dgKrediKartlari.ItemsSource = _db.KrediKartlari
                .AsNoTracking()
                .Where(k => k.IsActive && k.CardName.ToLower().Contains(filtre))
                .OrderBy(k => k.CardName)
                .ToList();
        }

        private void TxtHarcamaAra_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgKrediKartlari.SelectedItem is not KrediKarti seciliKart)
            {
                dgHarcamaListesi.ItemsSource = null;
                return;
            }

            var filtre = txtHarcamaAra.Text.ToLower();

            dgHarcamaListesi.ItemsSource = _db.KrediKartiHarcamalari
                .AsNoTracking()
                .Where(h => h.KrediKartiId == seciliKart.Id && h.IsActive &&
                            h.Aciklama.ToLower().Contains(filtre))
                .OrderByDescending(h => h.HarcamaTarihi)
                .ToList();
        }

        private void BtnHarcamaSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgHarcamaListesi.SelectedItem is not KrediKartiHarcama seciliHarcama)
            {
                MessageBox.Show("Silmek için bir harcama seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"'{seciliHarcama.Aciklama}' harcamasını silmek istiyor musunuz?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                seciliHarcama.IsActive = false;
                _db.KrediKartiHarcamalari.Update(seciliHarcama);

                var taksitler = _db.KrediKartiOdemeleri
                    .Where(o => o.KrediKartiHarcamaId == seciliHarcama.Id && o.IsActive)
                    .ToList();

                foreach (var taksit in taksitler)
                {
                    taksit.IsActive = false;
                }

                _db.SaveChanges();
                YukleHarcamalar(seciliHarcama.KrediKartiId);
                YukleTaksitler(seciliHarcama.KrediKartiId);
            }
        }

    }
}
