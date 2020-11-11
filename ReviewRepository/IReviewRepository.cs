using ReviewRepository.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReviewRepository
{
    public interface IReviewRepository
    {
        Task<ReviewEFModel> GetReview(string reviewId);

        Task<IList<ReviewEFModel>> GetCustomerReviews(int customerId, bool? visible);

        Task<bool> NewReview(ReviewEFModel review);

        Task<bool> EditReview(ReviewEFModel review);

        Task<bool> DeleteReview(string reviewId);
    }
}
