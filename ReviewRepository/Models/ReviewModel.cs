using System;
using System.Collections.Generic;
using System.Text;

namespace ReviewRepository.Models
{
    public class ReviewModel
    {
        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public int ProductId { get; set; }

        public int Rating { get; set; }

        public string ReviewText { get; set; }

        public bool Visible { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}
