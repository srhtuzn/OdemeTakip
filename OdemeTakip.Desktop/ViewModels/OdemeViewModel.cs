// OdemeTakip.Desktop.ViewModels/OdemeViewModel.cs
using System;
using System.ComponentModel; // INotifyPropertyChanged için gerekli

namespace OdemeTakip.Desktop.ViewModels
{
    /// <summary>
    /// Farklı ödeme türlerini (Kredi, Kredi Kartı, Sabit, Değişken, Çek, Genel)
    /// ortak bir yapı altında göstermek için kullanılan ViewModel sınıfı.
    /// UI'da veri bağlama ve property değişikliklerini bildirme yeteneği sağlar.
    /// </summary>
    public class OdemeViewModel : INotifyPropertyChanged
    {
        // PropertyChanged olayını implemente et
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Property değeri değiştiğinde UI'ya bildirimde bulunmak için kullanılan yardımcı metot.
        /// </summary>
        /// <param name="propertyName">Değişen property'nin adı.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Entity'nin asıl Primary Key'i (Ödeme kaynağının ID'si)
        public int Id { get; set; }

        // Kaynak Entity'nin ID'si (genellikle Id ile aynı olabilir, ancak farklı senaryolar için tutulabilir)
        public int KaynakId { get; set; }

        private string _kod = "";
        /// <summary>
        /// Ödemenin benzersiz kodu (örn: Kredi kodu, Fatura kodu).
        /// </summary>
        public string Kod
        {
            get => _kod;
            set
            {
                if (_kod != value)
                {
                    _kod = value;
                    OnPropertyChanged(nameof(Kod));
                }
            }
        }

        private string _aciklama = "";
        /// <summary>
        /// Ödeme ile ilgili açıklama.
        /// </summary>
        public string Aciklama
        {
            get => _aciklama;
            set
            {
                if (_aciklama != value)
                {
                    _aciklama = value;
                    OnPropertyChanged(nameof(Aciklama));
                }
            }
        }

        private DateTime _tarih;
        /// <summary>
        /// Ödemenin gerçekleştiği veya planlandığı tarih.
        /// Genellikle vade tarihi olarak kullanılır ve VadeTarihi ile senkronizedir.
        /// </summary>
        public DateTime Tarih // XAML'deki Tarih sütunu buna bağlanır
        {
            get => _tarih;
            set
            {
                if (_tarih != value)
                {
                    _tarih = value;
                    OnPropertyChanged(nameof(Tarih));
                    // Tarih değiştiğinde VadeTarihi'ni de güncelleyebiliriz veya ayrı tutabiliriz.
                    // Mevcut durumda VadeTarihi'nin setteri Tarih'i güncelliyor, bu yüzden bir döngüden kaçınmak için dikkatli olun.
                    // Burası sadece Tarih'in UI'da görüntülenen yüzü olsun.
                }
            }
        }

        private decimal _tutar;
        /// <summary>
        /// Ödemenin tutarı.
        /// </summary>
        public decimal Tutar
        {
            get => _tutar;
            set
            {
                if (_tutar != value)
                {
                    _tutar = value;
                    OnPropertyChanged(nameof(Tutar));
                }
            }
        }

        private string _paraBirimi = "TL";
        /// <summary>
        /// Ödemenin para birimi (örn: TL, USD, EUR).
        /// </summary>
        public string ParaBirimi
        {
            get => _paraBirimi;
            set
            {
                if (_paraBirimi != value)
                {
                    _paraBirimi = value;
                    OnPropertyChanged(nameof(ParaBirimi));
                }
            }
        }

        private string _kaynakModul = "";
        /// <summary>
        /// Ödemenin hangi ana modülden geldiği (örn: Kredi, Kredi Kartı, Sabit Ödeme).
        /// </summary>
        public string KaynakModul
        {
            get => _kaynakModul;
            set
            {
                if (_kaynakModul != value)
                {
                    _kaynakModul = value;
                    OnPropertyChanged(nameof(KaynakModul));
                }
            }
        }

