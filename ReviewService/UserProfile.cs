using AutoMapper;
using ReviewRepository.Models;
using ReviewService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewService
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<ProductDto, ProductModel>();
            CreateMap<ProductModel, ProductDto>();
            CreateMap<PurchaseDto, PurchaseModel>();
            CreateMap<PurchaseModel, PurchaseDto>();
            CreateMap<ReviewDto, ReviewModel>();
            CreateMap<ReviewModel, ReviewDto>();
        }
    }
}
