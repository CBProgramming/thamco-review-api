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
    [Authorize(Policy = "CustomerAccountAPIOnly")]
    public class CustomerAccountController : Controller
    {
        private readonly ILogger<CustomerAccountController> _logger;
        private readonly IReviewRepository _reviewRepo;
        private readonly IMapper _mapper;

        public CustomerAccountController(ILogger<CustomerAccountController> logger, IReviewRepository reviewRepository, IMapper mapper)
        {
            _logger = logger;
            _reviewRepo = reviewRepository;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerDto customer)
        {
            if (customer != null && !string.IsNullOrEmpty(customer.CustomerName))
            {
                if (await _reviewRepo.NewCustomer(_mapper.Map<CustomerModel>(customer)))
                {
                    return Ok();
                }
                return NotFound();
            }
            return UnprocessableEntity();
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] CustomerDto customer)
        {
            if (customer != null && !string.IsNullOrEmpty(customer.CustomerName))
            {
                if (await _reviewRepo.EditCustomer(_mapper.Map<CustomerModel>(customer)))
                {
                    return Ok();
                }
                return NotFound();
            }
            return UnprocessableEntity();
        }

        [HttpDelete("{customerId}")]
        public async Task<IActionResult> Delete([FromRoute] int customerId)
        {
            if (await _reviewRepo.AnonymiseCustomer(customerId))
            {
                return Ok();
            }
            return NotFound();
        }
    }
}