        private bool _odenmeDurumu;
        /// <summary>
        /// Ödemenin yapılıp yapılmadığını gösteren boolean değer.
        /// Bu property değiştiğinde, UI'da hem DurumText hem de RenkKod güncellenir.
        /// </summary>
        public bool OdenmeDurumu
        {
            get => _odenmeDurumu;
            set
            {
                if (_odenmeDurumu != value)
                {
                    _odenmeDurumu = value;
                    OnPropertyChanged(nameof(OdenmeDurumu));
                    // OdenmeDurumu değiştiğinde, DurumText ve RenkKod'un da değiştiğini bildir
                    OnPropertyChanged(nameof(Durum)); // Durum property'si için de bildirim
                    OnPropertyChanged(nameof(DurumText)); // Durum metni için bildirim
                    OnPropertyChanged(nameof(RenkKod));   // Renk kodu için bildirim
                }
            }
        }

        private DateTime? _odemeTarihi;
        /// <summary>
        /// Ödemenin fiilen yapıldığı tarih. Null olabilir (henüz ödenmediyse).
        /// </summary>
        public DateTime? OdemeTarihi
        {
            get => _odemeTarihi;
            set
            {
                if (_odemeTarihi != value)
                {
                    _odemeTarihi = value;
                    OnPropertyChanged(nameof(OdemeTarihi));
                }
            }
        }

        private string? _odemeBankasi;
        /// <summary>
        /// Ödemenin yapıldığı banka hesabı bilgisi.
        /// </summary>
        public string? OdemeBankasi
        {
            get => _odemeBankasi;
            set
            {
                if (_odemeBankasi != value)
                {
                    _odemeBankasi = value;
                    OnPropertyChanged(nameof(OdemeBankasi));
                }
            }
        }

        private string? _faturaNo;
        /// <summary>
        /// Ödemeye ait fatura numarası. Null veya boş olabilir.
        /// </summary>
        public string? FaturaNo
        {
            get => _faturaNo;
            set
            {
                if (_faturaNo != value)
                {
                    _faturaNo = value;
                    OnPropertyChanged(nameof(FaturaNo));
                }
            }
        }

        private string? _sirketAdi;
        /// <summary>
        /// Ödemeyi yapan veya ilgili şirketin adı.
        /// </summary>
        public string? SirketAdi
        {
            get => _sirketAdi;
            set
            {
                if (_sirketAdi != value)
                {
                    _sirketAdi = value;
                    OnPropertyChanged(nameof(SirketAdi));
                }
            }
        }

        private string? _odeyenKullaniciAdi = "";
        /// <summary>
        /// Ödemeyi onaylayan/işaretleyen kullanıcının adı.
        /// </summary>
        public string? OdeyenKullaniciAdi
        {
            get => _odeyenKullaniciAdi;
            set
            {
                if (_odeyenKullaniciAdi != value)
                {
                    _odeyenKullaniciAdi = value;
                    OnPropertyChanged(nameof(OdeyenKullaniciAdi));
                }
            }
        }

        /// <summary>
        /// Ödeme yapıldı mı? (OdenmeDurumu ile senkronize property).
        /// </summary>
        public bool Durum
        {
            get => OdenmeDurumu;
            set
            {
                // Setter'ı kullanırken OdenmeDurumu property'sini set et, bu onun OnPropertyChanged'ını tetikler.
                if (OdenmeDurumu != value)
                {
                    OdenmeDurumu = value;
                }
            }
        }

        private DateTime _vadeTarihi;
        /// <summary>
        /// Ödemenin vade tarihi. Bu, ödemenin ne zaman yapılması gerektiğini belirtir.
        /// </summary>
        public DateTime VadeTarihi // ViewModel'in ana vade tarihi property'si
        {
            get => _vadeTarihi;
            set
            {
                if (_vadeTarihi != value)
                {
                    _vadeTarihi = value;
                    OnPropertyChanged(nameof(VadeTarihi));
                    // VadeTarihi değiştiğinde, Tarih, DurumText ve RenkKod'un da değiştiğini bildir
                    OnPropertyChanged(nameof(Tarih)); // Tarih property'si için de bildirim
                    OnPropertyChanged(nameof(DurumText)); // Durum metni için bildirim
                    OnPropertyChanged(nameof(RenkKod));   // Renk kodu için bildirim
                }
            }
        }

        private int _taksitNo;
        /// <summary>
        /// Kredi veya taksitli harcama gibi durumlarda taksit numarası.
        /// </summary>
        public int TaksitNo
        {
            get => _taksitNo;
            set
            {
                if (_taksitNo != value)
                {
                    _taksitNo = value;
                    OnPropertyChanged(nameof(TaksitNo));
                }
            }
        }

