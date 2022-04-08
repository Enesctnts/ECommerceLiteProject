using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ECommerceLiteBLL.Account.MembershipTools;
using ECommerceLiteEntity.Enums;
using Microsoft.AspNet.Identity;
using ECommerceLiteEntity.IdentityModels;
namespace ECommerceLiteUI
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //Application_Start: Uygulama ilk kez çalıştıgında bir defaya mahsus olmak üzere çalışır.
            //Bu nedenle ben uygulama ilk kez çalıştıgında DB'de Roller ekli mi diye bakmak istiyorum.
            //Ekli değilse rolleri Enum'dan çagırıp ekleyelim.Ekli ise bişey yapmaya gerek kalmıyor.

            //adım1: Rollere bakıcagım şey --> Role Manager
            var myRoleManager = MembershipTools.NewRoleManager();
            //Adım2:Rollerin isimlerini almak için (ipucu-->Enum)
            var allRoles = Enum.GetNames(typeof(Roles));
            //Bize gelen diziyi tek tek döneceğiz(Döngü)
            foreach (var item in allRoles)
            {
                //adım4: Acaba b rol Db'de ekli mi?
                if (!myRoleManager.RoleExists(item))//Eğer bu role ekli değilse ?
                {
                    //Adım5: Rolü ekle!
                    myRoleManager.Create(new ApplicationRole()
                    {
                        Name = item
                    });
                }
            }

            
        }
        protected void Application_Error()
        {
            //Not: İhtiyacım olursa internetten Global.asax'ın metodlarına bakıp kullanabilirim
            //Örn: Application_Error: Uygulama içinde istenmeyen bir hata meydana geldiğinde çalışır.
            //Bu metodu yazarsak o hatayı loglayıp sorunu çözebilirsiniz.

            Exception exception = Server.GetLastError();
            //ex loglanacak
        }
    }
}
