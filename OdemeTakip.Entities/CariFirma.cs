using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CariFirma
{
    public int Id { get; set; }
    public string? CariKodu { get; set; }
    public string? Name { get; set; }             // Firma Adı
    public string? TaxNumber { get; set; }        // Vergi Numarası
    public string? TaxOffice { get; set; }        // Vergi Dairesi
    public string? Address { get; set; }          // Adres
    public string? Phone { get; set; }            // Telefon
    public string? Email { get; set; }            // E-Posta
    public string? Banka { get; set; }             // Banka
    public string? Iban { get; set; }             // IBAN
    public string? ContactPerson { get; set; }    // Yetkili / İrtibat kişisi
    public string? Notes { get; set; }            // Açıklama, not
    public bool IsActive { get; set; } = true;    // Aktif mi?
}
