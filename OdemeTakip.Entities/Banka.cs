using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdemeTakip.Entities
{
    public class Banka
    {
        public int Id { get; set; }
        public string Adi { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

}
