using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductInformation
{
    public class AMProducts
    {
        public int SKU { get; set; }
        public string Name { get; set; }
        public float Price { get; set; }
        public float Quantity { get; set; }
        public bool InStock { get; set; }
    }
}
