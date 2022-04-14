﻿using ECommerceLiteBLL.Repository;
using ECommerceLiteBLL.Settings;
using ECommerceLiteEntity.Models;
using ECommerceLiteUI.Models;
using Mapster;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECommerceLiteUI.Controllers
{
    public class ProductController : Controller
    {
        //Bu controllera Admin gibi  yetkili kişiler erşebilcektir. Bu arada ürünlerin listelenmesi, ekleme, silme, güncelleme işlemleri yapılacaktır
        CategoryRepo myCategoryRepo = new CategoryRepo();
        ProductRepo myProductRepo = new ProductRepo();
        ProductPictureRepo myProductPictureRepo = new ProductPictureRepo();
        public ActionResult ProductList(string Search = "")
        {

            List<Product> allProducts = new List<Product>();
            
            if (string.IsNullOrEmpty(Search))
            {
                allProducts = myProductRepo.GetAll();
            }
            else
            {
                allProducts=myProductRepo.GetAll().Where(x=>x.ProductName.ToLower().Contains(Search.ToLower()) || x.Description.ToLower().Contains(Search.ToLower())).ToList();
            }
            return View(allProducts);
        }

        [HttpGet]
        public ActionResult Create()
        {
            //Sayfayo çagırırken ürünün kategorisinini seçmesi lazıım bu nedenle sayfaya kategoriler gitmeli
            List<SelectListItem> subCategories = new List<SelectListItem>();
            //Linq
            //Select*from Categories where BaseCategoryId is not null bu sorguyu yapar. bu sorgudan 2 değer gelir. bu sorguyu yaparak verileri getiriyoruz bu olayı bize As@ueryable sağlıyor. Enumerable da kullanılıyor o butün veriyi getiriyo sonra aralarından bunları çekiyo.

            myCategoryRepo.AsQueryable().Where(x => x.BaseCategoryId != null).ToList().
                ForEach(x => subCategories.Add(new SelectListItem()
                {
                    Text = x.CategoryName,
                    Value = x.Id.ToString()
                }));

            ViewBag.SubCategories = subCategories;

            return View();
        }

        [HttpPost]
        public ActionResult Create(ProductViewModel model)
        {
            try
            {
                //Sayfayo çagırırken ürünün kategorisinini seçmesi lazıım bu nedenle sayfaya kategoriler gitmeli
                List<SelectListItem> subCategories = new List<SelectListItem>();
                //Linq
                //Select*from Categories where BaseCategoryId is not null bu sorguyu yapar. bu sorgudan 2 değer gelir. bu sorguyu yaparak verileri getiriyoruz bu olayı bize As@ueryable sağlıyor. Enumerable da kullanılıyor o butün veriyi getiriyo sonra aralarından bunları çekiyo.

                myCategoryRepo.AsQueryable().Where(x => x.BaseCategoryId != null).ToList().
                    ForEach(x => subCategories.Add(new SelectListItem()
                    {
                        Text = x.CategoryName,
                        Value = x.Id.ToString()
                    }));

                ViewBag.SubCategories = subCategories;
                if (!ModelState.IsValid)
                {
                    ModelState.AddModelError("", "Veri girişleri düzgün olmalıdır");
                    return View(model);

                }
                if (model.CategoryId <= 0 || model.CategoryId > myCategoryRepo.GetAll().Count())
                {
                    ModelState.AddModelError("", "Ürüne ait kategori seçilmelidir");
                    return View(model);
                }

                //Burada kontrol lazım
                //Acaba girdiği ürün kodu bizim db de var mı?

                if (myProductRepo.IsSameProductCode(model.ProductCode))
                {
                    ModelState.AddModelError("", "Dikkat! Girdiğiniz ürün kodu sistemdeki bir başka ürüne aittir.Ürün kodları benzersiz olmalıdır");
                    return View(model);
                }

                //Ürün tabloya kayıt olacak
                //To Do: Mapleme yapılacak

                //Product product = new Product()
                //{
                //    ProductName = model.ProductName,
                //    Description = model.Description,
                //    ProductCode = model.ProductCode,
                //    CategoryId = model.CategoryId,
                //    Discount = model.Discount,
                //    Quantity = model.Quantity,
                //    RegisterDate = DateTime.Now,
                //    Price = model.Price
                //};
                //Mapleme yapıldı
                //Mapster paketi indirildi.Mapster bir objedeki veriileri diger bir objeye zahmetsizce aktarır. Aktarım yapabilmesi için A objesiyle B objesinin içindeki propertylerin ismileri ve tipleri birebir aybı olamlıdır.
                //Bu projede mapster kullandık Core projesinde daha profesyonel olan AutoMapper' ı kullancagız. Bir dto objesinin içerisindeli verileri dto objesinin içendeki propertyleri aktarır.


                Product product = model.Adapt<Product>();
               //2.yol 
               //Product product2 = model.Adapt<ProductViewModel, Product>();

                int insertResult = myProductRepo.Insert(product);
                if (insertResult > 0)
                {
                    //Sıfırdan büyükse product tabloya eklendi
                    //Acaba bu producta resim seçilmiş mi? resim seçtiyse o resimlerin yollarını kayıt et
                    if (model.Files.Any())
                    {
                        ProductPicture productPicture = new ProductPicture();
                        productPicture.ProductId = product.Id;
                        productPicture.RegisterDate = DateTime.Now;
                        int counter = 1; //Bizim sistemde resim adeti 5 olarak belirlendiği için
                        foreach (var item in model.Files)
                        {
                            if (counter == 5)
                            {
                                break;
                            }
                            if (item != null && item.ContentType.Contains("image") && item.ContentLength > 0)
                            {
                                string filename = SiteSettings.StringCharacterConverter(model.ProductName).ToLower().Replace("-", "");
                                string extensionName = Path.GetExtension(item.FileName);
                                string guid = Guid.NewGuid().ToString().Replace("-", "");
                                string directoryPath = Server.MapPath($"~/ProductPictures/{filename}/{model.ProductCode}");

                                string filePath = Server.MapPath($"~/ProductPictures/{filename}/{model.ProductCode}/") + filename + "-" + counter + "-" + guid + extensionName;
                                if (!Directory.Exists(directoryPath))
                                {
                                    Directory.CreateDirectory(directoryPath);
                                }

                                item.SaveAs(filePath);
                                //To Do Buraya birisi çeki düzen versin
                                if (counter == 1)
                                {
                                    productPicture.ProductPicture1 = $"ProductPictures/{filename}/{model.ProductCode}/" + filename + "-" + counter + "-" + guid + extensionName;
                                }
                                if (counter == 2)
                                {
                                    productPicture.ProductPicture2 = $"ProductPictures/{filename}/{model.ProductCode}/" + filename + "-" + counter + "-" + guid + extensionName;
                                }
                                if (counter == 3)
                                {
                                    productPicture.ProductPicture3 = $"ProductPictures/{filename}/{model.ProductCode}/" + filename + "-" + counter + "-" + guid + extensionName;
                                }


                            }
                            counter++;
                        }


                        int productPictureInsertResult = myProductPictureRepo.Insert(productPicture);
                        if (productPictureInsertResult > 0)
                        {
                            return RedirectToAction("ProductList", "Product");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Ürün eklendi ama ürüne ait fotoğraflar eklenirken beklenmedik bir hata oluştu! Ürününüzün fotoğrafını daha sonra tekrar eklemeyi deneyebilirsiniz.");
                            return View(model);
                        }

                    }

                    else
                    {
                        return RedirectToAction("ProductList", "Product");
                    }

                }
                else
                {
                    ModelState.AddModelError("", "HATA: Ürün ekleme işleminde bir hata oluştu! Tekrar deneyiniz!");
                    return View(model);
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Beklenmedik bir hata oluştu");
                //ex loglanacak
                return View(model);
            }
        }
    }
}