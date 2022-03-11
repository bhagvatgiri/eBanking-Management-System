using eBM_System.ContextClass;
using eBM_System.Models.DB_Models;
using eBM_System.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace eBM_System.Controllers
{
    public class AccountController : Controller
    {
        public IConfiguration _configuration;
        string ConnectionString = "";
        private readonly UserManager<UserAccount> userManager;
        private readonly SignInManager<UserAccount> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly AppDbContext _context;

        public AccountController(UserManager<UserAccount> userManager, SignInManager<UserAccount> signInManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, AppDbContext context)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.signInManager = signInManager;
            _configuration = configuration;
            this._context = context;
            this.ConnectionString = _configuration.GetConnectionString("SqlConnection");
        }

        public async Task<IActionResult> Login()
        {
            try
            {
                var roles = roleManager.Roles;
                if (roles.ToList().Count == 0)
                {
                    IdentityRole roleAdmin = new IdentityRole
                    {
                        Name = "Admin"
                    };
                    IdentityResult res1 = await roleManager.CreateAsync(roleAdmin);
                    IdentityRole roleCustomer = new IdentityRole
                    {
                        Name = "Customer"
                    };
                    IdentityResult res2 = await roleManager.CreateAsync(roleCustomer);
                }
                var usersList = userManager.Users.ToList();
                if (usersList.Count == 0)
                {
                    var role = await roleManager.FindByNameAsync("Admin");
                    var user = new UserAccount
                    {
                        FirstName = "Add Your Name here",
                        LastName = "Add Last Name",
                        UserName = "Add Email Id",
                        Email = "Add Email Id",
                        Password = "Add Password"
                    };
                    var result = await userManager.CreateAsync(user, "password");
                    if (result.Succeeded)
                    {
                        var userCheck = await userManager.FindByNameAsync("Email_id");
                        if (!await userManager.IsInRoleAsync(userCheck, role.Name))
                        {
                            result = await userManager.AddToRoleAsync(userCheck, role.Name);
                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(Login_VM model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.Email);
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, false, true);
                if (result.Succeeded)
                {
                    var userRoles = await userManager.GetRolesAsync(user);
                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.Email),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };
                    foreach (var userRole in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                    }
                    return RedirectToAction("Index", "Home");
                }
                else if (result.IsLockedOut)
                {
                    ModelState.AddModelError("", "Too many invalid signin attempts. Account locked. Try again after 20 mins.");
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid Email or Password.");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "Invalid Email or Password.");
                return View(model);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllUsers()
        {
            List<UserAccount> userList = new List<UserAccount>();
            try
            {
                userList = userManager.Users.Where(x => x.IsActive == true).ToList();
            }
            catch (Exception ex)
            {

            }
            return View(userList);
        }


        /// <summary>
        /// Sign-in the user
        /// </summary>
        /// <returns></returns>
        /// 
        async Task<Microsoft.AspNetCore.Identity.SignInResult> AttemptUserLogin(Login_VM model, UserAccount user)
        {
            var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, false, true);
            if (result.Succeeded)
            {
                var userRoles = await userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }
                user.AccessFailedCount = 0;
            }
            return result;
        }

        [AllowAnonymous]
        public IActionResult CreateCustomer()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateCustomer(RegisterUserVM registerUser)
        {
            bool res = false;
            try
            {
                if (ModelState.IsValid)
                {
                    var UserName = userManager.GetUserName(User);
                    var roles = await roleManager.FindByNameAsync("Customer");
                    var user = new UserAccount
                    {
                        FirstName = registerUser.FirstName,
                        LastName = registerUser.LastName,
                        UserName = registerUser.Email,
                        Email = registerUser.Email,
                        IsActive = true,
                        Password = registerUser.Password,

                    };
                    var result = await userManager.CreateAsync(user, registerUser.Password);
                    if (result.Succeeded)
                    {
                        var userCheck = await userManager.FindByNameAsync(registerUser.Email);
                        if (!await userManager.IsInRoleAsync(userCheck, roles.Name))
                        {
                            result = await userManager.AddToRoleAsync(userCheck, roles.Name);
                        }
                        if (result.Succeeded)
                        {
                            ViewBag.ResultStatus = "success";
                        }
                        else
                        {
                            ViewBag.ResultStatus = "failed";
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Enter all relevant fields.");
                    ViewBag.ResultStatus = "failed";
                }
            }
            catch (Exception ex)
            {
                ViewBag.ResultStatus = "error";
            }
            return View(registerUser);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> EditCustomer(string email)
        {
            RegisterUserVM userData = null;
            try
            {
                var user = await userManager.GetUserAsync(User);
                var role = await userManager.GetRolesAsync(user);

                if (!string.IsNullOrEmpty(email))
                {
                    var findUser = await userManager.FindByEmailAsync(email);
                    if (role[0] == "Admin")
                    {
                        
                        userData = new RegisterUserVM
                        {
                            Email = findUser.Email,
                            FirstName = findUser.FirstName,
                            LastName = findUser.LastName,
                            Password = findUser.Password,
                            ConfirmPassword = findUser.Password
                        };
                        if (userData == null)
                        {
                            ModelState.AddModelError("", "Unable to get User Information. Try again later.");
                        }
                    }
                    else if(findUser.Id == user.Id)
                    {
                        userData = new RegisterUserVM
                        {
                            Email = findUser.Email,
                            FirstName = findUser.FirstName,
                            LastName = findUser.LastName,
                            Password = findUser.Password,
                            ConfirmPassword = findUser.Password
                        };
                        if (userData == null)
                        {
                            ModelState.AddModelError("", "Unable to get User Information. Try again later.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to get User Information. Try again later.");
                    }
                    
                }

                return View(userData);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error while fetching user Information. Try again later.");
                return View(userData);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> EditCustomer(RegisterUserVM updateData)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    if (updateData.Password != updateData.ConfirmPassword)
                    {
                        ModelState.AddModelError("", "Paswords don't match.");
                        return View(updateData);
                    }
                    var userData = await userManager.FindByEmailAsync(updateData.Email);
                    IdentityResult result1 = null;
                    if (userData != null)
                    {
                        userData.FirstName = updateData.FirstName;
                        userData.LastName = updateData.LastName;
                        userData.Password = updateData.Password;
                        var result2 = await userManager.UpdateAsync(userData);
                        if (!string.IsNullOrEmpty(updateData.Password) && !string.IsNullOrEmpty(updateData.ConfirmPassword))
                        {
                            var token = await userManager.GeneratePasswordResetTokenAsync(userData);
                            result1 = await userManager.ResetPasswordAsync(userData, token, updateData.Password);
                        }
                        if (result2.Succeeded && result1.Succeeded)
                        {
                            ViewBag.ResultStatus = "success";
                        }
                        else
                        {
                            ViewBag.ResultStatus = "failed";
                        }
                    }
                }
                else
                {
                    ViewBag.ResultStatus = "0";
                    ModelState.AddModelError("", "Provide Compelte information");
                }
                return View(updateData);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Provide Compelte information");
                ViewBag.ResultStatus = "error";
                return View(updateData);
            }
        }

        [HttpPost]
        public async Task<JsonResult> RemoveCustomer(string email)
        {
            string res = "";
            try
            {
                var userData = await userManager.FindByEmailAsync(email);
                var user = await userManager.FindByIdAsync(userData.Id);
                if (userData != null)
                {
                    var res1 = await userManager.RemoveFromRoleAsync(userData, "Customer");
                    if (res1.Succeeded)
                    {
                        var res2 = await userManager.DeleteAsync(userData);
                        if (res2.Succeeded)
                        {
                            //Remove from the TransactionHistory and AccountTable
                            var AccountData = _context.AccountInformation.Where(x => x.UserId == new Guid(userData.Id)).FirstOrDefault();
                            if (AccountData != null)
                            {
                                _context.AccountInformation.Remove(AccountData);
                            }
                            var TransactionHistoryData = _context.TransactionHistory.Where(x => x.FK_UserId == new Guid(userData.Id)).ToList();
                            if (TransactionHistoryData.Count > 0)
                            {
                                _context.TransactionHistory.RemoveRange(TransactionHistoryData);
                            }
                            _context.SaveChanges();
                        }
                    }
                }
                _context.SaveChanges();
                res = "success";
            }
            catch (Exception ex)
            {
                res = "failed";
            }
            return Json(res);
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
