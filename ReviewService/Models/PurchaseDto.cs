using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewService.Models
{
    public class PurchaseDto
    {
        public int CustomerId { get; set; }

        public string CustomerAuthId { get; set; }

        public List<ProductDto> OrderedItems { get; set; }
    }
}
