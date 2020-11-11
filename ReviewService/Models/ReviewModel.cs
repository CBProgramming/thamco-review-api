using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewService.Models
{
    public class ReviewModel
    {
        public int customerId { get; set; }

        public int productId { get; set; }

        public int rating { get; set; }

        public string reviewText { get; set; }
    }
}
