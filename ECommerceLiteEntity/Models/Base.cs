using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceLiteEntity.Models
{
    public abstract class Base<T> : IBase
    {
        [Key]
        [Column(Order=1)]
        public T Id { get; set; }

        [DataType(DataType.DateTime)] //Type DateTime olsun
        [Display(Name = "Kayıt Tarihi")]
        [Required] // Girmek zorunlu olsun
        [Column(Order = 2)]//Kaçıncı Sırada olsun diye yaptık
        public DateTime RegisterDate { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false;
    }
}
