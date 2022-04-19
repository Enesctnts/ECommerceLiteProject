using ECommerceLiteBLL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECommerceLiteUI.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {
        OrderRepo myOderRepo = new OrderRepo();

        public ActionResult Dashboard()
        {
            //Bu ay oluşturulan sipariş sayısını ekranda görelim
            var orderlist = myOderRepo.GetAll();
            var CurrentMontlyOrderCount = orderlist.Where(x => x.RegisterDate >= DateTime.Now.AddMonths(-1)).ToList().Count;
            ViewBag.NewOrderCount = CurrentMontlyOrderCount;
            return View();
        }
        public ActionResult Orders()
        {
            return View();
        }

    }
}