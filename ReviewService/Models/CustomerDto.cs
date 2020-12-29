using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewService.Models
{
    public class CustomerDto
    {
        public int CustomerId { get; set; }

        public string CustomerAuthId { get; set; }

        public string CustomerName { get; set; }
    }
}
