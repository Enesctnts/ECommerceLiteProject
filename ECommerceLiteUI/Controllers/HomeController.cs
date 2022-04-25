using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECommerceLiteBLL.Repository;
using ECommerceLiteUI.Models;
using Mapster;
using System.Threading.Tasks;
using ECommerceLiteBLL.Account;
using ECommerceLiteEntity.Models;
using QRCoder;
using System.Drawing;
using ECommerceLiteBLL.Settings;
using ECommerceLiteEntity.ViewModels;
using ECommerceLiteBLL.Account.MembershipTools;
using ECommerceLiteUI.LogManaging;

namespace ECommerceLiteUI.Controllers
{
    public class HomeController : BaseController
    {
        CategoryRepo myCategoryRepo = new CategoryRepo();
        ProductRepo myProductRepo = new ProductRepo();
        AdminRepo myAdminRepo = new AdminRepo();
        CustomerRepo myCustomerRepo = new CustomerRepo();
        OrderRepo myOrderRepo = new OrderRepo();
        OrderDetailRepo myOrderDetailRepo = new OrderDetailRepo();
        public ActionResult Index()
        {
            //Ana kategorileri viewbag ile sayfaya gönderelim
            var categoryList = myCategoryRepo.AsQueryable().Where(x => x.BaseCategoryId == null).Take(3).ToList();

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

        [Authorize]
        public async Task<ActionResult> Buy()
        {
            try
            {
                //1) Eğer müşteri değil isen alışveriş yapamazsın!
                var user = MembershipTools.GetUser();
                var customer = myCustomerRepo.AsQueryable()
                   .FirstOrDefault(x => x.UserId == user.Id);
                if (customer == null)
                {
                    TempData["BuyFailed"] = "Alışveriş yapabilmeniz için MÜŞTERİ bilgileriniz ile giriş yapmanız gereklidir!";
                    return RedirectToAction("Index", "Home");
                }
                //2)shoppingcart null mı değil mi?
                var shoppingcart = Session["ShoppingCart"] as List<ProductViewModel>;
                if (shoppingcart == null)
                {
                    TempData["BuyFailed"] = "Alışveriş yapmak için sepetenize ürün ekleniyiniz!";
                    return RedirectToAction("Index", "Home");
                }
                //3) shoppingcart içinde ürün var mı ?
                if (shoppingcart.Count == 0)
                {
                    TempData["BuyFailed"] = "Alışveriş sepetinizde ürün bulunmamaktadır!";
                    return RedirectToAction("Index", "Home");
                }

                //Artık alışveriş tamamlansın
                Order customerOrder = new Order()
                {
                    CustomerTCNumber = customer.TCNumber,
                    IsDeleted = false,
                    OrderNumber = customer.TCNumber // burayı düzelteceğiz
                };
                //insert yapılsın
                int orderInsertResult = myOrderRepo.Insert(customerOrder);
                if (orderInsertResult > 0)
                {
                    //siparişin detayları orderdetaile eklenmeli.
                    int orderDetailsInsertResult = 0;
                    foreach (var item in shoppingcart)
                    {
                        OrderDetail customerOrderDetail = new OrderDetail()
                        {
                            OrderId = customerOrder.Id,
                            IsDeleted = false,
                            ProductId = item.Id,
                            ProductPrice = item.Price,
                            Quantity = item.Quantity,
                            Discount = item.Discount
                        };
                        //TotalCount hesabı
                        if (item.Discount > 0)
                        {
                            customerOrderDetail.TotalPrice =
                                customerOrderDetail.Quantity *
                                (customerOrderDetail.ProductPrice -
                                (customerOrderDetail.ProductPrice *
                               ((decimal)customerOrderDetail.Discount / 100)
                                ));
                        }
                        else
                        {
                            //3 * telefonun fiyatı
                            customerOrderDetail.TotalPrice =
                                customerOrderDetail.Quantity * customerOrderDetail.ProductPrice;
                        }
                        //orderdetail tabloya insert edilsin
                        orderDetailsInsertResult += myOrderDetailRepo.Insert(customerOrderDetail);

                    }
                    //orderDetailsInsertResult büyükse sıfırdan
                    if (orderDetailsInsertResult > 0 &&
                        orderDetailsInsertResult == shoppingcart.Count)
                    {
                        //QR kodu eklenmiş email gönderilecek
                        #region SendOrderEmailWithQR

                        string siteUrl =
                        Request.Url.Scheme + Uri.SchemeDelimiter
                        + Request.Url.Host
                        + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                        siteUrl += "/Home/Order/" + customerOrder.Id;

                        QRCodeGenerator QRGenerator = new QRCodeGenerator();
                        QRCodeData QRData = QRGenerator.CreateQrCode(siteUrl, QRCodeGenerator.ECCLevel.Q);
                        QRCode QRCode = new QRCode(QRData);
                        Bitmap QRBitmap = QRCode.GetGraphic(60);
                        byte[] bitmapArray = BitmapToByteArray(QRBitmap);

                        List<OrderDetail> orderDetailList =
                         new List<OrderDetail>();
                        orderDetailList = myOrderDetailRepo.AsQueryable()
                            .Where(x => x.OrderId == customerOrder.Id).ToList();

                        string message = $"Merhaba {user.Name} {user.Surname} <br/><br/>" +
                        $"{orderDetailList.Sum(x => x.Quantity)} adet ürünlerinizin siparişini aldık.<br/><br/>" +
                        $"Toplam Tutar:{orderDetailList.Sum(x => x.TotalPrice).ToString()} ₺ <br/> <br/>" +
                        $"<table><tr><th>Ürün Adı</th><th>Adet</th><th>Birim Fiyat</th><th>Toplam</th></tr>";
                        foreach (var item in orderDetailList)
                        {
                            message += $"<tr><td>{myProductRepo.GetById(item.ProductId).ProductName}</td><td>{item.Quantity}</td><td>{item.TotalPrice}</td></tr>";
                        }


                        message += "</table><br/>Siparişinize ait QR kodunuz aşağıdadır. <br/><br/>";

                        SiteSettings.SendMail(bitmapArray, new MailModel()
                        {
                            To = user.Email,
                            Subject = "ECommerceLite - Siparişiniz alındı.",
                            Message = message
                        });


                        #endregion

                        TempData["BuySuccess"] = "Siparişiniz oluşturuldu. Sipariş Numarası: " + customerOrder.OrderNumber;
                        //temizlik
                        Session["ShoppingCart"] = null;
                        Logger.LogMessage($"{user.Name} {user.Surname} {orderDetailList.Sum(x => x.TotalPrice)} liralık alışveriş yaptı", "Home/Buy");
                        return RedirectToAction("Index", "Home");

                    }
                    else
                    {
                        //Sistem yöneticisine orderId detayı verilerek
                        //email gönderilsin.Eklenmeyen ürünleri acilen eklesinler.
                        var message = $"Merhaba Admin <br/>" + $"Aşagıdaki bilgileri verilen siparişin kendisi oluşturulmasına rağmen  detaylarından bazıları oluşturulmadı. Acilen müdahale edelim <br/><br/>" + $"OrderId:{customerOrder.Id} <br/>" + $"Sipariş detayları <br/>";
                        for (int i = 0; i < shoppingcart.Count; i++)
                        {
                            message += $"{i}- Id: {shoppingcart[i].Id}-" +
                                $"Birim Fiyat: {shoppingcart[i].Price}-" +
                                $"Sipariş Adedi: {shoppingcart[i].Discount}-" +
                                $"İndirimi: {shoppingcart[i].Quantity}-" +
                                $"<br/><br/>";
                        }
                        await SiteSettings.SendMail(new MailModel()
                        {
                            To = "nayazilim303@gmail.com",
                            Subject = "ACİL - ECommerceLite 303 - Sipariş Detay Sorunu",
                            Message = message

                        });
                    }
                }

                return RedirectToAction("Index", "Home");

            }
            catch (Exception ex)
            {
                Logger.LogMessage($"Satın alma işleminde hata: " + ex.ToString(), "Home/Buy", MembershipTools.GetUser().Id);
                TempData["BuyFailed"] = "Beklenmedik bir hata nedeniyle siparişiniz oluşturulamadı";
                return RedirectToAction("Index", "Home");
            }
        }
    
        public ActionResult ProductDetail(int? id)
        {
            try
            {
                if (id==null)
                {
                    return RedirectToAction("Index", "Home");
                }
                if (id >0)
                {
                    // ürünü bulacagız
                    ProductViewModel model = myProductRepo.GetById(id.Value).Adapt<ProductViewModel>();
                    model.GetCategory();
                    model.GetProductPicture();
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError("", "Ürün bulunamadı!");
                    return View(new ProductViewModel());
                }
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", "Beklenmeyen bir hata oluştu!");
                return View(new ProductViewModel());    
                    
            }
        }
    
    
    }
}