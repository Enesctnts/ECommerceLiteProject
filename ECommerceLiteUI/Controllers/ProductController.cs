﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECommerceLiteUI.Controllers
{
    public class ProductController : Controller
    {
        //Bu controllera Admin gibi  yetkili kişiler erşebilcektir. Bu arada ürünlerin listelenmesi, ekleme, silme, güncelleme işlemleri yapılacaktır
       public ActionResult ProductList()
        {
            return View();
        }
    }
}