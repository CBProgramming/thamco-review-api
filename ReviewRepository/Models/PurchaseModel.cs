using System;
using System.Collections.Generic;
using System.Text;

namespace ReviewRepository.Models
{
    public class PurchaseModel
    {
        public int CustomerId { get; set; }

        public string CustomerAuthId { get; set; }

        public IList<ProductModel> OrderedItems { get; set; }
    }
}
