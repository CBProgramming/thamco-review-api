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
        public CustomerModel Customer;
        public PurchaseModel Purchases;

        public async Task<bool> AnonymiseCustomer(int customerId)
        {
            if (Succeeds && customerId == Customer.CustomerId)
            {
                Customer.CustomerName = "Anonymised";
                return true;
            }
            return false;
        }

        public Task<bool> DeleteReview(int customerId, int productId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> EditCustomer(CustomerModel customer)
        {
            return await NewOrEditCustomer(customer);
        }

        public Task<bool> EditReview(ReviewModel review)
        {
            throw new NotImplementedException();
        }

        public ReviewModel GetReview(int customerId, int productId, bool staff)
        {
            throw new NotImplementedException();
        }

        public Task<IList<ReviewModel>> GetReviewsByCustomerId(int customerId, bool? visible)
        {
            throw new NotImplementedException();
        }

        public Task<IList<ReviewModel>> GetReviewsByProductId(int productId, bool? visible)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HideReview(int customerId, int productId)
        {
            throw new NotImplementedException();
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

        public Task<bool> NewReview(ReviewModel review)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PurchaseExists(int customerId, int productId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ReviewExists(int customerId, int productId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ValidAuthId(int customerId, string authId)
        {
            throw new NotImplementedException();
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
    }
}
