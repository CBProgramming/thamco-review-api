using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReviewService.Models;

namespace ReviewService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerReviewController : ControllerBase
    {
        private readonly ILogger<CustomerReviewController> _logger;

        public CustomerReviewController(ILogger<CustomerReviewController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public Task<IActionResult> Get(int customerId, int? productId)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public Task<IActionResult> Create([FromBody] ReviewModel model)
        {
            throw new NotImplementedException();
        }

        [HttpPut]
        public Task<IActionResult> Edit([FromBody] ReviewModel customerDto)
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        public Task<IActionResult> Delete(string reviewId)
        {
            throw new NotImplementedException();
        }
    }
}
