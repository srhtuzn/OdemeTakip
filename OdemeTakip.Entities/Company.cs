using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdemeTakip.Entities
{
    public class Company
    {
        public int Id { get; set; }
        public string? SirketKodu { get; set; }
        public string? Name { get; set; }               // Şirket adı
        public string? Type { get; set; }               // A.Ş., Ltd. vs.
        public string? TaxNumber { get; set; }          // Vergi numarası
        public string? TaxOffice { get; set; }          // Vergi dairesi
        public string? Phone { get; set; }              // Telefon
        public string? Email { get; set; }              // E-posta
        public string? Address { get; set; }            // Açık adres
        public string? AuthorizedPerson { get; set; }   // Yetkili kişi
        public List<BankaHesabi> BankaHesaplari { get; set; } = new();

        public string? Notes { get; set; }              // Açıklama / Not
        public bool IsActive { get; set; } = true;
    }
}