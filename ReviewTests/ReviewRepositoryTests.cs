using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using ReviewData;
using ReviewRepository.Models;
using ReviewService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ReviewTests
{
    public class ReviewRepositoryTests
    {
        public CustomerModel customerRepoModel;
        public ReviewModel reviewRepoModel;
        public PurchaseModel purchaseRepoModel;
        public IMapper mapper;
        public IQueryable<Customer> dbCustomers;
        public IQueryable<Purchase> dbPurchases;
        public IQueryable<Review> dbReviews;
        public Customer dbCustomer1, dbCustomer2;
        public Purchase dbPurchase1, dbPurchase2, dbPurchase3;
        public Review dbReview1, dbReview2, dbReview3;
        public Mock<DbSet<Customer>> mockCustomers;
        public Mock<DbSet<Purchase>> mockPurchases;
        public Mock<DbSet<Review>> mockReviews;
        public Mock<ReviewDb> mockDbContext;
        public ReviewRepository.ReviewRepository repo;

        private void SetupCustomerRepoModel()
        {
            customerRepoModel = new CustomerModel
            {
                CustomerId = 3,
                CustomerAuthId = "AuthId3",
                CustomerName = "Name 3"
            };
        }

        private void SetupDbCustomer()
        {
            dbCustomer1 = new Customer
            {
                CustomerId = 1,
                CustomerAuthId = "AuthId1",
                CustomerName = "Name 1"
            };
            dbCustomer2 = new Customer
            {
                CustomerId = 2,
                CustomerAuthId = "AuthId2",
                CustomerName = "Name 2"
            };
        }

        private void SetupDbCustomers()
        {
            SetupDbCustomer();
            dbCustomers = new List<Customer>
            {
                dbCustomer1, dbCustomer2
            }.AsQueryable();
        }

        private void SetupMockCustomers()
        {
            mockCustomers = new Mock<DbSet<Customer>>();
            mockCustomers.As<IQueryable<Customer>>().Setup(m => m.Provider).Returns(dbCustomers.Provider);
            mockCustomers.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(dbCustomers.Expression);
            mockCustomers.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(dbCustomers.ElementType);
            mockCustomers.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(dbCustomers.GetEnumerator());
        }

        private void SetupPurchaseRepoModel()
        {
            purchaseRepoModel = new PurchaseModel
            {
                CustomerId = 1,
                CustomerAuthId = "AuthId1",
                OrderedItems = new List<ProductModel>
                {
                    new ProductModel { ProductId = 1},
                    new ProductModel { ProductId = 4},
                }
            };
        }

        private void SetupDbPurchase()
        {
            dbPurchase1 = new Purchase
            {
                CustomerId = 1,
                ProductId = 1
            };
            dbPurchase2 = new Purchase
            {
                CustomerId = 1,
                ProductId = 2
            };
            dbPurchase3 = new Purchase
            {
                CustomerId = 2,
                ProductId = 1
            };
        }

        private void SetupDbPurchases()
        {
            SetupDbPurchase();
            dbPurchases = new List<Purchase>
            {
                dbPurchase1, dbPurchase2, dbPurchase3
            }.AsQueryable();
        }

        private void SetupMockPurchases()
        {
            mockPurchases = new Mock<DbSet<Purchase>>();
            mockPurchases.As<IQueryable<Purchase>>().Setup(m => m.Provider).Returns(dbPurchases.Provider);
            mockPurchases.As<IQueryable<Purchase>>().Setup(m => m.Expression).Returns(dbPurchases.Expression);
            mockPurchases.As<IQueryable<Purchase>>().Setup(m => m.ElementType).Returns(dbPurchases.ElementType);
            mockPurchases.As<IQueryable<Purchase>>().Setup(m => m.GetEnumerator()).Returns(dbPurchases.GetEnumerator());
        }

        private void SetupReviewRepoModel()
        {
            reviewRepoModel = new ReviewModel
            {
                CustomerId = 1,
                CustomerName = "New Review Customer",
                ProductId = 1,
                Rating = 1,
                ReviewText = "New Review Text",

            };
        }

        private void SetupDbReview()
        {
            dbReview1 = new Review
            {
                CustomerId = 1,
                ProductId = 1,
                Rating = 1,
                ReviewText = "Text 1",
                TimeStamp = new DateTime(),
                Visible = true
            };
            dbReview2 = new Review
            {
                CustomerId = 1,
                ProductId = 2,
                Rating = 2,
                ReviewText = "Text 2",
                TimeStamp = new DateTime(),
                Visible = true
            };
            dbReview3 = new Review
            {
                CustomerId = 2,
                ProductId = 2,
                Rating = 3,
                ReviewText = "Text 3",
                TimeStamp = new DateTime(),
                Visible = false
            };
        }

        private void SetupDbReviews()
        {
            SetupDbReview();
            dbReviews = new List<Review>
            {
                dbReview1, dbReview2, dbReview3
            }.AsQueryable();
        }

        private void SetupMockReviews()
        {
            mockReviews = new Mock<DbSet<Review>>();
            mockReviews.As<IQueryable<Review>>().Setup(m => m.Provider).Returns(dbReviews.Provider);
            mockReviews.As<IQueryable<Review>>().Setup(m => m.Expression).Returns(dbReviews.Expression);
            mockReviews.As<IQueryable<Review>>().Setup(m => m.ElementType).Returns(dbReviews.ElementType);
            mockReviews.As<IQueryable<Review>>().Setup(m => m.GetEnumerator()).Returns(dbReviews.GetEnumerator());
        }

        private void SetupMockDbContext()
        {
            mockDbContext = new Mock<ReviewDb>();
            mockDbContext.Setup(m => m.Customers).Returns(mockCustomers.Object);
            mockDbContext.Setup(m => m.Reviews).Returns(mockReviews.Object);
            mockDbContext.Setup(m => m.Purchases).Returns(mockPurchases.Object);
        }

        private void SetupMapper()
        {
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            }).CreateMapper();
        }

        private void DefaultSetup()
        {
            SetupMapper();
            SetupCustomerRepoModel();
            SetupDbCustomers();
            SetupMockCustomers();
            SetupPurchaseRepoModel();
            SetupDbPurchases();
            SetupMockPurchases();
            SetupReviewRepoModel();
            SetupDbReviews();
            SetupMockReviews();
            SetupMockDbContext();
            repo = new ReviewRepository.ReviewRepository(mockDbContext.Object, mapper);
        }

        [Fact]
        public async Task NewCustomer_ShouldTrue()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.NewCustomer(customerRepoModel);

            //Assert
            Assert.True(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Once());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task NewNullCustomer_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.NewCustomer(null);

            //Assert
            Assert.False(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task EditCustomer_ShouldTrue()
        {
            //Arrange
            DefaultSetup();
            customerRepoModel.CustomerId = dbCustomer1.CustomerId;
            customerRepoModel.CustomerAuthId = dbCustomer1.CustomerAuthId;

            //Act
            var result = await repo.EditCustomer(customerRepoModel);

            //Assert
            Assert.True(result);
            Assert.Equal(dbCustomer1.CustomerId, customerRepoModel.CustomerId);
            Assert.Equal(dbCustomer1.CustomerAuthId, customerRepoModel.CustomerAuthId);
            Assert.Equal(dbCustomer1.CustomerName, customerRepoModel.CustomerName);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task EditCustomer_AuthDoesntMatch_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            customerRepoModel.CustomerId = dbCustomer1.CustomerId;

            //Act
            var result = await repo.EditCustomer(customerRepoModel);

            //Assert
            Assert.False(result);
            Assert.Equal(dbCustomer1.CustomerId, customerRepoModel.CustomerId);
            Assert.NotEqual(dbCustomer1.CustomerAuthId, customerRepoModel.CustomerAuthId);
            Assert.NotEqual(dbCustomer1.CustomerName, customerRepoModel.CustomerName);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task EditCustomer_DoesntExist_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.EditCustomer(customerRepoModel);

            //Assert
            Assert.False(result);
            Assert.NotEqual(dbCustomer1.CustomerId, customerRepoModel.CustomerId);
            Assert.NotEqual(dbCustomer1.CustomerAuthId, customerRepoModel.CustomerAuthId);
            Assert.NotEqual(dbCustomer1.CustomerName, customerRepoModel.CustomerName);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task EditCustomer_Null_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.EditCustomer(null);

            //Assert
            Assert.False(result);
            Assert.NotEqual(dbCustomer1.CustomerId, customerRepoModel.CustomerId);
            Assert.NotEqual(dbCustomer1.CustomerAuthId, customerRepoModel.CustomerAuthId);
            Assert.NotEqual(dbCustomer1.CustomerName, customerRepoModel.CustomerName);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task AnonymiseCustomer_ShouldTrue()
        {
            //Arrange
            DefaultSetup();
            string anonString = repo.AnonString;
            int customerId = 1;
            string originalAuthId = dbCustomer1.CustomerAuthId;

            //Act
            var result = await repo.AnonymiseCustomer(customerId);

            //Assert
            Assert.True(result);
            Assert.Equal(customerId, dbCustomer1.CustomerId);
            Assert.Equal(originalAuthId, dbCustomer1.CustomerAuthId);
            Assert.Equal(anonString, dbCustomer1.CustomerName);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task AnonymiseCustomer_DoesntExist_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            string anonString = repo.AnonString;
            int customerId = 3;

            //Act
            var result = await repo.AnonymiseCustomer(customerId);

            //Assert
            Assert.False(result);
            Assert.NotEqual(anonString, dbCustomer1.CustomerAuthId);
            Assert.NotEqual(anonString, dbCustomer1.CustomerName);
            Assert.NotEqual(anonString, dbCustomer2.CustomerAuthId);
            Assert.NotEqual(anonString, dbCustomer2.CustomerName);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task AnonymiseCustomer_Zero_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            string anonString = repo.AnonString;

            //Act
            var result = await repo.AnonymiseCustomer(0);

            //Assert
            Assert.False(result);
            Assert.NotEqual(anonString, dbCustomer1.CustomerAuthId);
            Assert.NotEqual(anonString, dbCustomer1.CustomerName);
            Assert.NotEqual(anonString, dbCustomer2.CustomerAuthId);
            Assert.NotEqual(anonString, dbCustomer2.CustomerName);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task DeleteReview_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            int customerId = 1;
            int productId = 1;

            //Act
            var result = await repo.DeleteReview(customerId, productId);

            //Assert
            Assert.True(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(dbReview1), Times.Once());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task DeleteReview_ReviewDoesntExistByCustomerId_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            int customerId = 99;
            int productId = 1;

            //Act
            var result = await repo.DeleteReview(customerId, productId);

            //Assert
            Assert.False(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task DeleteReview_ReviewDoesntExistByProductId_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            int customerId = 1;
            int productId = 99;

            //Act
            var result = await repo.DeleteReview(customerId, productId);

            //Assert
            Assert.False(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task DeleteReview_ReviewDoesntExistByCustomerIdOrProductId_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            int customerId = 99;
            int productId = 99;

            //Act
            var result = await repo.DeleteReview(customerId, productId);

            //Assert
            Assert.False(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task EditReview_ShouldTrue()
        {
            //Arrange
            DefaultSetup();
            int productId = dbReview1.ProductId;
            int customerId = dbReview1.CustomerId;
            bool visible = dbReview1.Visible;

            //Act
            var result = await repo.EditReview(reviewRepoModel);

            //Assert
            Assert.True(result);
            Assert.Equal(customerId, dbReview1.CustomerId);
            Assert.Equal(productId, dbReview1.ProductId);
            Assert.Equal(reviewRepoModel.Rating, dbReview1.Rating);
            Assert.Equal(reviewRepoModel.ReviewText, dbReview1.ReviewText);
            Assert.Equal(reviewRepoModel.TimeStamp, dbReview1.TimeStamp);
            Assert.Equal(visible, dbReview1.Visible);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task EditReview_NullReview_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.EditReview(null);

            //Assert
            Assert.False(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task EditReview_ReviewDoesntExistsByCustomerId_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            reviewRepoModel.CustomerId = 99;

            //Act
            var result = await repo.EditReview(reviewRepoModel);

            //Assert
            Assert.False(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task EditReview_ReviewDoesntExistsByProductId_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            reviewRepoModel.ProductId = 99;

            //Act
            var result = await repo.EditReview(reviewRepoModel);

            //Assert
            Assert.False(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task EditReview_ReviewDoesntExistsByCustomerIdOrProductId_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            reviewRepoModel.CustomerId = 99;
            reviewRepoModel.ProductId = 99;

            //Act
            var result = await repo.EditReview(reviewRepoModel);

            //Assert
            Assert.False(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task NewReview_ShouldTrue()
        {
            //Arrange
            DefaultSetup();
            reviewRepoModel.ProductId = 3;

            //Act
            var result = await repo.NewReview(reviewRepoModel);

            //Assert
            Assert.True(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Once());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task NewReview_NullReview_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.NewReview(null);

            //Assert
            Assert.False(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task NewReview_ReviewAlreadyExists_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.NewReview(reviewRepoModel);

            //Assert
            Assert.False(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task NewPurchases_FirstAlreadyExists_ShouldTrue()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.NewPurchases(purchaseRepoModel);

            //Assert
            Assert.True(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Once());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task NewPurchases_SecondAlreadyExists_ShouldTrue()
        {
            //Arrange
            DefaultSetup();
            purchaseRepoModel.OrderedItems[0].ProductId = 1;
            purchaseRepoModel.OrderedItems[1].ProductId = 4;

            //Act
            var result = await repo.NewPurchases(purchaseRepoModel);

            //Assert
            Assert.True(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Once());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task NewPurchases_NeitherAlreadyExist_ShouldTrue()
        {
            //Arrange
            DefaultSetup();
            purchaseRepoModel.OrderedItems[0].ProductId = 5;
            purchaseRepoModel.OrderedItems[1].ProductId = 4;

            //Act
            var result = await repo.NewPurchases(purchaseRepoModel);

            //Assert
            Assert.True(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Exactly(2));
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task NewPurchases_NeitherAlreadyExistButAreIdentical_ShouldTrueOnlyAddingOne()
        {
            //Arrange
            DefaultSetup();
            purchaseRepoModel.OrderedItems[0].ProductId = 5;
            purchaseRepoModel.OrderedItems[1].ProductId = 5;

            //Act
            var result = await repo.NewPurchases(purchaseRepoModel);

            //Assert
            Assert.True(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Once());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task NewPurchases_BothAlreadyExist_ShouldTrue()
        {
            //Arrange
            DefaultSetup();
            purchaseRepoModel.OrderedItems[0].ProductId = 1;
            purchaseRepoModel.OrderedItems[1].ProductId = 2;

            //Act
            var result = await repo.NewPurchases(purchaseRepoModel);

            //Assert
            Assert.True(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task NewPurchases_BothAlreadyExistButAreIdentical_ShouldTrue()
        {
            //Arrange
            DefaultSetup();
            purchaseRepoModel.OrderedItems[0].ProductId = 1;
            purchaseRepoModel.OrderedItems[1].ProductId = 1;

            //Act
            var result = await repo.NewPurchases(purchaseRepoModel);

            //Assert
            Assert.True(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task NewPurchases_NullPurchases_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.NewPurchases(null);

            //Assert
            Assert.False(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task NewPurchases_NullOrderedItems_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            purchaseRepoModel.OrderedItems = null;

            //Act
            var result = await repo.NewPurchases(purchaseRepoModel);

            //Assert
            Assert.False(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task NewPurchases_EmptyOrderedItems_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            purchaseRepoModel.OrderedItems = new List<ProductModel>();

            //Act
            var result = await repo.NewPurchases(purchaseRepoModel);

            //Assert
            Assert.False(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task PurchaseExists_ShouldTrue()
        {
            //Arrange
            DefaultSetup();
            int customerId = 1;
            int productId = 1;

            //Act
            var result = await repo.PurchaseExists(customerId,productId);

            //Assert
            Assert.True(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task PurchaseExists_DoesntExistsByCustomerId_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            int customerId = 99;
            int productId = 1;

            //Act
            var result = await repo.PurchaseExists(customerId, productId);

            //Assert
            Assert.False(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task PurchaseExists_DoesntExistsByProductId_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            int customerId = 1;
            int productId = 99;

            //Act
            var result = await repo.PurchaseExists(customerId, productId);

            //Assert
            Assert.False(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task PurchaseExists_DoesntExistsByCustomerIdOrProductId_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            int customerId = 99;
            int productId = 99;

            //Act
            var result = await repo.PurchaseExists(customerId, productId);

            //Assert
            Assert.False(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetReviewsByCustomerId__ShouldOkObject()
        {
            //Arrange
            DefaultSetup();
            int customerId = 1;
            var expectedReviews = dbReviews.Where(r => r.CustomerId == customerId && r.Visible).ToList();
            var expectedCustomer = dbCustomers.FirstOrDefault(c => c.CustomerId == customerId);

            //Act
            var result = repo.GetReviewsByCustomerId(customerId);

            //Assert
            Assert.NotNull(result);
            var reviews = await result as List<ReviewModel>;
            Assert.NotNull(reviews);
            Assert.True(expectedReviews.Count == reviews.Count);
            Assert.True(reviews.Any());
            for (int i = 0; i < reviews.Count; i++)
            {
                Assert.Equal(expectedReviews[i].CustomerId, reviews[i].CustomerId);
                Assert.Equal(expectedCustomer.CustomerName, reviews[i].CustomerName);
                Assert.Equal(expectedReviews[i].ProductId, reviews[i].ProductId);
                Assert.Equal(expectedReviews[i].Rating, reviews[i].Rating);
                Assert.Equal(expectedReviews[i].ReviewText, reviews[i].ReviewText);
                Assert.Equal(expectedReviews[i].TimeStamp, reviews[i].TimeStamp);
                Assert.Equal(expectedReviews[i].Visible, reviews[i].Visible);
            }
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetReviewsByCustomerId_ReviewIsntVisible_ShouldOkEmptyList()
        {
            //Arrange
            DefaultSetup();
            int customerId = 2;
            var expectedReviews = dbReviews.Where(r => r.CustomerId == customerId && r.Visible).ToList();
            var expectedCustomer = dbCustomers.FirstOrDefault(c => c.CustomerId == customerId);

            //Act
            var result = repo.GetReviewsByCustomerId(customerId);

            //Assert
            Assert.NotNull(result);
            var reviews = await result as List<ReviewModel>;
            Assert.NotNull(reviews);
            Assert.True(expectedReviews.Count == reviews.Count);
            Assert.False(reviews.Any());
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetReviewsByCustomerId_OneReviewIsntVisibleOneIs_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            int customerId = 1;
            dbReview2.Visible = false;
            var expectedReviews = dbReviews.Where(r => r.CustomerId == customerId && r.Visible).ToList();
            var expectedCustomer = dbCustomers.FirstOrDefault(c => c.CustomerId == customerId);

            //Act
            var result = repo.GetReviewsByCustomerId(customerId);

            //Assert
            Assert.NotNull(result);
            var reviews = await result as List<ReviewModel>;
            Assert.NotNull(reviews);
            Assert.True(expectedReviews.Count == reviews.Count);
            Assert.True(reviews.Any());
            for (int i = 0; i < reviews.Count; i++)
            {
                Assert.Equal(expectedReviews[i].CustomerId, reviews[i].CustomerId);
                Assert.Equal(expectedCustomer.CustomerName, reviews[i].CustomerName);
                Assert.Equal(expectedReviews[i].ProductId, reviews[i].ProductId);
                Assert.Equal(expectedReviews[i].Rating, reviews[i].Rating);
                Assert.Equal(expectedReviews[i].ReviewText, reviews[i].ReviewText);
                Assert.Equal(expectedReviews[i].TimeStamp, reviews[i].TimeStamp);
                Assert.Equal(expectedReviews[i].Visible, reviews[i].Visible);
            }
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetHiddenReviewsByCustomerId_OneReviewIsntVisibleOneIs_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            int customerId = 2;
            var expectedReviews = dbReviews.Where(r => r.CustomerId == customerId && !r.Visible).ToList();
            var expectedCustomer = dbCustomers.FirstOrDefault(c => c.CustomerId == customerId);

            //Act
            var result = repo.GetReviewsByCustomerId(customerId, false);

            //Assert
            Assert.NotNull(result);
            var reviews = await result as List<ReviewModel>;
            Assert.NotNull(reviews);
            Assert.True(expectedReviews.Count == reviews.Count);
            Assert.True(reviews.Any());
            for (int i = 0; i < reviews.Count; i++)
            {
                Assert.Equal(expectedReviews[i].CustomerId, reviews[i].CustomerId);
                Assert.Equal(expectedCustomer.CustomerName, reviews[i].CustomerName);
                Assert.Equal(expectedReviews[i].ProductId, reviews[i].ProductId);
                Assert.Equal(expectedReviews[i].Rating, reviews[i].Rating);
                Assert.Equal(expectedReviews[i].ReviewText, reviews[i].ReviewText);
                Assert.Equal(expectedReviews[i].TimeStamp, reviews[i].TimeStamp);
                Assert.Equal(expectedReviews[i].Visible, reviews[i].Visible);
            }
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetHiddenReviewsByCustomerId_NoneVisible_ShouldOkEmptyList()
        {
            //Arrange
            DefaultSetup();
            int customerId = 1;
            var expectedReviews = dbReviews.Where(r => r.CustomerId == customerId && !r.Visible).ToList();
            var expectedCustomer = dbCustomers.FirstOrDefault(c => c.CustomerId == customerId);

            //Act
            var result = repo.GetReviewsByCustomerId(customerId, false);

            //Assert
            Assert.NotNull(result);
            var reviews = await result as List<ReviewModel>;
            Assert.NotNull(reviews);
            Assert.True(expectedReviews.Count == reviews.Count);
            Assert.False(reviews.Any());
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetHiddenReviewsByCustomerId_CustomerDoesntExists_ShouldOkEmptyList()
        {
            //Arrange
            DefaultSetup();
            int customerId = 99;
            var expectedReviews = dbReviews.Where(r => r.CustomerId == customerId && !r.Visible).ToList();
            var expectedCustomer = dbCustomers.FirstOrDefault(c => c.CustomerId == customerId);

            //Act
            var result = repo.GetReviewsByCustomerId(customerId, false);

            //Assert
            Assert.NotNull(result);
            var reviews = await result as List<ReviewModel>;
            Assert.NotNull(reviews);
            Assert.True(expectedReviews.Count == reviews.Count);
            Assert.False(reviews.Any());
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetVisibleReviewsByCustomerId_CustomerDoesntExists_ShouldOkEmptyList()
        {
            //Arrange
            DefaultSetup();
            int customerId = 99;
            var expectedReviews = dbReviews.Where(r => r.CustomerId == customerId && r.Visible).ToList();
            var expectedCustomer = dbCustomers.FirstOrDefault(c => c.CustomerId == customerId);

            //Act
            var result = repo.GetReviewsByCustomerId(customerId, false);

            //Assert
            Assert.NotNull(result);
            var reviews = await result as List<ReviewModel>;
            Assert.NotNull(reviews);
            Assert.True(expectedReviews.Count == reviews.Count);
            Assert.False(reviews.Any());
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetReviewsByProductId__ShouldOkObject()
        {
            //Arrange
            DefaultSetup();
            int productId = 1;
            var expectedReviews = dbReviews.Where(r => r.ProductId == productId && r.Visible).ToList();
            var expectedCustomer = dbCustomers.FirstOrDefault(c => c.CustomerId == expectedReviews[0].CustomerId);

            //Act
            var result = repo.GetReviewsByProductId(productId);

            //Assert
            Assert.NotNull(result);
            var reviews = await result as List<ReviewModel>;
            Assert.NotNull(reviews);
            Assert.True(expectedReviews.Count == reviews.Count);
            Assert.True(reviews.Any());
            for (int i = 0; i < reviews.Count; i++)
            {
                Assert.Equal(expectedReviews[i].CustomerId, reviews[i].CustomerId);
                Assert.Equal(expectedCustomer.CustomerName, reviews[i].CustomerName);
                Assert.Equal(expectedReviews[i].ProductId, reviews[i].ProductId);
                Assert.Equal(expectedReviews[i].Rating, reviews[i].Rating);
                Assert.Equal(expectedReviews[i].ReviewText, reviews[i].ReviewText);
                Assert.Equal(expectedReviews[i].TimeStamp, reviews[i].TimeStamp);
                Assert.Equal(expectedReviews[i].Visible, reviews[i].Visible);
            }
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetReviewsByProductId_ReviewIsntVisible_ShouldOkEmptyList()
        {
            //Arrange
            DefaultSetup();
            int productId = 1;
            dbReview1.Visible = false;
            var expectedReviews = dbReviews.Where(r => r.ProductId == productId && r.Visible).ToList();

            //Act
            var result = repo.GetReviewsByProductId(productId);

            //Assert
            Assert.NotNull(result);
            var reviews = await result as List<ReviewModel>;
            Assert.NotNull(reviews);
            Assert.True(expectedReviews.Count == reviews.Count);
            Assert.False(reviews.Any());
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetReviewsByProductId_OneReviewIsntVisibleOneIs_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            int productId = 2;
            var expectedReviews = dbReviews.Where(r => r.ProductId == productId && r.Visible).ToList();
            var expectedCustomer = dbCustomers.FirstOrDefault(c => c.CustomerId == expectedReviews[0].CustomerId);

            //Act
            var result = repo.GetReviewsByProductId(productId);

            //Assert
            Assert.NotNull(result);
            var reviews = await result as List<ReviewModel>;
            Assert.NotNull(reviews);
            Assert.True(expectedReviews.Count == reviews.Count);
            Assert.True(reviews.Any());
            for (int i = 0; i < reviews.Count; i++)
            {
                Assert.Equal(expectedReviews[i].CustomerId, reviews[i].CustomerId);
                Assert.Equal(expectedCustomer.CustomerName, reviews[i].CustomerName);
                Assert.Equal(expectedReviews[i].ProductId, reviews[i].ProductId);
                Assert.Equal(expectedReviews[i].Rating, reviews[i].Rating);
                Assert.Equal(expectedReviews[i].ReviewText, reviews[i].ReviewText);
                Assert.Equal(expectedReviews[i].TimeStamp, reviews[i].TimeStamp);
                Assert.Equal(expectedReviews[i].Visible, reviews[i].Visible);
            }
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetHiddenReviewsByProductId_OneReviewIsntVisibleOneIs_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            int productId = 2;
            var expectedReviews = dbReviews.Where(r => r.ProductId == productId && !r.Visible).ToList();
            var expectedCustomer = dbCustomers.FirstOrDefault(c => c.CustomerId == expectedReviews[0].CustomerId);


            //Act
            var result = repo.GetReviewsByProductId(productId, false);

            //Assert
            Assert.NotNull(result);
            var reviews = await result as List<ReviewModel>;
            Assert.NotNull(reviews);
            Assert.True(expectedReviews.Count == reviews.Count);
            Assert.True(reviews.Any());
            for (int i = 0; i < reviews.Count; i++)
            {
                Assert.Equal(expectedReviews[i].CustomerId, reviews[i].CustomerId);
                Assert.Equal(expectedCustomer.CustomerName, reviews[i].CustomerName);
                Assert.Equal(expectedReviews[i].ProductId, reviews[i].ProductId);
                Assert.Equal(expectedReviews[i].Rating, reviews[i].Rating);
                Assert.Equal(expectedReviews[i].ReviewText, reviews[i].ReviewText);
                Assert.Equal(expectedReviews[i].TimeStamp, reviews[i].TimeStamp);
                Assert.Equal(expectedReviews[i].Visible, reviews[i].Visible);
            }
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetHiddenReviewsByProductId_NoneVisible_ShouldOkEmptyList()
        {
            //Arrange
            DefaultSetup();
            int productId = 1;
            var expectedReviews = dbReviews.Where(r => r.ProductId == productId && !r.Visible).ToList();

            //Act
            var result = repo.GetReviewsByProductId(productId, false);

            //Assert
            Assert.NotNull(result);
            var reviews = await result as List<ReviewModel>;
            Assert.NotNull(reviews);
            Assert.True(expectedReviews.Count == reviews.Count);
            Assert.False(reviews.Any());
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetHiddenReviewsByProductId_CustomerDoesntExists_ShouldOkEmptyList()
        {
            //Arrange
            DefaultSetup();
            int productId = 99;
            var expectedReviews = dbReviews.Where(r => r.ProductId == productId && !r.Visible).ToList();

            //Act
            var result = repo.GetReviewsByProductId(productId, false);

            //Assert
            Assert.NotNull(result);
            var reviews = await result as List<ReviewModel>;
            Assert.NotNull(reviews);
            Assert.True(expectedReviews.Count == reviews.Count);
            Assert.False(reviews.Any());
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetVisibleReviewsByProductId_CustomerDoesntExists_ShouldOkEmptyList()
        {
            //Arrange
            DefaultSetup();
            int productId = 99;
            var expectedReviews = dbReviews.Where(r => r.ProductId == productId && r.Visible).ToList();

            //Act
            var result = repo.GetReviewsByProductId(productId, false);

            //Assert
            Assert.NotNull(result);
            var reviews = await result as List<ReviewModel>;
            Assert.NotNull(reviews);
            Assert.True(expectedReviews.Count == reviews.Count);
            Assert.False(reviews.Any());
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetVisibleReviewAsCustomer__ShouldOk()
        {
            //Arrange
            DefaultSetup();
            int customerId = 1;
            int productId = 1;
            bool staff = false;
            var expectedReview = dbReviews.FirstOrDefault(r => r.ProductId == productId && r.CustomerId == customerId);
            var expectedCustomer = dbCustomers.FirstOrDefault(c => c.CustomerId == expectedReview.CustomerId);

            //Act
            var result = repo.GetReview(customerId, productId, false);

            //Assert
            Assert.NotNull(result);
            var review = await result as ReviewModel;
            Assert.NotNull(review);
            Assert.Equal(expectedReview.CustomerId, review.CustomerId);
            Assert.Equal(expectedCustomer.CustomerName, review.CustomerName);
            Assert.Equal(expectedReview.ProductId, review.ProductId);
            Assert.Equal(expectedReview.Rating, review.Rating);
            Assert.Equal(expectedReview.ReviewText, review.ReviewText);
            Assert.Equal(expectedReview.TimeStamp, review.TimeStamp);
            Assert.Equal(expectedReview.Visible, review.Visible);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetHiddenReviewAsCustomer__ShouldNull()
        {
            //Arrange
            DefaultSetup();
            int customerId = 2;
            int productId = 2;
            bool staff = false;

            //Act
            var result = await repo.GetReview(customerId, productId, false);

            //Assert
            Assert.Null(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetReviewAsCustomer_ReviewDoesntExistsByCustomerId_ShouldNull()
        {
            //Arrange
            DefaultSetup();
            int customerId = 99;
            int productId = 1;
            bool staff = false;

            //Act
            var result = await repo.GetReview(customerId, productId, false);

            //Assert
            Assert.Null(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetReviewAsCustomer_ReviewDoesntExistsByProductId_ShouldNull()
        {
            //Arrange
            DefaultSetup();
            int customerId = 1;
            int productId = 99;
            bool staff = false;

            //Act
            var result = await repo.GetReview(customerId, productId, false);

            //Assert
            Assert.Null(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetReviewAsCustomer_ReviewDoesntExistsByCustomerIdOrProductId_ShouldNull()
        {
            //Arrange
            DefaultSetup();
            int customerId = 99;
            int productId = 99;
            bool staff = false;

            //Act
            var result = await repo.GetReview(customerId, productId, false);

            //Assert
            Assert.Null(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetVisibleReviewAsCustomer__ShouldOk_NullableParameterOmitted()
        {
            //Arrange
            DefaultSetup();
            int customerId = 1;
            int productId = 1;
            var expectedReview = dbReviews.FirstOrDefault(r => r.ProductId == productId && r.CustomerId == customerId);
            var expectedCustomer = dbCustomers.FirstOrDefault(c => c.CustomerId == expectedReview.CustomerId);

            //Act
            var result = repo.GetReview(customerId, productId);

            //Assert
            Assert.NotNull(result);
            var review = await result as ReviewModel;
            Assert.NotNull(review);
            Assert.Equal(expectedReview.CustomerId, review.CustomerId);
            Assert.Equal(expectedCustomer.CustomerName, review.CustomerName);
            Assert.Equal(expectedReview.ProductId, review.ProductId);
            Assert.Equal(expectedReview.Rating, review.Rating);
            Assert.Equal(expectedReview.ReviewText, review.ReviewText);
            Assert.Equal(expectedReview.TimeStamp, review.TimeStamp);
            Assert.Equal(expectedReview.Visible, review.Visible);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetHiddenReviewAsCustomer__ShouldNull_NullableParameterOmitted()
        {
            //Arrange
            DefaultSetup();
            int customerId = 2;
            int productId = 2;

            //Act
            var result = await repo.GetReview(customerId, productId);

            //Assert
            Assert.Null(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetReviewAsCustomer_ReviewDoesntExistsByCustomerId_ShouldNull_NullableParameterOmitted()
        {
            //Arrange
            DefaultSetup();
            int customerId = 99;
            int productId = 1;

            //Act
            var result = await repo.GetReview(customerId, productId);

            //Assert
            Assert.Null(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetReviewAsCustomer_ReviewDoesntExistsByProductId_ShouldNull_NullableParameterOmitted()
        {
            //Arrange
            DefaultSetup();
            int customerId = 1;
            int productId = 99;

            //Act
            var result = await repo.GetReview(customerId, productId);

            //Assert
            Assert.Null(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetReviewAsCustomer_ReviewDoesntExistsByCustomerIdOrProductId_ShouldNull_NullableParameterOmitted()
        {
            //Arrange
            DefaultSetup();
            int customerId = 99;
            int productId = 99;

            //Act
            var result = await repo.GetReview(customerId, productId);

            //Assert
            Assert.Null(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetVisibleReviewAsStaff__ShouldOk()
        {
            //Arrange
            DefaultSetup();
            int customerId = 1;
            int productId = 1;
            bool staff = true;
            var expectedReview = dbReviews.FirstOrDefault(r => r.ProductId == productId && r.CustomerId == customerId);
            var expectedCustomer = dbCustomers.FirstOrDefault(c => c.CustomerId == expectedReview.CustomerId);

            //Act
            var result = repo.GetReview(customerId, productId, staff);

            //Assert
            Assert.NotNull(result);
            var review = await result as ReviewModel;
            Assert.NotNull(review);
            Assert.Equal(expectedReview.CustomerId, review.CustomerId);
            Assert.Equal(expectedCustomer.CustomerName, review.CustomerName);
            Assert.Equal(expectedReview.ProductId, review.ProductId);
            Assert.Equal(expectedReview.Rating, review.Rating);
            Assert.Equal(expectedReview.ReviewText, review.ReviewText);
            Assert.Equal(expectedReview.TimeStamp, review.TimeStamp);
            Assert.Equal(expectedReview.Visible, review.Visible);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetHiddenReviewAsStaff__ShouldNull()
        {
            //Arrange
            DefaultSetup();
            int customerId = 2;
            int productId = 2;
            bool staff = true;
            var expectedReview = dbReviews.FirstOrDefault(r => r.ProductId == productId && r.CustomerId == customerId);
            var expectedCustomer = dbCustomers.FirstOrDefault(c => c.CustomerId == expectedReview.CustomerId);

            //Act
            var result = repo.GetReview(customerId, productId, staff);

            //Assert
            Assert.NotNull(result);
            var review = await result as ReviewModel;
            Assert.NotNull(review);
            Assert.Equal(expectedReview.CustomerId, review.CustomerId);
            Assert.Equal(expectedCustomer.CustomerName, review.CustomerName);
            Assert.Equal(expectedReview.ProductId, review.ProductId);
            Assert.Equal(expectedReview.Rating, review.Rating);
            Assert.Equal(expectedReview.ReviewText, review.ReviewText);
            Assert.Equal(expectedReview.TimeStamp, review.TimeStamp);
            Assert.Equal(expectedReview.Visible, review.Visible);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetReviewAsStaff_ReviewDoesntExistsByCustomerId_ShouldNull()
        {
            //Arrange
            DefaultSetup();
            int customerId = 99;
            int productId = 1;
            bool staff = true;

            //Act
            var result = await repo.GetReview(customerId, productId, staff);

            //Assert
            Assert.Null(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetReviewAsStaff_ReviewDoesntExistsByProductId_ShouldNull()
        {
            //Arrange
            DefaultSetup();
            int customerId = 1;
            int productId = 99;
            bool staff = true;

            //Act
            var result = await repo.GetReview(customerId, productId, staff);

            //Assert
            Assert.Null(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetReviewAsStaff_ReviewDoesntExistsByCustomerIdOrProductId_ShouldNull()
        {
            //Arrange
            DefaultSetup();
            int customerId = 99;
            int productId = 99;
            bool staff = true;

            //Act
            var result = await repo.GetReview(customerId, productId, staff);

            //Assert
            Assert.Null(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ReviewExists_ShouldTrue()
        {
            //Arrange
            DefaultSetup();
            int customerId = 1;
            int productId = 1;

            //Act
            var result = await repo.ReviewExists(customerId, productId);

            //Assert
            Assert.True(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ReviewExists_DoesntExistsByCustomerId_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            int customerId = 99;
            int productId = 1;

            //Act
            var result = await repo.ReviewExists(customerId, productId);

            //Assert
            Assert.False(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ReviewExists_DoesntExistsByProductId_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            int customerId = 1;
            int productId = 99;

            //Act
            var result = await repo.ReviewExists(customerId, productId);

            //Assert
            Assert.False(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ReviewExists_DoesntExistsByCustomerIdOrProductId_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            int customerId = 99;
            int productId = 99;

            //Act
            var result = await repo.ReviewExists(customerId, productId);

            //Assert
            Assert.False(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task HideReview_ShouldTrue()
        {
            //Arrange
            DefaultSetup();
            int customerId = 1;
            int productId = 1;
            var expectedReview = dbReviews.FirstOrDefault(r => r.ProductId == productId && r.CustomerId == customerId);
            var expectedCustomer = dbCustomers.FirstOrDefault(c => c.CustomerId == expectedReview.CustomerId);



            //Act
            var result = await repo.HideReview(customerId, productId);

            //Assert
            Assert.True(result);
            Assert.Equal(expectedReview.CustomerId, dbReview1.CustomerId);
            Assert.Equal(expectedCustomer.CustomerName, dbCustomer1.CustomerName);
            Assert.Equal(expectedReview.ProductId, dbReview1.ProductId);
            Assert.Equal(expectedReview.Rating, dbReview1.Rating);
            Assert.Equal(expectedReview.ReviewText, dbReview1.ReviewText);
            Assert.Equal(expectedReview.TimeStamp, dbReview1.TimeStamp);
            Assert.False(dbReview1.Visible);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task HideReview_AlreadyHidden_ShouldTrue()
        {
            //Arrange
            DefaultSetup();
            int customerId = 1;
            int productId = 1;
            dbReview1.Visible = false;
            var expectedReview = dbReviews.FirstOrDefault(r => r.ProductId == productId && r.CustomerId == customerId);
            var expectedCustomer = dbCustomers.FirstOrDefault(c => c.CustomerId == expectedReview.CustomerId);



            //Act
            var result = await repo.HideReview(customerId, productId);

            //Assert
            Assert.True(result);
            Assert.Equal(expectedReview.CustomerId, dbReview1.CustomerId);
            Assert.Equal(expectedCustomer.CustomerName, dbCustomer1.CustomerName);
            Assert.Equal(expectedReview.ProductId, dbReview1.ProductId);
            Assert.Equal(expectedReview.Rating, dbReview1.Rating);
            Assert.Equal(expectedReview.ReviewText, dbReview1.ReviewText);
            Assert.Equal(expectedReview.TimeStamp, dbReview1.TimeStamp);
            Assert.False(dbReview1.Visible);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task HideReview_DoesntExistsByCustomerId_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            int customerId = 99;
            int productId = 1;

            //Act
            var result = await repo.HideReview(customerId, productId);

            //Assert
            Assert.False(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task HideReview_DoesntExistsByProductId_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            int customerId = 1;
            int productId = 99;

            //Act
            var result = await repo.HideReview(customerId, productId);

            //Assert
            Assert.False(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task HideReview_DoesntExistsByCustomerIdOrProductId_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            int customerId = 99;
            int productId = 99;

            //Act
            var result = await repo.HideReview(customerId, productId);

            //Assert
            Assert.False(result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Purchase>()), Times.Never());
            mockPurchases.Verify(m => m.Remove(It.IsAny<Purchase>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Review>()), Times.Never());
            mockReviews.Verify(m => m.Remove(It.IsAny<Review>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }
    }
}
