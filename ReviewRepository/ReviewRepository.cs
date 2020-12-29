using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ReviewData;
using ReviewRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewRepository
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ReviewDb _context;
        private readonly IMapper _mapper;

        public ReviewRepository(ReviewDb context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> DeleteReview(int customerId, int productId)
        {
            try
            {
                var review = _context.Reviews.SingleOrDefault(r => r.ProductId == productId && r.CustomerId == customerId);
                if (review != null)
                {
                    _context.Reviews.Remove(review);
                    await _context.SaveChangesAsync();
                    return true;
                }
            }
            catch (DbUpdateConcurrencyException)
            {

            }
            return false;
        }

        public async Task<bool> EditReview(ReviewModel reviewModel)
        {
            if (reviewModel != null)
            {
                var review = _context.Reviews.FirstOrDefault(p => p.ProductId == reviewModel.ProductId && p.CustomerId == reviewModel.CustomerId);
                if (review != null)
                {
                    try
                    {
                        review.Rating = reviewModel.Rating;
                        review.ReviewText = review.ReviewText;
                        review.TimeStamp = review.TimeStamp;
                        await _context.SaveChangesAsync();
                        return true;
                    }
                    catch (DbUpdateConcurrencyException)
                    {

                    }
                }
            }
            return false;
        }

        public async Task<bool> NewPurchases(PurchaseModel purchases)
        {
            try
            {
                foreach (ProductModel product in purchases.OrderedItems)
                {
                    if (!await PurchaseExists(purchases.CustomerId, product.ProductId))
                    {
                        var purchase = new Purchase
                        {
                            CustomerId = purchases.CustomerId,
                            ProductId = product.ProductId
                        };
                        _context.Add(purchase);
                    }
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {

            }
            return false;
        }

        private bool CustomerExists(int customerId)
        {
            return _context.Customers.Any(c => c.CustomerId == customerId);
        }

        public async Task<bool> PurchaseExists(int customerId, int productId)
        {
            return _context.Purchases.Any(p => p.ProductId == productId && p.CustomerId == customerId);
        }

        public async Task<bool> NewReview(ReviewModel reviewModel)
        {
            if (reviewModel != null)
            {
                try
                {
                    var review = _mapper.Map<Review>(reviewModel);
                    review.Visible = true;
                    _context.Add(review);
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateConcurrencyException)
                {

                }
            }
            return false;
        }

        public async Task<IList<ReviewModel>> GetReviewsByCustomerId(int customerId, bool? visible = true)
        {
            return _mapper.Map<List<ReviewModel>>(_context.Reviews.Where(r => r.CustomerId == customerId && r.Visible == visible));
        }

        public async Task<IList<ReviewModel>> GetReviewsByProductId(int productId, bool? visible = true)
        {
            return _mapper.Map<List<ReviewModel>>(_context.Reviews.Where(r => r.ProductId == productId && r.Visible == visible));
        }

        public ReviewModel GetReview(int customerId, int productId, bool staff = false)
        {
            var review = _context.Reviews.Where(r => r.CustomerId == customerId && r.ProductId == productId).FirstOrDefault();
            if (review == null || (!staff && review.Visible == false))
            {
                return null;
            }
            return _mapper.Map<ReviewModel>(review);
        }

        public async Task<bool> ReviewExists(int customerId, int productId)
        {
            return _context.Reviews.Any(r => r.ProductId == productId && r.CustomerId == customerId);
        }

        public async Task<bool> HideReview(int customerId, int productId)
        {
            var review = _context.Reviews.FirstOrDefault(r => r.ProductId == productId && r.CustomerId == customerId);
            if (review != null)
            {
                try
                {
                    review.Visible = false;
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateConcurrencyException)
                {

                }
            }
            return false;
        }

        public async Task<bool> NewCustomer(CustomerModel newCustomer)
        {
            if (newCustomer == null)
            {
                return false;
            }
            try
            {
                var customer = _mapper.Map<Customer>(newCustomer);
                _context.Add(customer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        public async Task<bool> EditCustomer(CustomerModel editedCustomer)
        {
            if (editedCustomer == null)
            {
                return false;
            }
            var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == editedCustomer.CustomerId);
            if (customer == null)
            {
                return false;
            }
            try
            {
                if (customer.CustomerName != editedCustomer.CustomerName && !string.IsNullOrEmpty(editedCustomer.CustomerName))
                {
                    customer.CustomerName = editedCustomer.CustomerName;
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        public async Task<bool> AnonymiseCustomer(int customerId)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == customerId);
            if (customer == null)
            {
                return false;
            }
            try
            {
                customer.CustomerName = "Anonymised";
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }
    }
}
