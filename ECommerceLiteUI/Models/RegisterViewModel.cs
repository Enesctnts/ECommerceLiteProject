using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ECommerceLiteUI.Models
{
    public class RegisterViewModel
    {
        //Kayıt modeli içinde siteye kayıt olmak isteyen kişilerden hangi bilgileri alcagımız belirleyeceğiz.
        //Tckilik , isim soyisim email( eger yazdıysak telefon cinsiyet vb) alanları tanımlayalım.

        //Data annotationları kullnadığımız için validation kuralllarını belirlediğmiz için kapsüllemeye gerek kalmaz.

        [Required]
        [StringLength(11,MinimumLength =11, ErrorMessage ="Tc kimlik numarası 11 haneli olmalıdır!")]
        [Display(Name="Tc Kimlik")]
        public string TCNumber { get; set; }
        [Required]
        [Display(Name = "Ad")]
        [StringLength(maximumLength: 30, MinimumLength = 2, ErrorMessage = "İsminizin uzunlugu 2 ile 30 karakter arasında olmalıdır!")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Soyad")]
        [StringLength(maximumLength: 30, MinimumLength = 2, ErrorMessage = "Soyisminizin uzunlugu 2 ile 30 karakter arasında olmalıdır!")]
        public string Surname { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name="Şifre")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^[a-zA-Z]\w{4,14}$", ErrorMessage = @"	
        The password's first character must be a letter, it must contain at least 5 characters and no more than 15 characters and no characters other than letters, numbers and the underscore may be used")]

        public string Password { get; set; }

        [Required]
        [Display(Name = "Şifre Tekrar")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage ="Şifreler Uyuşmuyor!")]
        public string ConfirmPassword { get; set; }
    }
}

