using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdemeTakip.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // SHA256 ile saklanacak
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
