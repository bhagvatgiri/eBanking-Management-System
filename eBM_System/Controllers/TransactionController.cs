using eBM_System.ContextClass;
using eBM_System.Models.DB_Models;
using eBM_System.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eBM_System.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        public IConfiguration _configuration;
        string ConnectionString = "";
        private readonly UserManager<UserAccount> userManager;
        private readonly SignInManager<UserAccount> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly AppDbContext _context;

        public TransactionController(UserManager<UserAccount> userManager, SignInManager<UserAccount> signInManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, AppDbContext context)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.signInManager = signInManager;
            _configuration = configuration;
            this._context = context;
            this.ConnectionString = _configuration.GetConnectionString("SqlConnection");
        }

        public async Task<IActionResult> Index()
        {
            List<AccountInformation_VM> accountList = new List<AccountInformation_VM>();
            try
            {
                var user = await userManager.GetUserAsync(User);
                var role = await userManager.GetRolesAsync(user);
                ViewBag.Role = role[0];
                if (role[0] == "Admin")
                {
                    accountList = _context.AccountInformation.Select(x => new AccountInformation_VM
                    {
                        AccountNumber = x.AccountNumber,
                        AccountType = x.FK_AccountType,
                        AssosatedCard = x.FK_TransactionType,
                        CardNumber = x.CardNumber,
                        Id = x.Id,
                        UserName = userManager.Users.Where(y => y.Id == x.UserId.ToString()).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                    }).ToList();
                }
                else
                {
                    accountList = _context.AccountInformation.Where(x => x.UserId.ToString() == user.Id).Select(x => new AccountInformation_VM
                    {
                        AccountNumber = x.AccountNumber,
                        AccountType = x.FK_AccountType,
                        AssosatedCard = x.FK_TransactionType,
                        CardNumber = x.CardNumber,
                        Id = x.Id,
                        UserName = userManager.Users.Where(y => y.Id == x.UserId.ToString()).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                    }).ToList();
                }

            }
            catch (Exception ex)
            {
            }
            return View(accountList);
        }

        [Authorize]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AddAccount()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AddAccount(AccountInformation_VM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var AccountExist = _context.AccountInformation.Where(x => x.AccountNumber == model.AccountNumber).FirstOrDefault();
                    if (AccountExist != null)
                    {
                        ModelState.AddModelError("", "Account Number already exist. Try Again.");
                        return View(model);
                    }
                    var user = await userManager.GetUserAsync(User);
                    AccountInformation accountInfo = new AccountInformation
                    {
                        AccountNumber = model.AccountNumber,
                        CardNumber = model.CardNumber,
                        FK_AccountType = model.AccountType,
                        FK_TransactionType = model.AssosatedCard,
                        UserId = new Guid(user.Id)
                    };
                    _context.Add(accountInfo);
                    _context.SaveChanges();
                    ViewBag.ResultStatus = "success";
                }
                else
                {
                    ViewBag.ResultStatus = "failed";
                    ModelState.AddModelError("", "Invalid entries.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ViewBag.ResultStatus = "error";
            }
            return View(model);
        }

        public async Task<IActionResult> TransactionHistory()
        {
            List<TransactionHistory_VM> modelList = new List<TransactionHistory_VM>();
            try
            {
                var user = await userManager.GetUserAsync(User);
                var role = await userManager.GetRolesAsync(user);
                if (role[0] == "Admin")
                {
                    modelList = _context.TransactionHistory.Select(x => new TransactionHistory_VM
                    {
                        Id = x.ID,
                        Amount = x.Amount,
                        TransactionDateTime = x.TransactionDateTime.ToString("dd-MMM-yyyy hh:mm tt"),
                        TransactionId = x.TransactionID,
                        TransactionType = x.FK_TransactionType,
                        UserName = userManager.Users.Where(y => y.Id == x.FK_UserId.ToString()).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                    }).ToList();
                }
                else
                {
                    modelList = _context.TransactionHistory.Where(x => x.FK_UserId.ToString() == user.Id).Select(x => new TransactionHistory_VM
                    {
                        Id = x.ID,
                        Amount = x.Amount,
                        TransactionDateTime = x.TransactionDateTime.ToString("dd-MMM-yyyy hh:mm tt"),
                        TransactionId = x.TransactionID,
                        TransactionType = x.FK_TransactionType,
                        UserName = userManager.Users.Where(y => y.Id == x.FK_UserId.ToString()).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                    }).ToList();
                }
                ViewBag.Role = role[0];
            }
            catch (Exception ex)
            {
            }
            return View(modelList);
        }

        public async Task<IActionResult> AddTransaction()
        {
            var user = await userManager.GetUserAsync(User);
            var role = await userManager.GetRolesAsync(user);
            TransactionHistory_VM userList = new TransactionHistory_VM();
            if (role[0] == "Admin")
            {
                userList.UserList = userManager.Users.ToList();
            }
            userList.UserId = user.Id;
            ViewBag.Role = role[0];
            return View(userList);
        }

        [HttpPost]
        public async Task<IActionResult> AddTransaction(TransactionHistory_VM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await userManager.GetUserAsync(User);
                    bool IsUnique = false;
                    string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                    string TransactionID = "";
                    do
                    {
                        TransactionID = GenerateTransactionID(4, saAllowedCharacters);
                        var data = _context.TransactionHistory.Where(x => x.TransactionID == TransactionID && x.FK_UserId.ToString() == user.Id).FirstOrDefault();
                        if (data == null)
                            IsUnique = true;
                    } while (!IsUnique);
                    TransactionHistory history = new TransactionHistory
                    {
                        Amount = model.Amount,
                        TransactionID = TransactionID,
                        FK_TransactionType = model.TransactionType,
                        FK_UserId = new Guid(model.UserId),
                        TransactionDateTime = DateTime.Now,
                    };
                    _context.TransactionHistory.Add(history);
                    _context.SaveChanges();
                    ViewBag.ResultStatus = "success";
                }
                else
                {
                    ViewBag.ResultStatus = "failed";
                    ModelState.AddModelError("", "Enter Valid infomration");
                }
            }
            catch (Exception ex)
            {
                ViewBag.ResultStatus = "error";
            }
            return View(model);
        }

        public async Task<IActionResult> EditTransaction(int id)
        {
            
            TransactionHistory_VM modelData = new TransactionHistory_VM();
            try
            {
                var user = await userManager.GetUserAsync(User);
                var role = await userManager.GetRolesAsync(user);
                if(role[0] == "Admin")
                {
                    modelData = _context.TransactionHistory.Where(x => x.ID == id).Select(x => new TransactionHistory_VM
                    {
                        Id = x.ID,
                        Amount = x.Amount,
                        TransactionDateTime = x.TransactionDateTime.ToString("dd-MMM-yyyy hh:mm tt"),
                        TransactionId = x.TransactionID,
                        TransactionType = x.FK_TransactionType,
                        UserId = x.FK_UserId.ToString(),
                        UserName = userManager.Users.Where(y => y.Id == x.FK_UserId.ToString()).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                    }).FirstOrDefault();
                }
                else
                {
                    modelData = _context.TransactionHistory.Where(x => x.ID == id && x.FK_UserId == new Guid(user.Id)).Select(x => new TransactionHistory_VM
                    {
                        Id = x.ID,
                        Amount = x.Amount,
                        TransactionDateTime = x.TransactionDateTime.ToString("dd-MMM-yyyy hh:mm tt"),
                        TransactionId = x.TransactionID,
                        TransactionType = x.FK_TransactionType,
                        UserId = x.FK_UserId.ToString(),
                        UserName = userManager.Users.Where(y => y.Id == x.FK_UserId.ToString()).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                    }).FirstOrDefault();
                }
                
            }
            catch (Exception ex)
            {

            }
            return View(modelData);
        }

        [HttpPost]
        public async Task<IActionResult> EditTransaction(TransactionHistory_VM model)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                var role = await userManager.GetRolesAsync(user);
                
                if (role[0] == "Admin")
                {
                    var data = _context.TransactionHistory.Where(x => x.ID == model.Id).FirstOrDefault();
                    if (data != null)
                    {
                        data.Amount = model.Amount;
                        data.FK_TransactionType = model.TransactionType;
                        data.TransactionDateTime = Convert.ToDateTime(model.TransactionDateTime);
                        _context.SaveChanges();
                        ViewBag.ResultStatus = "success";
                    }
                    else
                    {
                        ViewBag.ResultStatus = "failed";
                    }
                }
                else
                {
                    var data = _context.TransactionHistory.Where(x => x.ID == model.Id && x.FK_UserId.ToString() == user.Id).FirstOrDefault();
                    if (data != null)
                    {
                        data.Amount = model.Amount;
                        data.FK_TransactionType = model.TransactionType;
                        data.TransactionDateTime = Convert.ToDateTime(model.TransactionDateTime);
                        _context.SaveChanges();
                        ViewBag.ResultStatus = "success";
                    }
                    else
                    {
                        ViewBag.ResultStatus = "failed";
                    }
                }
                
            }
            catch (Exception ex)
            {
                ViewBag.ResultStatus = "error";
            }
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> RemoveTransaction(int transactionId)
        {
            var res = "";
            try
            {
                var user = await userManager.GetUserAsync(User);
                var role = await userManager.GetRolesAsync(user);
                if (role[0] == "Admin")
                {
                    var transactionData = _context.TransactionHistory.Where(x => x.ID == transactionId).FirstOrDefault();
                    if (transactionData != null)
                    {
                        _context.TransactionHistory.Remove(transactionData);
                        _context.SaveChanges();
                        res = "success";
                    }
                    else
                    {
                        res = "failed";
                    }
                }
                else
                {
                    var transactionData = _context.TransactionHistory.Where(x => x.ID == transactionId && x.FK_UserId.ToString() == user.Id).FirstOrDefault();
                    if (transactionData != null)
                    {
                        _context.TransactionHistory.Remove(transactionData);
                        _context.SaveChanges();
                        res = "success";
                    }
                    else
                    {
                        res = "failed";
                    }

                }
                

            }
            catch (Exception ex)
            {
                res = "error";
            }
            return Json(res);
        }


        [HttpPost]
        public async Task<JsonResult> GetTransactionData(int transcationId)
        {
            try
            {
                var TransactionHistory = _context.TransactionHistory.Where(x => x.ID == transcationId).FirstOrDefault();
                if (TransactionHistory != null)
                {
                    return Json(new
                    {
                        transactionID = TransactionHistory.TransactionID,
                        transactionType = TransactionHistory.FK_TransactionType.ToString(),
                        transactionTime = TransactionHistory.TransactionDateTime.ToString("dd-MMM-yyyy hh:mm tt"),
                        transactionAmount = TransactionHistory.Amount,
                    });
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string GenerateTransactionID(int iOTPLength, string[] saAllowedCharacters)
        {

            string sOTP = String.Empty;

            string sTempChars = String.Empty;

            Random rand = new Random();

            for (int i = 0; i < iOTPLength; i++)

            {

                int p = rand.Next(0, saAllowedCharacters.Length);

                sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];

                sOTP += sTempChars;

            }

            return sOTP;

        }
    }
}
