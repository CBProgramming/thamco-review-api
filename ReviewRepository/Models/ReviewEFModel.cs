using System;
using System.Collections.Generic;
using System.Text;

namespace ReviewRepository.Models
{
    public class ReviewEFModel
    {
        public string reviewId { get; set; } //concatonate "customerId + : + productId"

        public int customerId { get; set; }

        public int productId { get; set; }

        public int rating { get; set; }

        public string reviewText { get; set; }

        public bool Visible { get; set; }
    }
}
