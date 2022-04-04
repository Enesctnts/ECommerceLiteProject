﻿using ECommerceLiteEntity.IdentityModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceLiteEntity.Models
{
    [Table("PassiveUsers")]
    public class PassiveUser
    {
        //Identity model ile bize verilen tablodaki Id buraya foreignKey olacaktır.
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser Application { get; set; }
    }
}
