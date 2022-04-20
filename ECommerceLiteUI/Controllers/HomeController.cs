using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECommerceLiteBLL.Repository;
using ECommerceLiteUI.Models;
using Mapster;

namespace ECommerceLiteUI.Controllers
{
    public class HomeController : Controller
    {
        CategoryRepo myCategoryRepo = new CategoryRepo();
        ProductRepo myProductRepo = new ProductRepo();
        public ActionResult Index()
        {
            //Ana kategorileri viewbag ile sayfaya gönderelim
            var categoryList = myCategoryRepo.AsQueryable().Where(x => x.BaseCategoryId == null).Take(6).ToList();

            ViewBag.CategoryList = categoryList.OrderByDescending(x => x.Id).ToList();

            //ürünler 
            var productList = myProductRepo.AsQueryable().Where(x => x.IsDeleted == false && x.Quantity >= 1).Take(10).ToList();
            List<ProductViewModel> model = new List<ProductViewModel>();

            //Mapster ile mapledik
            productList.ForEach(x =>
            {
                var item = x.Adapt<ProductViewModel>();
                item.GetCategory();
                item.GetProductPicture();
                model.Add(item);
            });

            //2.yol
            //foreach (var item in productList)
            //{

            //    //mapster 
            //    //model.Add(item.Adapt<ProductViewModel>())
            //    var product = new ProductViewModel()
            //    {
            //        Id = item.Id,
            //        CategoryId = item.CategoryId,
            //        ProductName = item.ProductName,
            //        Description = item.Description,
            //        Quantity = item.Quantity,
            //        Discount = item.Discount,
            //        RegisterDate = item.RegisterDate,
            //        Price = item.Price,
            //        ProductCode = item.ProductCode
            //        //Isdeleted alanını viewmodel içine eklemeyi unuttuk.Çünkü isdeleted alanını daaha dün ekledik.Viewmodeli geçen hafta oluşturduk.
            //    };
            //    product.GetCategory();
            //    product.GetProductPicture();
            //    model.Add(product);

            //}

            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult AddToCart(int id)
        {
            try
            {
                //Session'a eklenecek
                //Session oturum demektir
                var shoppingCart = Session["ShoppingCart"] as List<ProductViewModel>;
                if (shoppingCart==null)
                {
                    shoppingCart = new List<ProductViewModel>();
                }
                if (0<id)
                {
                    var product = myProductRepo.GetById(id);
                    if (product==null)
                    {
                        TempData["AddToCartFailed"] = "Ürün eklemesi başarısızdır. Lütfen tekrar deneyiniz";
                        //Product null geldi logla
                        return RedirectToAction("Index", "Home");
                    }
                    //Tamam ekleme yapılacak 
                    //ProductViewModel productAddToCart = product.Adapt<ProductViewModel>();

                    var productAddToCart = new ProductViewModel()
                    {
                        Id = product.Id,
                        ProductName = product.ProductName,
                        Description = product.Description,
                        Discount = product.Discount,
                        Price = product.Price,
                        Quantity = product.Quantity,
                        CategoryId = product.CategoryId,
                        RegisterDate = product.RegisterDate,
                        ProductCode = product.ProductCode
                    };
                    //Aynı üründen varsa miktar artırılacak
                    if (shoppingCart.Count(x=>x.Id== productAddToCart.Id) >0)
                    {
                        shoppingCart.FirstOrDefault(x => x.Id == productAddToCart.Id).Quantity++;

                    }
                    else
                    {
                        //Aynı üründen yoksa miktar 1 olucak ve sepete ekledik.
                        productAddToCart.Quantity = 1;
                        shoppingCart.Add(productAddToCart);
                    }
                    //Önemli --> Session'a bu listeyi atamamız gerekli
                    Session["ShoppingCart"] = shoppingCart;
                    TempData["AddToCardSuccess"] = "Ürün sepete eklendi";
                    return RedirectToAction("Index", "Home");

                }
                else
                {
                    TempData["AddToCartFailed"] = "Ürün eklemesi başarısızdır. Lütfen tekrar deneyiniz";
                    //Loglama yap id düzgün gelmedi
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {

                //ex loglanacak
                TempData["AddToCartFailed"] = "Ürün eklemesi başarısızdır. Lütfen tekrar deneyiniz";
                return RedirectToAction("Index", "Home");
            }
        }

    }
}