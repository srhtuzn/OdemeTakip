using System;
using System.ComponentModel; // INotifyPropertyChanged için

namespace OdemeTakip.Desktop.ViewModels
{
    /// <summary>
    /// Kullanıcı arayüzünde eksik faturaları göstermek için kullanılan ViewModel sınıfı.
    /// MVVM prensiplerine uygun olarak veri bağlama ve bildirim mekanizmalarını sağlar.
    /// </summary>
    public class EksikFaturaViewModel : INotifyPropertyChanged
    {
        // Entity'nin benzersiz kimliği
        public int Id { get; set; }

        private string _giderTuru = ""; // Varsayılan boş string
        /// <summary>
        /// Faturanın gider türünü temsil eder (örn: Elektrik, Su).
        /// </summary>
        public string GiderTuru
        {
            get => _giderTuru;
            set
            {
                if (_giderTuru != value)
                {
                    _giderTuru = value;
                    OnPropertyChanged(nameof(GiderTuru));
                }
            }
        }

        private string _aciklama = ""; // Varsayılan boş string
        /// <summary>
        /// Fatura ile ilgili ek açıklamayı temsil eder.
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

        private string _sirketAdi = ""; // Varsayılan boş string
        /// <summary>
        /// Faturanın ilgili olduğu şirketin adını temsil eder.
        /// </summary>
        public string SirketAdi
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

        private string _cariFirmaAdi = ""; // Varsayılan boş string
        /// <summary>
        /// Faturanın ait olduğu cari firmanın adını temsil eder.
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

        // VadeTarihi ve OdemeTarihi property'leri kaldırıldı.
        // OdemeTarihi entity'deki ana tarih olduğu için, doğrudan onu kullanacağız.

        private DateTime _tarih;
        /// <summary>
        /// Faturanın ödeme/vade tarihini temsil eder.
        /// Bu, DegiskenOdeme entity'sindeki OdemeTarihi'ne karşılık gelir.
        /// </summary>
        public DateTime Tarih // XAML'de DataGrid için "Tarih" olarak bağlanacak
        {
            get => _tarih;
            set
            {
                if (_tarih != value)
                {
                    _tarih = value;
                    OnPropertyChanged(nameof(Tarih));
                }
            }
        }

        private string _paraBirimi = "TL"; // Varsayılan olarak "TL"
        /// <summary>
        /// Fatura tutarının para birimini temsil eder (örn: TL, USD, EUR).
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

        private decimal _tutar;
        /// <summary>
        /// Faturanın tutarını temsil eder. Eksik faturalar için başlangıçta 0 olabilir.
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

        private string? _faturaNo; // Fatura numarası null atanabilir olmalı
        /// <summary>
        /// Faturanın numarasını temsil eder. Eksik faturalar için başlangıçta null veya boş olabilir.
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

        /// <summary>
        /// Property değiştiğinde UI'ya bildirimde bulunmak için kullanılan olay.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Belirtilen property adı için PropertyChanged olayını tetikler.
        /// </summary>
        /// <param name="propertyName">Değişen property'nin adı.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}