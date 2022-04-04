using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceLiteEntity.IdentityModels
{
    public class ApplicationUser : IdentityUser
    {
        //IdentityUser'dan kalıtım alındı. Identity User microsoftun identity semasına ait bir classtır.
        //Identity User clası ile bize sunlan AspNetUsers tablosundaki kolanları genişletmek için kalıtım aldık.
        // Aşagırdaki ihtiyacımız olan kolanları ekledik.
        
        [Required]
        [Display(Name="Ad")]
        [StringLength(maximumLength:30,MinimumLength =2,ErrorMessage ="İsminizin uzunlugu 2 ile 30 karakter arasında olmalıdır!")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Soyad")]
        [StringLength(maximumLength: 30, MinimumLength = 2, ErrorMessage = "Soyisminizin uzunlugu 2 ile 30 karakter arasında olmalıdır!")]
        public string Surname { get; set; }

        [Required]
        [Display(Name = "Kayıt Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime RegisterDate { get; set; } = DateTime.Now;

        //ToDo: Guid 'in kaç haneli oldugunu bakıp buraya string ile length ile attribute tanımlanacaktır.
        public string ActivationCode { get; set; }

    }
}
