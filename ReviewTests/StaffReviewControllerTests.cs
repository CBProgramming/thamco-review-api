using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ReviewRepository;
using ReviewRepository.Models;
using ReviewService;
using ReviewService.Controllers;
using ReviewService.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ReviewTests
{
    public class StaffReviewControllerTests
    {
        private ReviewModel reviewModel;
        private CustomerModel customerRepoModel;
        private FakeReviewRepository fakeRepo;
        private Mock<IReviewRepository> mockRepo;
        private IMapper mapper;
        private ILogger<StaffReviewController> logger;
        private StaffReviewController controller;
        private bool reviewExists = true;
        private bool hideReviewSucceeds = true;

        public void SetupReviewModel()
        {
            reviewModel = new ReviewModel
            {
                CustomerId = 1,
                CustomerName = "Customer Name 2",
                ProductId = 1,
                Rating = 4,
                ReviewText = "Review Text 2",
                TimeStamp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day),
                Visible = true
            };
        }

        private void SetupCustomerRepoModel()
        {
            customerRepoModel = new CustomerModel
            {
                CustomerId = 1,
                CustomerAuthId = "fakeauthid",
                CustomerName = "Fake Name"
            };
        }

        private void SetFakeRepo()
        {
            fakeRepo = new FakeReviewRepository
            {
                ReviewModel = reviewModel,
                Customer = customerRepoModel
            };
        }

        private void SetMapper()
        {
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            }).CreateMapper();
        }

        private void SetLogger()
        {
            logger = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider()
                .GetService<ILoggerFactory>()
                .CreateLogger<StaffReviewController>();
        }

        private void SetMockReviewRepo()
        {
            mockRepo = new Mock<IReviewRepository>(MockBehavior.Strict);
            mockRepo.Setup(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()))
               .ReturnsAsync(new List<ReviewModel>()).Verifiable();
            mockRepo.Setup(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()))
              .ReturnsAsync(new List<ReviewModel>()).Verifiable();
            mockRepo.Setup(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
              .ReturnsAsync(reviewExists ? new ReviewModel () : null).Verifiable();
            mockRepo.Setup(repo => repo.HideReview(It.IsAny<int>(), It.IsAny<int>()))
              .ReturnsAsync(hideReviewSucceeds && reviewExists).Verifiable();
        }

        private void DefaultSetup()
        {
            SetupReviewModel();
            SetupCustomerRepoModel();
            SetMapper();
            SetLogger();
        }

        private void SetupWithFakes()
        {
            DefaultSetup();
            SetFakeRepo();
            controller = new StaffReviewController(logger, fakeRepo, mapper);
        }

        private void SetupWithMocks()
        {
            DefaultSetup();
            SetMockReviewRepo();
            controller = new StaffReviewController(logger, mockRepo.Object, mapper);
        }

        [Fact]
        public async void GetReviewsByProductId_NullCustomerId_ShouldOk()
        {
            //Arrange
            SetupWithFakes();
            int productId = 1;

            //Act
            var result = await controller.Get(null, productId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var reviews = objResult.Value as List<ReviewDto>;
            Assert.NotNull(reviews);
            Assert.True(1 == reviews.Count);
            Assert.True(reviewModel.CustomerId == reviews[0].CustomerId);
            Assert.True(reviewModel.CustomerName == reviews[0].CustomerName);
            Assert.True(reviewModel.ProductId == reviews[0].ProductId);
            Assert.True(reviewModel.Rating == reviews[0].Rating);
            Assert.True(reviewModel.ReviewText == reviews[0].ReviewText);
            Assert.True(reviewModel.TimeStamp == reviews[0].TimeStamp);
        }

        [Fact]
        public async void GetReviewsByProductId_InvalidCustomerId_ShouldOk()
        {
            //Arrange
            SetupWithFakes();
            int productId = 1;

            //Act
            var result = await controller.Get(0, productId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var reviews = objResult.Value as List<ReviewDto>;
            Assert.NotNull(reviews);
            Assert.True(1 == reviews.Count);
            Assert.True(reviewModel.CustomerId == reviews[0].CustomerId);
            Assert.True(reviewModel.CustomerName == reviews[0].CustomerName);
            Assert.True(reviewModel.ProductId == reviews[0].ProductId);
            Assert.True(reviewModel.Rating == reviews[0].Rating);
            Assert.True(reviewModel.ReviewText == reviews[0].ReviewText);
            Assert.True(reviewModel.TimeStamp == reviews[0].TimeStamp);
        }

        [Fact]
        public async void GetReviewsByProductId_NoReviewsForProduct_ShouldOkEmptyList()
        {
            //Arrange
            SetupWithFakes();
            int productId = 99;

            //Act
            var result = await controller.Get(0, productId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var reviews = objResult.Value as List<ReviewDto>;
            Assert.NotNull(reviews);
            Assert.True(0 == reviews.Count);
        }

        [Fact]
        public async void GetReviewsByCustomerId_NullProductId_ShouldOk()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.Get(1, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var reviews = objResult.Value as List<ReviewDto>;
            Assert.NotNull(reviews);
            Assert.True(1 == reviews.Count);
            Assert.True(reviewModel.CustomerId == reviews[0].CustomerId);
            Assert.True(reviewModel.CustomerName == reviews[0].CustomerName);
            Assert.True(reviewModel.ProductId == reviews[0].ProductId);
            Assert.True(reviewModel.Rating == reviews[0].Rating);
            Assert.True(reviewModel.ReviewText == reviews[0].ReviewText);
            Assert.True(reviewModel.TimeStamp == reviews[0].TimeStamp);
        }

        [Fact]
        public async void GetReviewsByCustomerId_InvalidProductId_ShouldOk()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.Get(1, 0);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var reviews = objResult.Value as List<ReviewDto>;
            Assert.NotNull(reviews);
            Assert.True(1 == reviews.Count);
            Assert.True(reviewModel.CustomerId == reviews[0].CustomerId);
            Assert.True(reviewModel.CustomerName == reviews[0].CustomerName);
            Assert.True(reviewModel.ProductId == reviews[0].ProductId);
            Assert.True(reviewModel.Rating == reviews[0].Rating);
            Assert.True(reviewModel.ReviewText == reviews[0].ReviewText);
            Assert.True(reviewModel.TimeStamp == reviews[0].TimeStamp);
        }

        [Fact]
        public async void GetReviewsByCustomerId_NoReviews_ShouldOkEmptyList()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel.CustomerId = 99;

            //Act
            var result = await controller.Get(1, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var reviews = objResult.Value as List<ReviewDto>;
            Assert.NotNull(reviews);
            Assert.True(0 == reviews.Count);
        }

        [Fact]
        public async void GetReviewsByCustomerIdAndProductId_ProductReviewDoesntExist_ShouldNotFound()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.Get(1, 99);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void GetReviewsByCustomerIdAndProductId_CustomerReviewDoesntExist_ShouldNotFound()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel.CustomerId = 99;

            //Act
            var result = await controller.Get(1, 1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void GetReviewsByCustomerIdAndProductId_ShouldOk()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.Get(1, 1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var review = objResult.Value as ReviewDto;
            Assert.NotNull(review);
            Assert.True(reviewModel.CustomerId == review.CustomerId);
            Assert.True(reviewModel.CustomerName == review.CustomerName);
            Assert.True(reviewModel.ProductId == review.ProductId);
            Assert.True(reviewModel.Rating == review.Rating);
            Assert.True(reviewModel.ReviewText == review.ReviewText);
            Assert.True(reviewModel.TimeStamp == review.TimeStamp);
        }

        [Fact]
        public async void GetReviewsByProductId_NullCustomerId_ShouldOk_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            int productId = 1;

            //Act
            var result = await controller.Get(null, productId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var reviews = objResult.Value as List<ReviewDto>;
            Assert.NotNull(reviews);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.HideReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void GetReviewsByProductId_InvalidCustomerId_ShouldOk_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            int productId = 1;

            //Act
            var result = await controller.Get(0, productId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var reviews = objResult.Value as List<ReviewDto>;
            Assert.NotNull(reviews);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.HideReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void GetReviewsByProductId_NoReviewsForProduct_ShouldOkEmptyList_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            int productId = 99;

            //Act
            var result = await controller.Get(0, productId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var reviews = objResult.Value as List<ReviewDto>;
            Assert.NotNull(reviews);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.HideReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }


        [Fact]
        public async void GetReviewsByCustomerId_NullProductId_ShouldOk_CheckMocks()
        {
            //Arrange
            SetupWithMocks();

            //Act
            var result = await controller.Get(1, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var reviews = objResult.Value as List<ReviewDto>;
            Assert.NotNull(reviews);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.HideReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void GetReviewsByCustomerId_InvalidProductId_ShouldOk_CheckMocks()
        {
            //Arrange
            SetupWithMocks();

            //Act
            var result = await controller.Get(1, 0);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var reviews = objResult.Value as List<ReviewDto>;
            Assert.NotNull(reviews);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.HideReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void GetReviewsByCustomerId_NoReviews_ShouldOkEmptyList_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            SetupWithMocks();

            //Act
            var result = await controller.Get(1, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var reviews = objResult.Value as List<ReviewDto>;
            Assert.NotNull(reviews);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.HideReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }


        [Fact]
        public async void GetReviewsByCustomerIdAndProductId_ProductReviewDoesntExist_ShouldNotFound_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            SetupWithMocks();

            //Act
            var result = await controller.Get(1, 99);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.HideReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void GetReviewsByCustomerIdAndProductId_CustomerReviewDoesntExist_ShouldNotFound_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            SetupWithMocks();

            //Act
            var result = await controller.Get(1, 1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.HideReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void GetReviewsByCustomerIdAndProductId_ShouldOk_CheckMocks()
        {
            //Arrange
            SetupWithMocks();

            //Act
            var result = await controller.Get(1, 1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var review = objResult.Value as ReviewDto;
            Assert.NotNull(review);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.HideReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void HideReview_ShouldOk()
        {
            //Arrange
            SetupWithFakes();
            int customerId = 1;
            int productId = 1;

            //Act
            var result = await controller.Delete(customerId, productId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void HideReview_ZeroCustomerId_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            int customerId = 0;
            int productId = 1;

            //Act
            var result = await controller.Delete(customerId, productId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void HideReview_ZeroProductId_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            int customerId = 1;
            int productId = 0;

            //Act
            var result = await controller.Delete(customerId, productId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void HideReview_ZeroCustomerAndProductId_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            int customerId = 0;
            int productId = 0;

            //Act
            var result = await controller.Delete(customerId, productId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void HideReview_DoesntExistsByCustomerId_ShouldNotFound()
        {
            //Arrange
            SetupWithFakes();
            int customerId = 99;
            int productId = 1;

            //Act
            var result = await controller.Delete(customerId, productId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void HideReview_DoesntExistByProductId_ShouldNotFound()
        {
            //Arrange
            SetupWithFakes();
            int customerId = 1;
            int productId = 99;

            //Act
            var result = await controller.Delete(customerId, productId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void HideReview_DoesntExistByCustomerOrProductId_ShouldNotFound()
        {
            //Arrange
            SetupWithFakes();
            int customerId = 99;
            int productId = 99;

            //Act
            var result = await controller.Delete(customerId, productId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void HideReview_ShouldOk_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            int customerId = 1;
            int productId = 1;

            //Act
            var result = await controller.Delete(customerId, productId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.HideReview(customerId, productId), Times.Once);
        }

        [Fact]
        public async void HideReview_ZeroCustomerId_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            int customerId = 0;
            int productId = 1;

            //Act
            var result = await controller.Delete(customerId, productId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.HideReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void HideReview_ZeroProductId_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            int customerId = 1;
            int productId = 0;

            //Act
            var result = await controller.Delete(customerId, productId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.HideReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void HideReview_ZeroCustomerAndProductId_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            int customerId = 0;
            int productId = 0;

            //Act
            var result = await controller.Delete(customerId, productId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.HideReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void HideReview_DoesntExistsByCustomerId_ShouldNotFound_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            SetupWithMocks();
            int customerId = 99;
            int productId = 1;

            //Act
            var result = await controller.Delete(customerId, productId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.HideReview(customerId, productId), Times.Once);
        }

        [Fact]
        public async void HideReview_DoesntExistByProductId_ShouldNotFound_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            SetupWithMocks();
            int customerId = 1;
            int productId = 99;

            //Act
            var result = await controller.Delete(customerId, productId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.HideReview(customerId, productId), Times.Once);
        }

        [Fact]
        public async void HideReview_DoesntExistByCustomerOrProductId_ShouldNotFound_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            SetupWithMocks();
            int customerId = 99;
            int productId = 99;

            //Act
            var result = await controller.Delete(customerId, productId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.HideReview(customerId, productId), Times.Once);
        }
    }
}
