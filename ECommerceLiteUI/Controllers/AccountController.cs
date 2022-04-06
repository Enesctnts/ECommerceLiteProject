using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECommerceLiteBLL.Account;
using ECommerceLiteBLL.Account.MembershipTools;
using ECommerceLiteBLL.Repository;
using ECommerceLiteBLL.Settings;
using ECommerceLiteEntity.IdentityModels;
using ECommerceLiteEntity.Models;
using ECommerceLiteUI.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ECommerceLiteEntity.Enums;
using System.Threading.Tasks;
using ECommerceLiteEntity.ViewModels;

namespace ECommerceLiteUI.Controllers
{
    public class AccountController : BaseController
    {

        //Global alan
        //not: Bir sonraki projede repoları newlemeyeceğiz !
        //çünkü bu bağımlılık oluşturur. Bir sonraki projede bağımlılıkları tersinne çevirme işlemi olarak bilinen dependency ınjection işlemleri yapacağız.

        CustomerRepo myCustomerRepo = new CustomerRepo();
        PassiveUserRepo myPassiveUserRepo = new PassiveUserRepo();
        UserManager<ApplicationUser> myUserManager = MembershipTools.NewUserManager();
        UserStore<ApplicationUser> myUserStore = MembershipTools.NewUserStore();
        RoleManager<ApplicationRole> myRoleManager = MembershipTools.NewRoleManager();

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid) // model validasyonları sağlandımı ?
                {
                    return View(model);
                }

                var checkUserTC = myUserStore.Context.Set<Customer>().FirstOrDefault(p => p.TCNumber == model.TCNumber)?.TCNumber;
                if (checkUserTC != null)
                {
                    ModelState.AddModelError("", "Bu Tc numarasıyla ile daha önceden sisteme kayıt yapılmıştır!");
                    return View(model);
                }


                //To do: soru işareti silinip doğrulanacak.
                var checkUserEmail = myUserStore.Context.Set<ApplicationUser>().FirstOrDefault(x => x.Email == model.Email)?.Email;
                if (checkUserEmail != null)
                {
                    ModelState.AddModelError("", "Bu email ile daha önceden sisteme kayıt yapılmıştır!");
                    return View(model);
                }

                //aktivasyon kodu üretelim

                var activationCode = Guid.NewGuid().ToString().Replace("-", "");

                //Artık sisteme kayıt olabilir..
                var newUser = new ApplicationUser()
                {
                    Name = model.Name,
                    Surname = model.Surname,
                    Email = model.Email,
                    UserName = model.TCNumber,
                    ActivationCode = activationCode
                };

                
                //artık ekleyelim

                var createResult = myUserManager.CreateAsync(newUser, model.Password);

                //To Do: createResult.Isfault ne acaba? debuglarken bakalım
                if (createResult.Result.Succeeded)
                {
                    //görev başarıyla tamamlandıysa kişi aspnetusers tablosuna eklenmiştir.
                    //Yeni Kayıt oldugu için bu kişiyi pasif rol verilecektir!
                    //Kişi emailini gelen aktivasyon koduna tıklarsa pasiflikten çıkıp customer olabilir.

                    await myUserManager.AddToRoleAsync(newUser.Id, Roles.Passive.ToString());
                    PassiveUser myPassiveUser = new PassiveUser()
                    {
                        UserId = newUser.Id,
                        TCNumber = model.TCNumber,
                        IsDeleted = false,
                        LastActiveTime = DateTime.Now
                    };
                    //myPassiveUserRepo.Insert(myPassiveUser)
                    await myPassiveUserRepo.InsertAsync(myPassiveUser);

                    //email gönderilecek
                    //Site adresini alıyoruz.
                    var siteUrl = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                    await SiteSettings.SendMail(new MailModel()
                    {
                        To = newUser.Email,
                        Subject = "ECommerceLite Site Aktivasyob Emaili",
                        Message = $"Merhaba {newUser.Name} {newUser.Surname}," + 
                        $"<br/>Hesabınızı aktifleştirmek için <b>" +
                        $"<a href='{siteUrl}/Account/Activation?" +
                        $"code{activationCode}'>Aktivasyon Linkine</a></b> tıklayınız..."
                    });

                    //işlemler bitti...
                    return RedirectToAction("Login", "Account", new { email = $"{newUser.Email}" });





                }

                else
                {
                    ModelState.AddModelError("", "Kayıt işleminde beklenmedik hata oluştu");
                    return View(model);
                }



            }
            catch (Exception)
            {
                //To Do Loglama yapılacak
                ModelState.AddModelError("", "Beklenmedik bir hata oluştu! Tekrar Deneyiniz!");
                return View(model);
            }
        }


        [HttpGet]
        public async Task<ActionResult> Activation(string code)
        {
            try
            {
                var user = myUserStore.Context.Set<ApplicationUser>().FirstOrDefault(x => x.ActivationCode == code);
                if (user==null)
                {
                    ViewBag.ActivationResult = "Activation işlemi başarısız! Sistem yöneticisinden yeniden email isteyiniz..";
                }
                //user bulundu!
                if (user.EmailConfirmed)//zaten aktifleşmiş mi?
                {
                    ViewBag.ActivationResult = "Aktivasyon işleminiz zaten gerçekleşmiştir! Giriş yaparak sistemi kullanabilirsiniz";
                    return View();
                }
                user.EmailConfirmed = true;
                await myUserStore.UpdateAsync(user);
                await myUserStore.Context.SaveChangesAsync();
                //Bu kişi artık aktif
                PassiveUser passiveUser = myPassiveUserRepo.AsQueryable().FirstOrDefault(x => x.UserId == user.Id);
                if (passiveUser != null)
                {
                    //To Do : 

                    passiveUser.IsDeleted = true;
                    myPassiveUserRepo.Update();
                    Customer customer = new Customer()
                    {
                        UserId = user.Id,
                        TCNumber = passiveUser.TCNumber,
                        IsDeleted = false,
                        LastActiveTime = DateTime.Now
                    };
                }


            }
            catch (Exception ex)
            {
                // To Do: Loglama yapılacak.
                ModelState.AddModelError("", "Beklenmedik bir hata oluştu!");
                return View();

            }
        }
    }
} 