using eBM_System.Models.DB_Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eBM_System.Models.ViewModels
{
    public class AccountInformation_VM
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        [Required]
        public AccountType AccountType { get; set; }
        [Required]
        [StringLength(8, MinimumLength = 8)]
        public string AccountNumber { get; set; }
        [Required]
        public TransactionType AssosatedCard { get; set; }
        [Required]
        [StringLength(16, MinimumLength = 16)]
        public string CardNumber { get; set; }
    }
}
