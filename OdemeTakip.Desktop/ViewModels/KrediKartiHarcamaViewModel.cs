// OdemeTakip.Desktop.ViewModels/KrediKartiHarcamaViewModel.cs
using System;
using System.ComponentModel; // INotifyPropertyChanged için

namespace OdemeTakip.Desktop.ViewModels
{
    /// <summary>
    /// KrediKartiHarcama entity'sini UI'da göstermek için kullanılan ViewModel sınıfı.
    /// </summary>
    public class KrediKartiHarcamaViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public int Id { get; set; }
        public int KrediKartiId { get; set; }
        public string Aciklama { get; set; } = "";
        public decimal Tutar { get; set; }
        public int TaksitSayisi { get; set; }
        public DateTime HarcamaTarihi { get; set; }
        public bool IsActive { get; set; }

        // İlişkili kredi kartı bilgileri
        public string KrediKartiAdi { get; set; } = "";
    }
}