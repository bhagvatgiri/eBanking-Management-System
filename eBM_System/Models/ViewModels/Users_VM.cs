using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eBM_System.Models.ViewModels
{
    public class Users_VM
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
