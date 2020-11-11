using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ReviewService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffReviewController : ControllerBase
    {
        [HttpGet]
        public Task<IActionResult> Get(int customerId, int? productId, bool? visible)
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        public Task<IActionResult> Delete(int reviewId)
        {
            throw new NotImplementedException();
        }
    }
}