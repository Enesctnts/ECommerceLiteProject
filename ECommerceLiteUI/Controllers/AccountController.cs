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
using Microsoft.Owin.Security;

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

                    await myCustomerRepo.InsertAsync(customer);

                    //Aspnet tablosuna da bu kiişinin customer oldugununu bildirmek gerek
                    myUserManager.RemoveFromRole(user.Id, Roles.Passive.ToString());
                    myUserManager.AddToRole(user.Id, Roles.Customer.ToString());

                    //işlem bitti başarılı old. dair mesajı gönderelim.

                    ViewBag.ActivationResult = $"Merhaba sayın{user.Name} {user.Surname}, aktifleştirme işleminiz başarılıdır! Giriş yapıp sistemi kullanabilirsiniz";
                    return View();

                }

                //Not: Müsait oldugunuzda bireysel bir beyin fırtınası yapabilirsiniz. Kendinize şu soruyu sorun ! PassiveUser null gelirse nasıl bir yol izlenileblir? PassiveUser null gelmesi çok büyük bir problem mi? Customerda bu kişi kayıtlı mı? Customerda bir proble yok.. Customer kayıtlı değilse Problem varr!!!

                //Buraya yazılması gereken mini minnacık kodları şimdilik size bırakmış gibi yapıyorum
                return View();
            }
            catch (Exception ex)
            {
                // To Do: Loglama yapılacak.
                ModelState.AddModelError("", "Beklenmedik bir hata oluştu!");
                return View();

            }
        }
    
    
        [HttpGet]
        [Authorize]
        public ActionResult UserProfile()
        {
            //Login olmuş kişinin id bilgisini alalım 
            var user = myUserManager.FindById(HttpContext.User.Identity.GetUserId());
            if (user !=null)
            {
            //Kişi bulacagız ve mevcut bilgileri ProfileViewModele atayıp sayfayla göndereceğiz.
                ProfileViewModel model = new ProfileViewModel()
                {
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email,
                    TCNumber = user.UserName
                };
                return View(model);
            }
            //User null ise (temkinli davrandık...)
            ModelState.AddModelError("", "Beklenmedik bir sorun oluşmuş olabilir mi ? Giriş yapıp,tekrar deneyiniz! Sizinle tekrar buluşalım!");
            return View();

        }
    
        
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserProfile(ProfileViewModel model)
        {
            try
            {
                //Sisteme kayıt olmuş ve login ile giriş yapmış kişi  hesabıma tıkladı. Bilgilerini gördü.Bilgilerini gördü. Bilgilerinde değişiklik yaptı.Biz burada kontrol edeceğiz.Yapılan değişikleri tespit edip db'mizi güncelleyebileceğiz.
                var user = myUserManager.FindById(HttpContext.User.Identity.GetUserId());
                if (user==null)
                {
                    ModelState.AddModelError("", "MEvcut kullanıcı bilgilerinize ulaşılamadıgı için işlem yapamıyoruz.");
                    return View(model);

                }
                //Bir user herhangi bir bilgisini değiştirecekse Parolasını girmek zorunda.
                //Bu nedenle model ile gelen parola Db'deki parola ile eşleşiyor mu diye bakmak lazım...
                if (myUserManager.PasswordHasher.VerifyHashedPassword(user.PasswordHash,model.Password)==PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError("", "Mevcut şifrenizi yanlış girdiğiniz için güncelleyemedik! lütfen tekrar deneyiniz!");
                    return View(model);
                }

                //Başarılıysa yani parolayı doğru yazdı!
                //bilgileri güncelleyeceğiz

                user.Name = model.Name;
                user.Surname = model.Surname;
                await myUserManager.UpdateAsync(user);
                ViewBag.Result = "Bilgileriniz güncellendi";
                var updatedModel = new ProfileViewModel()
                {
                    Name = user.Name,
                    Surname = user.Surname,
                    TCNumber = user.UserName,
                    Email = user.Email
                };
                return View(updatedModel);


            }
            catch (Exception ex)
            {
                // ex loglanacak
                ModelState.AddModelError("", "Beklenmedik bir hata oluştu! Tekrar deneyiniz");
                return View(model);
            }
        }
 
    
        [HttpGet]
        [Authorize]
        public ActionResult UpdatePassword()
        {
            var user = myUserManager.FindById(HttpContext.User.Identity.GetUserId());
            if (user!=null)
            {
                ProfileViewModel model = new ProfileViewModel()
                {
                    Email=user.Email,
                    Name=user.Name,
                    Surname=user.Surname,
                    TCNumber=user.UserName
                };
                return View();

            }
            ModelState.AddModelError("", "Sisteme giriş yapmamız gerekmektedir");
            return View();
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdatePassword(ProfileViewModel model)
        {
            try
            {
                //Mevcut logino lmuş kişinin Id isini veriyor. o id ile manager kişiyi db'den bulup 
                var user = myUserManager.FindById(HttpContext.User.Identity.GetUserId());

                //Ya eski şifresi ile yeni şifresi aynı aynıysa
                if (myUserManager.PasswordHasher.VerifyHashedPassword(user.PasswordHash,model.NewPassword)==PasswordVerificationResult.Success)
                {
                    //Bu kişi mevcut şifresinin aynısını yeni şifre olarak yutturmaya çalışıyor.
                    ModelState.AddModelError("", "Yeni şifreniz mevcut şifrenizle aynı olmasın madem değiştirmek istedin!");
                    return View(model);
                }
                //Yeni şifre ile mevcut tekrarı uyuşuyor mu?

                if (model.NewPassword!= model.ConfirmPassword)
                {
                    ModelState.AddModelError("", "Şifreler uyuşmuyor");
                    return View(model);
                }
                

                //Acana mevcut şifresini doğru yazdı mı?
                var checkCurrent = myUserManager.Find(user.UserName, model.Password);
                if (checkCurrent==null)
                {
                    //Mevcut şifresini yanlış yazmış
                    ModelState.AddModelError("", "Mevcutşifrenizi yanlış girdiğiniz yeni şifre oluşturma işleminiz başarısız oldu! Tekrar deneyiniz");
                    return View();
                }

                //Artık şifresini değiştirebilir.

                await myUserStore.SetPasswordHashAsync(user, myUserManager.PasswordHasher.HashPassword(model.NewPassword));

                await myUserManager.UpdateAsync(user);

                //şifre değiştirdikten sonra sistemden atalım!
                TempData["PasswordUpdated"] = "Paralonız değiştirildi";
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Login", "Account", new { email = user.Email });


            }
            catch (Exception ex)
            {

                //ex loglanacak

                ModelState.AddModelError("", "Beklenmedik hata oldu! Tekrar deneyiniz");
                return View(model);
            }
        }


        [HttpGet]
        public ActionResult RecoverPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> RecoverPassword(ProfileViewModel model)
        {
            try
            {
                //Şifresini unutmuş
                //1.Yönteml
                //var user = myUserStore.Context.Set<ApplicationUser>().FirstOrDefault(p => p.Email == model.Email);
                //2.Yöntem
                var user = myUserManager.FindByEmail(model.Email);

                if (user==null)
                {
                    ViewBag.RecoverPassword = "Sistemde böyle bir kullanıcı olmadığı için size yeni bir şifre gönderemiyoruz! Lütfen önce sisteme kayıt olunuz.";
                    return View(model);
                }

                //Random şifre oluştur!
                var randomPassword = CreateRandomNewPassword();
                await myUserStore.SetPasswordHashAsync(user, myUserManager.PasswordHasher.HashPassword(randomPassword));
                await myUserStore.UpdateAsync(user);

                //email gönderilecek
                //Site adresini alıyoruz.
                var siteURL = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                await SiteSettings.SendMail(new MailModel()
                {
                    To = user.Email,
                    Subject = "ECommerceLite - Şifre Yenilendi!",
                    Message = $"Merhaba {user.Name} {user.Surname}," +
                    $"<br/>Yeni şifreniz:<b> {randomPassword} </b> Sisteme Giriş" +
                    $"yapmak için <b>" +
                    $"<a href='{siteURL}/Account/Login?" +
                    $"email={user.Email}'>BURAYA</a></b> tıklayınız..."
                });

                //işlemler bitti...
                ViewBag.RecoverPassword = "Email adresinize şifre gönderilmiştir.";
                return View();
            }
            catch (Exception ex)
            {

                //To Do ex loglanacak
                ViewBag.RecoverPasswordResult = "Sistemsel bir hata oluştu! Tekrar deneyiniz!";
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult Login(string returnUrl , string email)
        {
            try
            {
                //To Do: sayfa patlamazsa if kontrolü gerek yok! test ederken bakacağız.
                var model = new LoginViewmodel()
                {
                    ReturnUrl=returnUrl,
                    Email=email
                };
                return View(model);
            }
            catch (Exception ex)
            {
                //ex loglanacak
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewmodel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await myUserManager.FindAsync(model.Email, model.Password);

                if (user==null)
                {
                    ModelState.AddModelError("", "Emailinizi yada şifrenizi yanlış girdiniz!");
                    return View(model);
                }
                //User bulduk ama rolü pasif ise sisteme girmelisin!
                if (user.Roles.FirstOrDefault().RoleId == 
                    myRoleManager.FindByName(Enum.GetName(typeof(Roles), Roles.Passive)).Id) 
                {
                    ViewBag.Result = "Sistemi kullanmak için aktivasyon yapmanız gerekmektedir! Emailinizi gönderilen aktivasyon linkini tıklayınız!";
                    //To Do: Zaman Kalırsa Email gönder adında küçük bir buton burada olsun 
                    return View(model);
                }
                //Artık login olabilir 

                var authManager = HttpContext.GetOwinContext().Authentication;
                var userIdentity = await myUserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                
                //Owin kullanıyoruz.. araştır
                // 2.yol //AuthenticationProperties auth = new AuthenticationProperties();
                //auth.IsPersistent = model.RememberMe;
                //authManager.SignIn(auth, userIdentity);

                //1.yol
                authManager.SignIn( new AuthenticationProperties() { IsPersistent=model.RememberMe } , userIdentity );


                //Giriş yaptı! Peki nereye gidercek?
                //Herkes rolünü uygun defauly bir sayfaya gitsin

                if (user.Roles.FirstOrDefault().RoleId ==
                    myRoleManager.FindByName(Enum.GetName(typeof(Roles),Roles.Admin)).Id)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (string.IsNullOrEmpty(model.ReturnUrl))
                {
                    return RedirectToAction("Index", "Home");
                }

                //ReturnUrl dolu ise?
                var url = model.ReturnUrl.Split('/');//Split ettik
                if (url.Length==4)
                {
                    return RedirectToAction(url[2], url[1], new { id = url[3] });
                }
                else
                {
                    //Örn:  RedirectToAction("UserProfile", "Account");
                    return RedirectToAction(url[2], url[1]);
                }


            }
            catch (Exception ex)
            {
                //ex loglanacak
                ModelState.AddModelError("", "Beklenmedik hata oluştu! Tekrar deneyiniz");
                return View(model);
            }
        }


        [Authorize]
        public ActionResult Logout()
        {
            Session.Clear();
            var user = MembershipTools.GetUser();
            HttpContext.GetOwinContext().Authentication.SignOut();
            return RedirectToAction("Login", "Account",new { email = user.Email });

        }
    }
} 
