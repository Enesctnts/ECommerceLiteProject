﻿using ECommerceLiteBLL.Repository;
using ECommerceLiteUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECommerceLiteUI.Controllers
{
    public class PartialsController : Controller
    {
        CategoryRepo myCategoryRepo = new CategoryRepo();
        ProductRepo myProductRepo = new ProductRepo();
        public PartialViewResult AdminSideBarResult()
        {
            return PartialView("_PartialAdminSideBar");
        }

        public PartialViewResult AdminSideBarMenuResult()
        {
            TempData["CategoryCount"] = myCategoryRepo.GetAll().Count();
            return PartialView("_PartialAdminSideBarMenu");
        }
        
        public PartialViewResult AdminSideBarProducts()
        {
            TempData["ProductCount"] = myProductRepo.GetAll().Count();
            return PartialView("_PartialAdminSideBarProduct");
        }

        public PartialViewResult ShoppingCart()
        {
            var shoppingCart = Session["ShoppingCart"]  as List<ProductViewModel>;
            if (shoppingCart == null)
            {
                return PartialView("_PartialShoppingCart",
                    new List<ProductViewModel>());
            }
            else
            {
                foreach (var item in shoppingCart)
                {
                    item.GetProductPicture();
                }
                // yada linq foreach ile daha kısa işlem yapabilirsiniz.
                //  shoppingCart.ForEach(x => x.GetProductPictures());

                return PartialView("_PartialShoppingCart", shoppingCart);
            }

        }
        
    }
}