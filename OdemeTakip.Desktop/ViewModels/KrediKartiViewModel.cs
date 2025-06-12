// OdemeTakip.Desktop.ViewModels/KrediKartiViewModel.cs
using System;
using System.ComponentModel; // INotifyPropertyChanged için

namespace OdemeTakip.Desktop.ViewModels
{
    /// <summary>
    /// KrediKarti entity'sini UI'da göstermek için kullanılan ViewModel sınıfı.
    /// </summary>
    public class KrediKartiViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public int Id { get; set; }
        public string CardName { get; set; } = "";
        public string OwnerType { get; set; } = "";
        // KrediKarti entity'sinde CompanyId int? olduğu için burada da int? olmalı.
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; } = ""; // İlişkili şirket adı
        public string Banka { get; set; } = "";
        public string CardNumberLast4 { get; set; } = "";
        public decimal Limit { get; set; }
        // KrediKarti entity'sinde DateTime olduğu için burada da DateTime olmalı.
        public DateTime DueDate { get; set; } // Ekstre kesim tarihi
        public DateTime PaymentDueDate { get; set; } // Son ödeme tarihi
        public string Notes { get; set; } = "";
        public bool IsActive { get; set; }
        public string Aciklama { get; set; } = ""; // 🔥 BU SATIRI EKLEYİN 🔥

        // Diğer ilgili property'ler eklenebilir.
    }
}