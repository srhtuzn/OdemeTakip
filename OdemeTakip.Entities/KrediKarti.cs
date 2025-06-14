﻿// OdemeTakip.Entities/KrediKarti.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdemeTakip.Entities
{
    public class KrediKarti
    {
        public int Id { get; set; }
        public string? CardName { get; set; }
        public string? OwnerType { get; set; }

        public int? CompanyId { get; set; }  // FK: Şirketi Seçeceğiz
        public Company? Company { get; set; }  // Navigation Property

        public string Banka { get; set; } = string.Empty;
        public string? CardNumberLast4 { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Limit { get; set; }

        public DateTime DueDate { get; set; } // Ekstre kesim tarihi
        public DateTime PaymentDueDate { get; set; } // Son ödeme tarihi
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
    }
}