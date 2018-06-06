﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Tangy.Models
{
    public class OrderDetails
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual OrderHeader OrderHeader { get; set; }

        [Required]
        public int MenuItemId { get; set; }

        [ForeignKey("MenuItemId")]
        public virtual MenuItem MenuItem{ get; set; }

        public int Count { get; set; }

        //this is for a copy of the actual order (could get the details from menuitem)
        //if the admin updates the price we dont want to get it from MenuItem as it will change the price of the order
        //store a local copy here

        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public double Price { get; set; }








    }
}
