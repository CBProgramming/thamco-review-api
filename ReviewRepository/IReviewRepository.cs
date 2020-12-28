using ReviewRepository.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReviewRepository
{
    public interface IReviewRepository
    {
        ReviewModel GetReview(int customerId, int productId, bool staff);

        Task<IList<ReviewModel>> GetReviewsByCustomerId(int customerId, bool? visible);

        Task<IList<ReviewModel>> GetReviewsByProductId(int productId, bool? visible);

        Task<bool> NewReview(ReviewModel review);

        Task<bool> EditReview(ReviewModel review);

        Task<bool> DeleteReview(int customerId, int productId);

        Task<bool> NewPurchases(PurchaseModel purchases);

        Task<bool> ReviewExists(int customerId, int productId);

        Task<bool> PurchaseExists(int customerId, int productId);

        Task<bool> HideReview(int customerId, int productId);
    }
}
