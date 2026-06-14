using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OTS_ORDER.Dtos
{
 public class ReqPlaceOrderObj
    {
        public string CategoryCode { get; set; } 
        public string Email { get; set; } 
        public int Quantity { get; set; }
    }
}