        private string _cariFirmaAdi = "";
        /// <summary>
        /// Ödemenin yapıldığı veya ilgili olan cari firmanın adı.
        /// </summary>
        public string CariFirmaAdi
        {
            get => _cariFirmaAdi;
            set
            {
                if (_cariFirmaAdi != value)
                {
                    _cariFirmaAdi = value;
                    OnPropertyChanged(nameof(CariFirmaAdi));
                }
            }
        }

        private string _modulIkon = "";
        /// <summary>
        /// Ödeme modülünü temsil eden ikon (UI için).
        /// Bu property genellikle ViewModel'in kendisinde set edilir (örneğin mapping sırasında).
        /// </summary>
        public string ModulIkon
        {
            get => _modulIkon;
            set
            {
                if (_modulIkon != value)
                {
                    _modulIkon = value;
                    OnPropertyChanged(nameof(ModulIkon));
                }
            }
        }

        private string _durumText = "";
        /// <summary>
        /// Ödeme durumunu metin olarak temsil eder (örn: "Ödendi", "Ödenecek", "Gecikmiş").
        /// Bu property, OdenmeDurumu ve VadeTarihi'ne göre otomatik olarak güncellenir.
        /// </summary>
        public string DurumText // Bu, UI'da gösterilecek olan metin
        {
            get
            {
                // Bu değerin dinamik olarak hesaplanması daha iyi.
                if (OdenmeDurumu) return "✓ Ödendi";
                if (VadeTarihi.Date < DateTime.Today.Date) return "❌ Gecikmiş";
                if (VadeTarihi.Date == DateTime.Today.Date) return "⚠️ Bugün";
                return "⏳ Ödenecek";
            }
            set // Eğer dışarıdan da set edilirse bu setter kullanılır.
            {
                if (_durumText != value)
                {
                    _durumText = value;
                    OnPropertyChanged(nameof(DurumText));
                }
            }
        }

        private string _renkKod = "";
        /// <summary>
        /// UI'da ödeme durumuna göre satırın rengini belirlemek için kullanılan kod (örn: "Green", "Red").
        /// Bu property, OdenmeDurumu ve VadeTarihi'ne göre otomatik olarak güncellenir.
        /// </summary>
        public string RenkKod // Bu, UI'da satır rengini belirleyecek olan metin
        {
            get
            {
                // Bu değerin dinamik olarak hesaplanması daha iyi.
                if (OdenmeDurumu) return "ForestGreen"; // Renk converter'lar ile çakışmaması için Color adları yerine Brush adları
                if (VadeTarihi.Date < DateTime.Today.Date) return "Red";
                if (VadeTarihi.Date == DateTime.Today.Date) return "Orange";
                return "Gray";
            }
            set // Eğer dışarıdan da set edilirse bu setter kullanılır.
            {
                if (_renkKod != value)
                {
                    _renkKod = value;
                    OnPropertyChanged(nameof(RenkKod));
                }
            }
        }

        private string _modulTipi = "";
        /// <summary>
        /// Ödemenin hangi tip modülden geldiğini belirtir (örn: "Kredi", "KrediKart", "Sabit", "Degisken", "Cek", "Genel").
        /// </summary>
        public string ModulTipi
        {
            get => _modulTipi;
            set
            {
                if (_modulTipi != value)
                {
                    _modulTipi = value;
                    OnPropertyChanged(nameof(ModulTipi));
                    OnPropertyChanged(nameof(ModulTipiAciklama));
                }
            }
        }

        /// <summary>
        /// Modül tipinin daha okunaklı bir açıklamasını sağlar (UI için).
        /// </summary>
        public string ModulTipiAciklama
        {
            get
            {
                return ModulTipi switch
                {
                    "Kredi" => "Kredi",
                    "KrediKart" => "Kredi Kartı",
                    "Sabit" => "Sabit Ö.",
                    "Degisken" => "Abonelik Ö.",
                    "Cek" => "Çek",
                    "Genel" => "Genel Ö.",
                    _ => "Bilinmeyen"
                };
            }
        }

        // 👇 BURAYA EKLENECEK PROPERTY'LER 👇
        // Kredi kartı harcaması detaylarını OdemeViewModel'e aktarmak için
        public decimal HarcamaToplamTutar { get; set; }
        public int HarcamaTaksitSayisi { get; set; }

        // 👆 BURAYA EKLENECEK PROPERTY'LER 👆
    }
}