using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReviewRepository;
using ReviewRepository.Models;
using ReviewService.Models;

namespace ReviewService.Controllers
{
    [ApiController]
    [Route("api/Customer")]
    [Authorize(Policy = "CustomerOnly")]
    public class CustomerReviewController : ControllerBase
    {
        private readonly ILogger<CustomerReviewController> _logger;
        private readonly IReviewRepository _reviewRepo;
        private readonly IMapper _mapper;
        private string authId, clientId;

        public CustomerReviewController(ILogger<CustomerReviewController> logger, IReviewRepository reviewRepo, IMapper mapper)
        {
            _logger = logger;
            _reviewRepo = reviewRepo;
            _mapper = mapper;
        }

        private void GetTokenDetails()
        {
            authId = User
                .Claims
                .FirstOrDefault(c => c.Type == "sub")?.Value;
            clientId = User
                .Claims
                .FirstOrDefault(c => c.Type == "client_id")?.Value;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int? customerId, int? productId)
        {
            if (productId != null && productId > 0 && (customerId == null || customerId < 1))
            {
                return Ok(_mapper.Map<List<ReviewDto>>(await _reviewRepo.GetReviewsByProductId(productId ?? 0, visible: true)));
            }
            GetTokenDetails();
            if (! await _reviewRepo.ValidAuthId(customerId??0, authId))
            {
                return Forbid();
            }
            if (customerId != null && customerId > 0 && (productId == null || productId < 1))
            {
                return Ok(_mapper.Map<List<ReviewDto>>(await _reviewRepo.GetReviewsByCustomerId(customerId??0, visible: true)));
            }
            var review = _mapper.Map<ReviewDto>(await _reviewRepo.GetReview(customerId??0, productId??0, staff: false));
            if (review != null)
            {
                return Ok(review);
            }
            return NotFound();
        }

        [HttpPost]
        public Task<IActionResult> Create([FromBody] ReviewDto review)
        {
            return CreateOrEdit(review);
        }

        [HttpPut]
        public Task<IActionResult> Edit([FromBody] ReviewDto review)
        {
            return CreateOrEdit(review);
        }

        private async Task<IActionResult> CreateOrEdit([FromBody] ReviewDto reviewDto)
        {
            GetTokenDetails();
            if (!await _reviewRepo.ValidAuthId(reviewDto.CustomerId, authId))
            {
                return Forbid();
            }
            if (!ValidReview(reviewDto))
            {
                return UnprocessableEntity();
            }
            if (await _reviewRepo.PurchaseExists(reviewDto.CustomerId, reviewDto.ProductId))
            {
                var review = await _reviewRepo.GetReview(reviewDto.CustomerId, reviewDto.ProductId, staff: false);
                if (review != null && !review.Visible)
                {
                    return NotFound();
                }
                reviewDto.TimeStamp = ValidateDate(reviewDto.TimeStamp);
                if (review == null)
                {
                    if (await _reviewRepo.NewReview(_mapper.Map<ReviewModel>(reviewDto)))
                    {
                        return Ok();
                    }
                }
                if (await _reviewRepo.EditReview(_mapper.Map<ReviewModel>(reviewDto)))
                {
                    return Ok();
                }
            }
            return NotFound();
        }

        private bool ValidReview(ReviewDto reviewDto)
        {
            return !string.IsNullOrEmpty(reviewDto.ReviewText)
                && reviewDto.Rating >= 0
                && reviewDto.Rating <= 5;
        }

        private DateTime ValidateDate(DateTime orderDate)
        {
            //if date is over 7 days old, or a future date, set date to now (7 days chosen arbitrarily as it would 
            //likely be a business decision above my position)
            var now = DateTime.Now.Ticks;
            var orderTicks = orderDate.Ticks;
            var difference = now - orderTicks;
            var limit = TimeSpan.TicksPerDay * 7;
            if (DateTime.Now.Ticks - orderDate.Ticks > (TimeSpan.TicksPerDay * 7) || orderDate > DateTime.Now)
            {
                return DateTime.Now;
            }
            return orderDate;
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int customerId, int productId)
        {
            GetTokenDetails();
            if (!await _reviewRepo.ValidAuthId(customerId, authId))
            {
                return Forbid();
            }
            if (await _reviewRepo.DeleteReview(customerId, productId))
            {
                return Ok();
            }
            return NotFound();
        }
    }
}
