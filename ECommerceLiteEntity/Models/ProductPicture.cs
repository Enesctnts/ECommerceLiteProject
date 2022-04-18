using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceLiteEntity.Models
{
    [Table("ProductPictures")]
    public class ProductPicture:Base<int>
    {
        public int ProductId { get; set; } // ProductPicture tablosu Product tablosuyla ilişkilidir. ProductId yi o yüzden yazıyoz sonra  [ForeignKey("ProductId")] ve  public virtual Product Product { get; set; }

        [StringLength(400,ErrorMessage ="Ürün resim yolu en fazla 400 karakter")]
        public string Picture { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }


    }
}
