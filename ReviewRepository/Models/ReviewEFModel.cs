﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ReviewRepository.Models
{
    public class ReviewEFModel
    {
        public int CustomerId { get; set; }

        public int ProductId { get; set; }

        public int Rating { get; set; }

        public string ReviewText { get; set; }

        public bool Visible { get; set; }
    }
}
