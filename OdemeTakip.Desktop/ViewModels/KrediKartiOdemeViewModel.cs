// OdemeTakip.Desktop.ViewModels/KrediKartiOdemeViewModel.cs
using System;
using System.ComponentModel; // INotifyPropertyChanged için

namespace OdemeTakip.Desktop.ViewModels
{
    /// <summary>
    /// KrediKartiOdeme (taksit ödemeleri) entity'sini UI'da göstermek için kullanılan ViewModel sınıfı.
    /// </summary>
    public class KrediKartiOdemeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public int Id { get; set; }
        // KrediKartiOdeme entity'sinde KrediKartiId int? olduğu için burada da int? olmalı.
        public int? KrediKartiId { get; set; }
        // KrediKartiOdeme entity'sinde KrediKartiHarcamaId int? olduğu için burada da int? olmalı.
        public int? KrediKartiHarcamaId { get; set; } // Hangi harcamaya ait olduğu

        public DateTime? OdemeTarihi { get; set; } // KrediKartiOdeme entity'sinde DateTime? olduğu için burada da DateTime? olmalı.
        public decimal Tutar { get; set; }
        public bool IsActive { get; set; }
        public bool OdenmeDurumu { get; set; }

        // KrediKartiOdeme entity'sinde int? olduğu için burada da int? olmalı.
        public int? ToplamTaksit { get; set; }
        // KrediKartiOdeme entity'sinde int? olduğu için burada da int? olmalı.
        public int? TaksitNo { get; set; }

        // İlişkili harcama bilgileri
        public string HarcamaAciklamasi { get; set; } = "";
        public decimal HarcamaToplamTutar { get; set; }
        // KrediKartiHarcama entity'sinde TaksitSayisi int olduğu için burada int olmalı.
        public int HarcamaTaksitSayisi { get; set; }

        // İlişkili kredi kartı bilgileri
        public string KrediKartiAdi { get; set; } = "";
    }
}