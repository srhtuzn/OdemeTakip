﻿// OdemeTakip.Entities/SabitGider.cs
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OdemeTakip.Entities
{
    public class SabitGider
    {
        public int Id { get; set; }
        public string? OdemeKodu { get; set; }
        public string? GiderAdi { get; set; }
        public string? Aciklama { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tutar { get; set; }
        public string? ParaBirimi { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public string? Periyot { get; set; }

        // FK + Navigation
        public int? CompanyId { get; set; }
        public Company? Company { get; set; }

        public bool OtomatikMi { get; set; }
        public bool OdendiMi { get; set; } = false; // Ödendi/Ödenmedi durumu
        public bool IsActive { get; set; } = true;
        public string? OdeyenKullaniciAdi { get; set; }
        public string? FaturaNo { get; set; }
        public int? CariFirmaId { get; set; }
        public virtual CariFirma? CariFirma { get; set; }

        public DateTime? OdemeTarihi { get; set; } // Sabit giderin fiilen ödendiği tarih
        public string? OdemeBankasi { get; set; } // Sabit giderin ödendiği banka hesabı bilgisi
    }
}