using AspNet.Security.OpenIdConnect.Primitives;
using AutoMapper;
using Microsoft.AspNetCore.Http;
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
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ReviewTests
{
    public class CustomerReviewControllerTests
    {
        private ReviewDto reviewDto;
        private ReviewModel reviewModel;
        private CustomerModel customerRepoModel;
        private FakeReviewRepository fakeRepo;
        private Mock<IReviewRepository> mockRepo;
        private IMapper mapper;
        private ILogger<CustomerReviewController> logger;
        private CustomerReviewController controller;
        private bool validAuthId = true;
        private bool purchaseExists = true;
        private bool newReviewSucceeds = true;
        private bool editReviewSucceeds = true;
        private bool reviewExists = true;
        private bool reviewVisible = true;

        private void SetupUser(CustomerReviewController controller)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "name"),
                                        new Claim(ClaimTypes.Name, "name"),
                                        new Claim(OpenIdConnectConstants.Claims.Subject, "fakeauthid" ),
                                        new Claim("client_id","customer_web_app"),
                                        new Claim("id", "1")
                                   }, "TestAuth")); ; ;
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        }

        public void SetupReviewDto()
        {
            reviewDto = new ReviewDto
            {
                CustomerId = 1,
                CustomerName = "Customer Name",
                ProductId = 1,
                Rating = 3,
                ReviewText = "Review Text",
                TimeStamp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)
            };
        }

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
                .CreateLogger<CustomerReviewController>();
        }

        private void SetMockReviewRepo()
        {
            mockRepo = new Mock<IReviewRepository>(MockBehavior.Strict);
            mockRepo.Setup(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()))
               .ReturnsAsync(new List<ReviewModel>()).Verifiable();
            mockRepo.Setup(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()))
               .ReturnsAsync(validAuthId).Verifiable();
            mockRepo.Setup(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()))
              .ReturnsAsync(new List<ReviewModel>()).Verifiable();
            mockRepo.Setup(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
              .ReturnsAsync(reviewExists?new ReviewModel {Visible = reviewVisible } :null).Verifiable();
            mockRepo.Setup(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()))
              .ReturnsAsync(purchaseExists).Verifiable();
            mockRepo.Setup(repo => repo.NewReview(It.IsAny<ReviewModel>()))
              .ReturnsAsync(newReviewSucceeds).Verifiable();
            mockRepo.Setup(repo => repo.EditReview(It.IsAny<ReviewModel>()))
              .ReturnsAsync(editReviewSucceeds).Verifiable();
            mockRepo.Setup(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()))
              .ReturnsAsync(editReviewSucceeds && reviewExists).Verifiable();
        }

        private void DefaultSetup()
        {
            SetupReviewDto();
            SetupReviewModel();
            SetupCustomerRepoModel();
            SetMapper();
            SetLogger();
        }

        private void SetupWithFakes()
        {
            DefaultSetup();
            SetFakeRepo();
            controller = new CustomerReviewController(logger, fakeRepo, mapper);
            SetupUser(controller);
        }

        private void SetupWithMocks()
        {
            DefaultSetup();
            SetMockReviewRepo();
            controller = new CustomerReviewController(logger, mockRepo.Object, mapper);
            SetupUser(controller);
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
        public async void GetReviewsByCustomerId_InvalidAuthId_ShouldForbid()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.Customer.CustomerAuthId = "different value";

            //Act
            var result = await controller.Get(1, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void GetReviewsByCustomerId_InvalidCustomerId_ShouldForbid()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.Customer.CustomerId = 2;

            //Act
            var result = await controller.Get(1, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
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
        public async void GetReviewsByCustomerIdAndProductId_InvalidAuthId_ShouldForbid()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.Customer.CustomerAuthId = "different value";

            //Act
            var result = await controller.Get(1, 1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void GetReviewsByCustomerIdAndProductId_InvalidCustomerId_ShouldForbid()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.Customer.CustomerId = 2;

            //Act
            var result = await controller.Get(1, 1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
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
        public async void PostReviewByCustomer_ReviewDoesntExist_ShouldOk()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist_ReviewScoreZero_ShouldOk()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            reviewDto.Rating = 0;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist_ReviewScoreFive_ShouldOk()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            reviewDto.Rating = 5;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist_InvalidCustomerId_ShouldForbid()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            reviewDto.CustomerId = 99;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist_InvalidAuthId_ShouldForbid()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            fakeRepo.Customer.CustomerAuthId = "different";

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist_EmptyReviewText_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            reviewDto.ReviewText = "";

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist_NullReviewText_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            reviewDto.ReviewText = null;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist_ReviewScoreNegative_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            reviewDto.Rating = -1;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist_ReviewScoreSix_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            reviewDto.Rating = 6;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist_PurchaseDoesntExist_ShouldNotFound()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            fakeRepo.PurchaseDoesExist = false;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist__FutureDate_ShouldOkWithTodaysDate()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            reviewDto.TimeStamp = new DateTime(2099, 1, 1, 1, 1, 1, 1);

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, DateTime.Now.Year);
            Assert.Equal(reviewDto.TimeStamp.Month, DateTime.Now.Month);
            Assert.Equal(reviewDto.TimeStamp.Day, DateTime.Now.Day);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist__DateSevenDaysAgoExactly_ShouldOkWithTodaysDate()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            reviewDto.TimeStamp = DateTime.Now.Subtract(TimeSpan.FromDays(7));

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, DateTime.Now.Year);
            Assert.Equal(reviewDto.TimeStamp.Month, DateTime.Now.Month);
            Assert.Equal(reviewDto.TimeStamp.Day, DateTime.Now.Day);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist__AlmostSevenDaysAgo_ShouldOkWithOriginal()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            reviewDto.TimeStamp = DateTime.Now.Subtract(TimeSpan.FromDays(7)).Add(TimeSpan.FromSeconds(2));
            int year = reviewDto.TimeStamp.Year;
            int month = reviewDto.TimeStamp.Month;
            int day = reviewDto.TimeStamp.Day;
            int hour = reviewDto.TimeStamp.Hour;
            int minute = reviewDto.TimeStamp.Minute;
            int second = reviewDto.TimeStamp.Second;
            int millisecond = reviewDto.TimeStamp.Millisecond;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, year);
            Assert.Equal(reviewDto.TimeStamp.Month, month);
            Assert.Equal(reviewDto.TimeStamp.Day, day);
            Assert.Equal(reviewDto.TimeStamp.Hour, hour);
            Assert.Equal(reviewDto.TimeStamp.Minute, minute);
            Assert.Equal(reviewDto.TimeStamp.Second, second);
            Assert.Equal(reviewDto.TimeStamp.Millisecond, millisecond);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist_ShouldOk()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist_ReviewScoreZero_ShouldOk()
        {
            //Arrange
            SetupWithFakes();
            reviewDto.Rating = 0;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist_ReviewScoreFive_ShouldOk()
        {
            //Arrange
            SetupWithFakes();
            reviewDto.Rating = 5;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist_InvalidCustomerId_ShouldForbid()
        {
            //Arrange
            SetupWithFakes();
            reviewDto.CustomerId = 99;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist_InvalidAuthId_ShouldForbid()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.Customer.CustomerAuthId = "different";

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist_EmptyReviewText_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            reviewDto.ReviewText = "";

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist_NullReviewText_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            reviewDto.ReviewText = null;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist_ReviewScoreNegative_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            reviewDto.Rating = -1;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist_ReviewScoreSix_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            reviewDto.Rating = 6;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist_PurchaseDoesntExist_ShouldNotFound()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.PurchaseDoesExist = false;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist__FutureDate_ShouldOkWithTodaysDate()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            SetupWithFakes();
            reviewDto.TimeStamp = new DateTime(2099, 1, 1, 1, 1, 1, 1);

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, DateTime.Now.Year);
            Assert.Equal(reviewDto.TimeStamp.Month, DateTime.Now.Month);
            Assert.Equal(reviewDto.TimeStamp.Day, DateTime.Now.Day);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist__DateSevenDaysAgoExactly_ShouldOkWithTodaysDate()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            SetupWithFakes();
            reviewDto.TimeStamp = DateTime.Now.Subtract(TimeSpan.FromDays(7));

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, DateTime.Now.Year);
            Assert.Equal(reviewDto.TimeStamp.Month, DateTime.Now.Month);
            Assert.Equal(reviewDto.TimeStamp.Day, DateTime.Now.Day);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist__AlmostSevenDaysAgo_ShouldOkWithOriginal()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            SetupWithFakes();
            reviewDto.TimeStamp = DateTime.Now.Subtract(TimeSpan.FromDays(7)).Add(TimeSpan.FromSeconds(2));
            int year = reviewDto.TimeStamp.Year;
            int month = reviewDto.TimeStamp.Month;
            int day = reviewDto.TimeStamp.Day;
            int hour = reviewDto.TimeStamp.Hour;
            int minute = reviewDto.TimeStamp.Minute;
            int second = reviewDto.TimeStamp.Second;
            int millisecond = reviewDto.TimeStamp.Millisecond;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, year);
            Assert.Equal(reviewDto.TimeStamp.Month, month);
            Assert.Equal(reviewDto.TimeStamp.Day, day);
            Assert.Equal(reviewDto.TimeStamp.Hour, hour);
            Assert.Equal(reviewDto.TimeStamp.Minute, minute);
            Assert.Equal(reviewDto.TimeStamp.Second, second);
            Assert.Equal(reviewDto.TimeStamp.Millisecond, millisecond);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist_ShouldOk()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist_ReviewScoreZero_ShouldOk()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            reviewDto.Rating = 0;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist_ReviewScoreFive_ShouldOk()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            reviewDto.Rating = 5;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist_InvalidCustomerId_ShouldForbid()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            reviewDto.CustomerId = 99;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist_InvalidAuthId_ShouldForbid()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            fakeRepo.Customer.CustomerAuthId = "different";

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist_EmptyReviewText_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            reviewDto.ReviewText = "";

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist_NullReviewText_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            reviewDto.ReviewText = null;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist_ReviewScoreNegative_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            reviewDto.Rating = -1;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist_ReviewScoreSix_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            reviewDto.Rating = 6;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist_PurchaseDoesntExist_ShouldNotFound()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            fakeRepo.PurchaseDoesExist = false;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist__FutureDate_ShouldOkWithTodaysDate()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            reviewDto.TimeStamp = new DateTime(2099, 1, 1, 1, 1, 1, 1);

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, DateTime.Now.Year);
            Assert.Equal(reviewDto.TimeStamp.Month, DateTime.Now.Month);
            Assert.Equal(reviewDto.TimeStamp.Day, DateTime.Now.Day);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist__DateSevenDaysAgoExactly_ShouldOkWithTodaysDate()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            reviewDto.TimeStamp = DateTime.Now.Subtract(TimeSpan.FromDays(7));

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, DateTime.Now.Year);
            Assert.Equal(reviewDto.TimeStamp.Month, DateTime.Now.Month);
            Assert.Equal(reviewDto.TimeStamp.Day, DateTime.Now.Day);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist__AlmostSevenDaysAgo_ShouldOkWithOriginal()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel = null;
            reviewDto.TimeStamp = DateTime.Now.Subtract(TimeSpan.FromDays(7)).Add(TimeSpan.FromSeconds(2));
            int year = reviewDto.TimeStamp.Year;
            int month = reviewDto.TimeStamp.Month;
            int day = reviewDto.TimeStamp.Day;
            int hour = reviewDto.TimeStamp.Hour;
            int minute = reviewDto.TimeStamp.Minute;
            int second = reviewDto.TimeStamp.Second;
            int millisecond = reviewDto.TimeStamp.Millisecond;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, year);
            Assert.Equal(reviewDto.TimeStamp.Month, month);
            Assert.Equal(reviewDto.TimeStamp.Day, day);
            Assert.Equal(reviewDto.TimeStamp.Hour, hour);
            Assert.Equal(reviewDto.TimeStamp.Minute, minute);
            Assert.Equal(reviewDto.TimeStamp.Second, second);
            Assert.Equal(reviewDto.TimeStamp.Millisecond, millisecond);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist_ShouldOk()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist_ReviewScoreZero_ShouldOk()
        {
            //Arrange
            SetupWithFakes();
            reviewDto.Rating = 0;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist_ReviewScoreFive_ShouldOk()
        {
            //Arrange
            SetupWithFakes();
            reviewDto.Rating = 5;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist_InvalidCustomerId_ShouldForbid()
        {
            //Arrange
            SetupWithFakes();
            reviewDto.CustomerId = 99;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist_InvalidAuthId_ShouldForbid()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.Customer.CustomerAuthId = "different";

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist_EmptyReviewText_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            reviewDto.ReviewText = "";

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist_NullReviewText_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            reviewDto.ReviewText = null;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist_ReviewScoreNegative_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            reviewDto.Rating = -1;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist_ReviewScoreSix_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            reviewDto.Rating = 6;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist_PurchaseDoesntExist_ShouldNotFound()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.PurchaseDoesExist = false;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist__FutureDate_ShouldOkWithTodaysDate()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            SetupWithFakes();
            reviewDto.TimeStamp = new DateTime(2099, 1, 1, 1, 1, 1, 1);

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, DateTime.Now.Year);
            Assert.Equal(reviewDto.TimeStamp.Month, DateTime.Now.Month);
            Assert.Equal(reviewDto.TimeStamp.Day, DateTime.Now.Day);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist__DateSevenDaysAgoExactly_ShouldOkWithTodaysDate()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            SetupWithFakes();
            reviewDto.TimeStamp = DateTime.Now.Subtract(TimeSpan.FromDays(7));

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, DateTime.Now.Year);
            Assert.Equal(reviewDto.TimeStamp.Month, DateTime.Now.Month);
            Assert.Equal(reviewDto.TimeStamp.Day, DateTime.Now.Day);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist__AlmostSevenDaysAgo_ShouldOkWithOriginal()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            SetupWithFakes();
            reviewDto.TimeStamp = DateTime.Now.Subtract(TimeSpan.FromDays(7)).Add(TimeSpan.FromSeconds(2));
            int year = reviewDto.TimeStamp.Year;
            int month = reviewDto.TimeStamp.Month;
            int day = reviewDto.TimeStamp.Day;
            int hour = reviewDto.TimeStamp.Hour;
            int minute = reviewDto.TimeStamp.Minute;
            int second = reviewDto.TimeStamp.Second;
            int millisecond = reviewDto.TimeStamp.Millisecond;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, year);
            Assert.Equal(reviewDto.TimeStamp.Month, month);
            Assert.Equal(reviewDto.TimeStamp.Day, day);
            Assert.Equal(reviewDto.TimeStamp.Hour, hour);
            Assert.Equal(reviewDto.TimeStamp.Minute, minute);
            Assert.Equal(reviewDto.TimeStamp.Second, second);
            Assert.Equal(reviewDto.TimeStamp.Millisecond, millisecond);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(reviewDto.CustomerId == fakeRepo.ReviewModel.CustomerId);
            Assert.True(reviewDto.CustomerName == fakeRepo.ReviewModel.CustomerName);
            Assert.True(reviewDto.ProductId == fakeRepo.ReviewModel.ProductId);
            Assert.True(reviewDto.Rating == fakeRepo.ReviewModel.Rating);
            Assert.True(reviewDto.ReviewText == fakeRepo.ReviewModel.ReviewText);
            Assert.True(reviewDto.TimeStamp == fakeRepo.ReviewModel.TimeStamp);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewExists_ReviewNotVisible_ShouldNotFound()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel.Visible = false;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewExists_ReviewNotVisible_ShouldNotFound()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel.Visible = false;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void DeleteReviewByCustomer_ReviewExists_ShouldOk()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.Delete(1, 1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Null(fakeRepo.ReviewModel);
        }

        [Fact]
        public async void DeleteReviewByCustomer_ReviewExists_InvalidCustomerId_ShouldForbid()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.Customer.CustomerId = 99;

            //Act
            var result = await controller.Delete(1, 1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.ReviewModel);
        }

        [Fact]
        public async void DeleteReviewByCustomer_ReviewExists_InvalidCustomerAuthId_ShouldForbid()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.Customer.CustomerAuthId = "different";

            //Act
            var result = await controller.Delete(1, 1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.ReviewModel);
        }

        [Fact]
        public async void DeleteReviewByCustomer_ReviewDoesntExistByCustomerId_ShouldForbid()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.ReviewModel.CustomerId = 99;

            //Act
            var result = await controller.Delete(1, 1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.ReviewModel);
        }

        [Fact]
        public async void DeleteReviewByCustomer_ReviewDoesntExistByProductId_ShouldForbid()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.Delete(1, 2);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.ReviewModel);
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
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
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
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
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
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void GetReviewsByCustomerId_InvalidAuthId_ShouldForbid_CheckMocks()
        {
            //Arrange
            validAuthId = false;
            SetupWithMocks();

            //Act
            var result = await controller.Get(1, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void GetReviewsByCustomerId_InvalidCustomerId_ShouldForbid_CheckMocks()
        {
            //Arrange
            validAuthId = false;
            SetupWithMocks();

            //Act
            var result = await controller.Get(1, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
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
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
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
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
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
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void GetReviewsByCustomerIdAndProductId_InvalidAuthId_ShouldForbid_CheckMocks()
        {
            //Arrange
            validAuthId = false;
            SetupWithMocks();

            //Act
            var result = await controller.Get(1, 1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void GetReviewsByCustomerIdAndProductId_InvalidCustomerId_ShouldForbid_CheckMocks()
        {
            //Arrange
            validAuthId = false;
            SetupWithMocks();

            //Act
            var result = await controller.Get(1, 1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
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
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
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
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
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
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist_ShouldOk_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            SetupWithMocks();

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist_ReviewScoreZero_ShouldOk_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            SetupWithMocks();
            reviewDto.Rating = 0;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist_ReviewScoreFive_ShouldOk_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            SetupWithMocks();
            reviewDto.Rating = 5;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist_InvalidCustomerId_ShouldForbid_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            validAuthId = false;
            SetupWithMocks();
            reviewDto.CustomerId = 99;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist_InvalidAuthId_ShouldForbid_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            validAuthId = false;
            SetupWithMocks();

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist_EmptyReviewText_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            SetupWithMocks();
            reviewDto.ReviewText = "";

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist_NullReviewText_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            SetupWithMocks();
            reviewDto.ReviewText = null;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist_ReviewScoreNegative_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            SetupWithMocks();
            reviewDto.Rating = -1;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist_ReviewScoreSix_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            SetupWithMocks();
            reviewDto.Rating = 6;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist_PurchaseDoesntExist_ShouldNotFound_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            purchaseExists = false;
            SetupWithMocks();

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist__FutureDate_ShouldOkWithTodaysDate_CheckMocks()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            reviewExists = false;
            SetupWithMocks();
            reviewDto.TimeStamp = new DateTime(2099, 1, 1, 1, 1, 1, 1);

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, DateTime.Now.Year);
            Assert.Equal(reviewDto.TimeStamp.Month, DateTime.Now.Month);
            Assert.Equal(reviewDto.TimeStamp.Day, DateTime.Now.Day);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist__DateSevenDaysAgoExactly_ShouldOkWithTodaysDate_CheckMocks()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            reviewExists = false;
            SetupWithMocks();
            reviewDto.TimeStamp = DateTime.Now.Subtract(TimeSpan.FromDays(7));

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, DateTime.Now.Year);
            Assert.Equal(reviewDto.TimeStamp.Month, DateTime.Now.Month);
            Assert.Equal(reviewDto.TimeStamp.Day, DateTime.Now.Day);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesntExist__AlmostSevenDaysAgo_ShouldOkWithOriginal_CheckMocks()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            reviewExists = false;
            SetupWithMocks();
            reviewDto.TimeStamp = DateTime.Now.Subtract(TimeSpan.FromDays(7)).Add(TimeSpan.FromSeconds(2));
            int year = reviewDto.TimeStamp.Year;
            int month = reviewDto.TimeStamp.Month;
            int day = reviewDto.TimeStamp.Day;
            int hour = reviewDto.TimeStamp.Hour;
            int minute = reviewDto.TimeStamp.Minute;
            int second = reviewDto.TimeStamp.Second;
            int millisecond = reviewDto.TimeStamp.Millisecond;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, year);
            Assert.Equal(reviewDto.TimeStamp.Month, month);
            Assert.Equal(reviewDto.TimeStamp.Day, day);
            Assert.Equal(reviewDto.TimeStamp.Hour, hour);
            Assert.Equal(reviewDto.TimeStamp.Minute, minute);
            Assert.Equal(reviewDto.TimeStamp.Second, second);
            Assert.Equal(reviewDto.TimeStamp.Millisecond, millisecond);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist_ShouldOk_CheckMocks()
        {
            //Arrange
            SetupWithMocks();

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist_ReviewScoreZero_ShouldOk_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            reviewDto.Rating = 0;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist_ReviewScoreFive_ShouldOk_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            reviewDto.Rating = 5;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist_InvalidCustomerId_ShouldForbid_CheckMocks()
        {
            //Arrange
            validAuthId = false;
            SetupWithMocks();

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist_InvalidAuthId_ShouldForbid_CheckMocks()
        {
            //Arrange
            validAuthId = false;
            SetupWithMocks();

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist_EmptyReviewText_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            reviewDto.ReviewText = "";

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist_NullReviewText_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            reviewDto.ReviewText = null;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist_ReviewScoreNegative_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            reviewDto.Rating = -1;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist_ReviewScoreSix_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            reviewDto.Rating = 6;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist_PurchaseDoesntExist_ShouldNotFound_CheckMocks()
        {
            //Arrange
            purchaseExists = false;
            SetupWithMocks();

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist__FutureDate_ShouldOkWithTodaysDate_CheckMocks()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            SetupWithMocks();
            reviewDto.TimeStamp = new DateTime(2099, 1, 1, 1, 1, 1, 1);

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, DateTime.Now.Year);
            Assert.Equal(reviewDto.TimeStamp.Month, DateTime.Now.Month);
            Assert.Equal(reviewDto.TimeStamp.Day, DateTime.Now.Day);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist__DateSevenDaysAgoExactly_ShouldOkWithTodaysDate_CheckMocks()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            SetupWithMocks();
            reviewDto.TimeStamp = DateTime.Now.Subtract(TimeSpan.FromDays(7));

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, DateTime.Now.Year);
            Assert.Equal(reviewDto.TimeStamp.Month, DateTime.Now.Month);
            Assert.Equal(reviewDto.TimeStamp.Day, DateTime.Now.Day);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewDoesExist__AlmostSevenDaysAgo_ShouldOkWithOriginal_CheckMocks()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            SetupWithMocks();
            reviewDto.TimeStamp = DateTime.Now.Subtract(TimeSpan.FromDays(7)).Add(TimeSpan.FromSeconds(2));
            int year = reviewDto.TimeStamp.Year;
            int month = reviewDto.TimeStamp.Month;
            int day = reviewDto.TimeStamp.Day;
            int hour = reviewDto.TimeStamp.Hour;
            int minute = reviewDto.TimeStamp.Minute;
            int second = reviewDto.TimeStamp.Second;
            int millisecond = reviewDto.TimeStamp.Millisecond;

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, year);
            Assert.Equal(reviewDto.TimeStamp.Month, month);
            Assert.Equal(reviewDto.TimeStamp.Day, day);
            Assert.Equal(reviewDto.TimeStamp.Hour, hour);
            Assert.Equal(reviewDto.TimeStamp.Minute, minute);
            Assert.Equal(reviewDto.TimeStamp.Second, second);
            Assert.Equal(reviewDto.TimeStamp.Millisecond, millisecond);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist_ShouldOk_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            SetupWithMocks();

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist_ReviewScoreZero_ShouldOk_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            SetupWithMocks();
            reviewDto.Rating = 0;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist_ReviewScoreFive_ShouldOk_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            SetupWithMocks();
            reviewDto.Rating = 5;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist_InvalidCustomerId_ShouldForbid_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            validAuthId = false;
            SetupWithMocks();
            reviewDto.CustomerId = 99;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist_InvalidAuthId_ShouldForbid_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            validAuthId = false;
            SetupWithMocks();

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist_EmptyReviewText_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            SetupWithMocks();
            reviewDto.ReviewText = "";

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist_NullReviewText_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            SetupWithMocks();
            reviewDto.ReviewText = null;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist_ReviewScoreNegative_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            SetupWithMocks();
            reviewDto.Rating = -1;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist_ReviewScoreSix_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            SetupWithMocks();
            reviewDto.Rating = 6;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist_PurchaseDoesntExist_ShouldNotFound_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            purchaseExists = false;
            SetupWithMocks();

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist__FutureDate_ShouldOkWithTodaysDate_CheckMocks()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            reviewExists = false;
            SetupWithMocks();

            reviewDto.TimeStamp = new DateTime(2099, 1, 1, 1, 1, 1, 1);

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, DateTime.Now.Year);
            Assert.Equal(reviewDto.TimeStamp.Month, DateTime.Now.Month);
            Assert.Equal(reviewDto.TimeStamp.Day, DateTime.Now.Day);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist__DateSevenDaysAgoExactly_ShouldOkWithTodaysDate_CheckMocks()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange

            reviewExists = false;
            SetupWithMocks();

            reviewDto.TimeStamp = DateTime.Now.Subtract(TimeSpan.FromDays(7));

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, DateTime.Now.Year);
            Assert.Equal(reviewDto.TimeStamp.Month, DateTime.Now.Month);
            Assert.Equal(reviewDto.TimeStamp.Day, DateTime.Now.Day);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesntExist__AlmostSevenDaysAgo_ShouldOkWithOriginal_CheckMocks()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            reviewExists = false;
            SetupWithMocks();

            reviewDto.TimeStamp = DateTime.Now.Subtract(TimeSpan.FromDays(7)).Add(TimeSpan.FromSeconds(2));
            int year = reviewDto.TimeStamp.Year;
            int month = reviewDto.TimeStamp.Month;
            int day = reviewDto.TimeStamp.Day;
            int hour = reviewDto.TimeStamp.Hour;
            int minute = reviewDto.TimeStamp.Minute;
            int second = reviewDto.TimeStamp.Second;
            int millisecond = reviewDto.TimeStamp.Millisecond;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, year);
            Assert.Equal(reviewDto.TimeStamp.Month, month);
            Assert.Equal(reviewDto.TimeStamp.Day, day);
            Assert.Equal(reviewDto.TimeStamp.Hour, hour);
            Assert.Equal(reviewDto.TimeStamp.Minute, minute);
            Assert.Equal(reviewDto.TimeStamp.Second, second);
            Assert.Equal(reviewDto.TimeStamp.Millisecond, millisecond);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist_ShouldOk_CheckMocks()
        {
            //Arrange
            SetupWithMocks();

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist_ReviewScoreZero_ShouldOk_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            reviewDto.Rating = 0;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist_ReviewScoreFive_ShouldOk_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            reviewDto.Rating = 5;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist_InvalidCustomerId_ShouldForbid_CheckMocks()
        {
            //Arrange
            validAuthId = false;
            SetupWithMocks();
            reviewDto.CustomerId = 99;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist_InvalidAuthId_ShouldForbid_CheckMocks()
        {
            //Arrange
            validAuthId = false;
            SetupWithMocks();

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist_EmptyReviewText_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            reviewDto.ReviewText = "";

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist_NullReviewText_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            reviewDto.ReviewText = null;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist_ReviewScoreNegative_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            reviewDto.Rating = -1;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist_ReviewScoreSix_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            reviewDto.Rating = 6;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist_PurchaseDoesntExist_ShouldNotFound_CheckMocks()
        {
            //Arrange
            purchaseExists = false;
            SetupWithMocks();

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist__FutureDate_ShouldOkWithTodaysDate_CheckMocks()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            SetupWithMocks();
            reviewDto.TimeStamp = new DateTime(2099, 1, 1, 1, 1, 1, 1);

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, DateTime.Now.Year);
            Assert.Equal(reviewDto.TimeStamp.Month, DateTime.Now.Month);
            Assert.Equal(reviewDto.TimeStamp.Day, DateTime.Now.Day);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist__DateSevenDaysAgoExactly_ShouldOkWithTodaysDate_CheckMocks()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            SetupWithMocks();
            reviewDto.TimeStamp = DateTime.Now.Subtract(TimeSpan.FromDays(7));

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, DateTime.Now.Year);
            Assert.Equal(reviewDto.TimeStamp.Month, DateTime.Now.Month);
            Assert.Equal(reviewDto.TimeStamp.Day, DateTime.Now.Day);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewDoesExist__AlmostSevenDaysAgo_ShouldOkWithOriginal_CheckMocks()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }

            //Arrange
            SetupWithMocks();
            reviewDto.TimeStamp = DateTime.Now.Subtract(TimeSpan.FromDays(7)).Add(TimeSpan.FromSeconds(2));
            int year = reviewDto.TimeStamp.Year;
            int month = reviewDto.TimeStamp.Month;
            int day = reviewDto.TimeStamp.Day;
            int hour = reviewDto.TimeStamp.Hour;
            int minute = reviewDto.TimeStamp.Minute;
            int second = reviewDto.TimeStamp.Second;
            int millisecond = reviewDto.TimeStamp.Millisecond;

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.Equal(reviewDto.TimeStamp.Year, year);
            Assert.Equal(reviewDto.TimeStamp.Month, month);
            Assert.Equal(reviewDto.TimeStamp.Day, day);
            Assert.Equal(reviewDto.TimeStamp.Hour, hour);
            Assert.Equal(reviewDto.TimeStamp.Minute, minute);
            Assert.Equal(reviewDto.TimeStamp.Second, second);
            Assert.Equal(reviewDto.TimeStamp.Millisecond, millisecond);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Once);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostReviewByCustomer_ReviewExists_ReviewNotVisible_ShouldNotFound_CheckMocks()
        {
            //Arrange
            reviewVisible = false;
            SetupWithMocks();

            //Act
            var result = await controller.Create(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PutReviewByCustomer_ReviewExists_ReviewNotVisible_ShouldNotFound_CheckMocks()
        {
            //Arrange
            reviewVisible = false;
            SetupWithMocks();
            

            //Act
            var result = await controller.Edit(reviewDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void DeleteReviewByCustomer_ReviewExists_ShouldOk_CheckMocks()
        {
            //Arrange
            SetupWithMocks();

            //Act
            var result = await controller.Delete(1, 1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async void DeleteReviewByCustomer_ReviewExists_InvalidCustomerId_ShouldForbid_CheckMocks()
        {
            //Arrange
            validAuthId = false;
            SetupWithMocks();

            //Act
            var result = await controller.Delete(1, 1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void DeleteReviewByCustomer_ReviewExists_InvalidCustomerAuthId_ShouldForbid_CheckMocks()
        {
            //Arrange
            validAuthId = false;
            SetupWithMocks();

            //Act
            var result = await controller.Delete(1, 1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void DeleteReviewByCustomer_ReviewDoesntExistByCustomerId_ShouldNotFound_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            SetupWithMocks();

            //Act
            var result = await controller.Delete(1, 1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async void DeleteReviewByCustomer_ReviewDoesntExistByProductId_ShouldNotFound_CheckMocks()
        {
            //Arrange
            reviewExists = false;
            SetupWithMocks();

            //Act
            var result = await controller.Delete(1, 2);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetReviewsByProductId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.ValidAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetReviewsByCustomerId(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.GetReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
            mockRepo.Verify(repo => repo.PurchaseExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditReview(It.IsAny<ReviewModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteReview(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }
    }
}

