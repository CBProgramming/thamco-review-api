using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ReviewRepository;
using ReviewRepository.Models;
using ReviewService;
using ReviewService.Controllers;
using System;
using System.Collections.Generic;
using System.Text;

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
              .ReturnsAsync(hideReviewSucceeds).Verifiable();
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
    }
}
