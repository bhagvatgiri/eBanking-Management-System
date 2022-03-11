using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eBM_System.Models.DB_Models
{
    public class AccountInformation
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public AccountType FK_AccountType { get; set; }
        [Required]
        [StringLength(8, MinimumLength = 8)]
        public string AccountNumber { get; set; }
        [Required]
        public TransactionType FK_TransactionType { get; set; }
        [Required]
        [StringLength(16, MinimumLength = 16)]
        public string CardNumber { get; set; }
    }

    public class TransactionHistory
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [StringLength(8, MinimumLength = 8)]
        public string TransactionID { get; set; }
        [Required]
        public Guid FK_UserId { get; set; }
        [Required]
        public TransactionType FK_TransactionType { get; set; }
        [Required]
        public DateTime TransactionDateTime { get; set; }
        [Required]
        public decimal Amount { get; set; }
    }

    public enum TransactionType
    {
        Debit, Credit
    }

    public enum AccountType
    {
        Debit, Saving, Checking
    }
}
