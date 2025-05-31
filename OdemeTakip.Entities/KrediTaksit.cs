using OdemeTakip.Entities;

public class KrediTaksit
{
    public int Id { get; set; }

    public string KrediKodu { get; set; } = null!;

    public int KrediId { get; set; }   // ✅ foreign key alanı
    public Kredi Kredi { get; set; } = null!; // ✅ ilişkili nesne

    public int TaksitNo { get; set; }
    public DateTime Tarih { get; set; }
    public decimal Tutar { get; set; }

    public bool OdenmeDurumu { get; set; } = false;
    public DateTime? OdenmeTarihi { get; set; }
    public string? OdemeBankasi { get; set; }
}
