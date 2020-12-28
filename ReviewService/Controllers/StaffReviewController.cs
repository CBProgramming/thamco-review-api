using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReviewRepository;
using ReviewService.Models;

namespace ReviewService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "StaffOnly")]
    public class StaffReviewController : ControllerBase
    {
        private readonly ILogger<CustomerReviewController> _logger;
        private readonly IReviewRepository _reviewRepo;
        private readonly IMapper _mapper;

        public StaffReviewController(ILogger<CustomerReviewController> logger, IReviewRepository reviewRepo, IMapper mapper)
        {
            _logger = logger;
            _reviewRepo = reviewRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int? customerId, int? productId, bool? visible = true)
        {
            if (customerId != null && customerId > 0 && (productId == null || productId < 1))
            {
                return Ok(_mapper.Map<List<ReviewDto>>(await _reviewRepo.GetReviewsByCustomerId(customerId??0, visible: visible)));
            }
            if (productId != null && productId > 0 && (customerId == null || customerId < 1))
            {
                return Ok(_mapper.Map<List<ReviewDto>>(await _reviewRepo.GetReviewsByProductId(productId ?? 0, visible: visible)));
            }
            var review = _mapper.Map<ReviewDto>(_reviewRepo.GetReview(customerId??0, productId ?? 0, staff: true));
            if (review != null)
            {
                return Ok(review);
            }
            return NotFound();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int customerId, int productId)
        {
            if (await _reviewRepo.HideReview(customerId, productId))
            {
                return Ok();
            }
            return NotFound();
        }
    }
}