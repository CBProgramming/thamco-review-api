using ReviewData;
using ReviewRepository.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ReviewRepository
{
    public class FakeReviewRepository : IReviewRepository
    {
        public bool Succeeds = true;
        public bool PurchaseDoesExist = true;
        public CustomerModel Customer;
        public PurchaseModel Purchases;
        public ReviewModel ReviewModel;

        public async Task<bool> AnonymiseCustomer(int customerId)
        {
            if (Succeeds && customerId == Customer.CustomerId)
            {
                Customer.CustomerName = "Anonymised";
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteReview(int customerId, int productId)
        {
            if (Succeeds && customerId == ReviewModel.CustomerId && productId == ReviewModel.ProductId)
            {
                ReviewModel = null;
                return true;
            }
            return false;
        }

        public async Task<bool> EditCustomer(CustomerModel customer)
        {
            return await NewOrEditCustomer(customer);
        }

        public async Task<bool> EditReview(ReviewModel review)
        {
            return await NewOrEditReview(review);
        }

        public async Task<ReviewModel> GetReview(int customerId, int productId, bool staff)
        {
            if(Succeeds 
                && ReviewModel != null 
                && customerId == ReviewModel.CustomerId 
                && productId == ReviewModel.ProductId)
            {
                return ReviewModel;
            }
            return null;
        }

        public async Task<IList<ReviewModel>> GetReviewsByCustomerId(int customerId, bool? visible)
        {
            if (Succeeds && ReviewModel != null && ReviewModel.CustomerId == customerId)
            {
                return new List<ReviewModel> { ReviewModel };
            }
            return new List<ReviewModel>();
        }

        public async Task<IList<ReviewModel>> GetReviewsByProductId(int productId, bool? visible)
        {
            if (Succeeds && ReviewModel != null && ReviewModel.ProductId == productId)
            {
                return new List<ReviewModel> { ReviewModel };
            }
            return new List<ReviewModel>();
        }

        public async Task<bool> HideReview(int customerId, int productId)
        {
            if (Succeeds 
                && ReviewModel != null 
                && ReviewModel.CustomerId == customerId 
                && ReviewModel.ProductId == productId)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> NewCustomer(CustomerModel customer)
        {
            return await NewOrEditCustomer(customer);
        }

        public async Task<bool> NewPurchases(PurchaseModel purchases)
        {
            if (Succeeds)
            {
                Purchases = purchases;
                return true;
            }
            return false;
        }

        public async Task<bool> NewReview(ReviewModel review)
        {
            return await NewOrEditReview(review);
        }

        public async Task<bool> PurchaseExists(int customerId, int productId)
        {
            return Succeeds && PurchaseDoesExist;
        }

        public Task<bool> ReviewExists(int customerId, int productId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ValidAuthId(int customerId, string authId)
        {
            if (Succeeds)
            {
                return customerId == Customer.CustomerId
                && authId == Customer.CustomerAuthId;
            }
            return false;
        }

        private async Task<bool> NewOrEditCustomer(CustomerModel customer)
        {
            if (Succeeds && customer != null)
            {
                if (Customer != null && Customer.CustomerId != customer.CustomerId)
                {
                    return false;
                }
                if (Customer == null)
                {
                    Customer = customer;
                }
                else
                {
                    Customer.CustomerName = customer.CustomerName;
                }
            }
            return Succeeds;
        }

        private async Task<bool> NewOrEditReview(ReviewModel review)
        {
            if (Succeeds)
            {
                ReviewModel = review;
            }
            return Succeeds;
        }
    }
}
