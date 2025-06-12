using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore; // Include metodu için
using OdemeTakip.Data;
using OdemeTakip.Entities;
using System; // Exception için eklendi

namespace OdemeTakip.Desktop
{
    public partial class KrediControl : UserControl
    {
        private AppDbContext _db;

        public KrediControl()
        {
            InitializeComponent();

            _db = App.DbContext; // App.DbContext'ten veritabanı bağlamını al

            LoadKrediler(); // Kontrol yüklendiğinde kredileri yükle
        }

        /// <summary>
        /// Kredi listesini yeniden yükler.
        /// </summary>
        public void YenidenYukle()
        {
            LoadKrediler();
        }

        /// <summary>
        /// Veritabanından kredi verilerini çeker ve DataGrid'e bağlar.
        /// İlişkili şirket ve cari firma bilgilerini de yükler.
        /// </summary>
        private void LoadKrediler()
        {
            // Kredileri AsNoTracking ile çekiyoruz, böylece DbContext tarafından takip edilmezler.
            // Bu, DataGrid'de sadece görüntüleme amaçlıdır ve takip çakışmalarını önler.
            var list = _db.Krediler.AsNoTracking()
                .Include(k => k.Company)          // İlişkili Company nesnesini yükle
                .Include(k => k.CariFirma)        // İlişkili CariFirma nesnesini yükle
                .ToList(); // Veriyi belleğe çek

            dgKrediler.ItemsSource = list; // DataGrid'e veriyi bağla
        }

        /// <summary>
        /// Yeni bir kredi ekleme formunu açar.
        /// </summary>
        private void BtnEkle_Click(object sender, RoutedEventArgs e)
        {
            var form = new KrediForm(_db); // Yeni bir KrediForm penceresi oluştur
            if (form.ShowDialog() == true) // Form başarıyla kapatılırsa
            {
                LoadKrediler(); // Listeyi yeniden yükle
            }
        }

        /// <summary>
        /// Seçili krediyi güncelleme formunu açar.
        /// </summary>
        private void BtnGuncelle_Click(object sender, RoutedEventArgs e)
        {
            if (dgKrediler.SelectedItem is Kredi secili) // DataGrid'de bir Kredi seçili mi kontrol et
            {
                // Seçili kredi nesnesini tam olarak yükle (ilişkili taksitleriyle birlikte).
                // LoadKrediler'da AsNoTracking kullandığımız için, burada DbContext tarafından takip edilen
                // yeni bir örnek çekmeliyiz ki üzerinde değişiklik yapıp SaveChanges() diyebilelim.
                var krediEntity = _db.Krediler
                                    .Include(k => k.Taksitler) // Taksitleri de yükle
                                    .FirstOrDefault(k => k.Id == secili.Id);

                if (krediEntity == null)
                {
                    MessageBox.Show("Seçili kredi bulunamadı veya veritabanından silinmiş.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var form = new KrediForm(_db, krediEntity); // KrediForm penceresini takip edilen kredi ile oluştur
                if (form.ShowDialog() == true) // Form başarıyla kapatılırsa
                {
                    LoadKrediler(); // Listeyi yeniden yükle
                }
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek için bir kayıt seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Seçili krediyi siler (veritabanından kaldırır).
        /// İlişkili taksitleri de manuel olarak siler.
        /// </summary>
        private void BtnSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgKrediler.SelectedItem is Kredi secili) // DataGrid'de bir Kredi seçili mi kontrol et
            {
                var result = MessageBox.Show($"'{secili.KrediKodu} - {secili.KrediKonusu}' krediyi silmek istediğinize emin misiniz?",
                                            "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Entity Framework'te takip çakışmalarını önlemek için en güvenli silme yöntemi:
                        // Sadece ID'si bilinen bir dummy entity oluşturup onu Deleted olarak işaretle.
                        var entityToDelete = new Kredi { Id = secili.Id };
                        _db.Entry(entityToDelete).State = EntityState.Deleted;

                        // Kredi ile ilişkili taksitleri de sil (Cascade Delete ayarlı değilse bu gerekli)
                        // Önce ilgili taksitleri veritabanından çekmeliyiz.
                        var relatedTaksitler = _db.KrediTaksitler.Where(t => t.KrediId == secili.Id).ToList();
                        if (relatedTaksitler.Any())
                        {
                            _db.KrediTaksitler.RemoveRange(relatedTaksitler);
                        }

                        _db.SaveChanges(); // Değişiklikleri veritabanına kaydet
                        LoadKrediler(); // Listeyi yeniden yükle
                        MessageBox.Show("Kredi başarıyla silindi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Kredi silinirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir kayıt seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}