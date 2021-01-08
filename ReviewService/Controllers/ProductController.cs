using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReviewRepository;
using ReviewRepository.Models;
using ReviewService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "OrderingAPIOnly")]
    public class ProductController : Controller
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IReviewRepository _reviewRepo;
        private readonly IMapper _mapper;

        public ProductController(ILogger<ProductController> logger, IReviewRepository reviewRepository, IMapper mapper)
        {
            _logger = logger;
            _reviewRepo = reviewRepository;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PurchaseDto purchases)
        {
            if(!ValidPurchases(purchases))
            {
                return UnprocessableEntity();
            }
            if (await _reviewRepo.NewPurchases(_mapper.Map<PurchaseModel>(purchases)))
            {
                return Ok();
            }
            return NotFound();
        }

        private bool ValidPurchases(PurchaseDto purchases)
        {
            return purchases != null
                && purchases.CustomerId > 0
                && !string.IsNullOrEmpty(purchases.CustomerAuthId)
                && purchases.OrderedItems != null
                && purchases.OrderedItems.Count > 0;
        }
    }
}
