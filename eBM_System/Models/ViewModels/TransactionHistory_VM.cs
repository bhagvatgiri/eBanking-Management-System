using eBM_System.Models.DB_Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eBM_System.Models.ViewModels
{
    public class TransactionHistory_VM
    {
        public int Id { get; set; }
        public string TransactionId { get; set; }
        [Required]
        public TransactionType TransactionType { get; set; }
        public string TransactionDateTime { get; set; }
        
        public string UserName { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public decimal Amount { get; set; }

        public List<UserAccount> UserList = new List<UserAccount>();
    }
}
