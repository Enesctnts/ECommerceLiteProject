using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceLiteEntity.Models
{
    [Table("Categories")]
    public class Category:Base<int>
    {
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Kategori adının uzuluğu 2 ile 50 karakter arasında olmalıdır!")]
        public string CategoryName { get; set; }
        [StringLength(500, ErrorMessage = "Kategori açıklmasının uzunluğu en fazla 500 karakter olmalıdır!")]
        public string CategoryDescription { get; set; }

        public int? BaseCategoryId { get; set; }
        [ForeignKey("BaseCategoryId")]
        public virtual Category BaseCategory { get; set; }
        public virtual List<Product> ProductList { get; set; }
        public virtual List<Category> CategoryList { get; set; }

        //[Required]
        //[StringLength(100,MinimumLength =2,ErrorMessage ="Kategori adı 2 ile 100 karakter aralıgında olmalıdır")]
        //[Display(Name ="Kategori Adı")]
        //public string CategoryName { get; set; }

        //[Required]
        //[StringLength(500, MinimumLength = 2, ErrorMessage = "Kategori açıklaması 2 ile 500 karakter aralıgında olmalıdır")]
        //[Display(Name = "Kategori Açıklaması")]
        //public string CategoryDescription { get; set; } 

        //public int? BaseCategoryId { get; set; } // int normalde asla null değer olamaz! int in yanna ? yazarsak Nullable oluruz.
        ////public Nullable<int> BaseCategoryId { get; set; } //yukardakinden önce bu kullanılırdı.

        //[ForeignKey("BaseCategoryId")]
        //public virtual Category BaseCategory { get; set; }

        //public virtual List<Category> CategoryList { get; set; }

        ////Her ürünün kategorisi olur cümlesinden yola çıkarak 
        ////Productta tanımlanan ilişkiyi burada karşılaştıralım.
        ////1'e sonsuz ilişki nedeniyle yani bir kategorini birden çok ürünü olabilir
        ////mantıgını karşılamak amacıyla burada virtual property list tipindedir.
        //public virtual List<Product> ProductList { get; set; }
    }
}
