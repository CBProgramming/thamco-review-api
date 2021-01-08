using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ReviewData;
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
    public class ProductControllerTests
    {
        private PurchaseDto purchaseDto;
        private ProductDto productDto1, productDto2;
        private List<Purchase> repoPurchases;
        private FakeReviewRepository fakeRepo;
        private Mock<IReviewRepository> mockRepo;
        private IMapper mapper;
        private ILogger<ProductController> logger;
        private ProductController controller;
        private bool newPurchasesSucceeds = true;

        private void SetStandardProductDtos()
        {
            productDto1 = new ProductDto
            {
                ProductId = 1,
                ProductName = "Product 1"
            };
            productDto2 = new ProductDto
            {
                ProductId = 2,
                ProductName = "Product 2"
            };
        }

        private void SetStandardPurchaseDto()
        {
            SetStandardProductDtos();
            purchaseDto = new PurchaseDto
            {
                CustomerId = 1,
                CustomerAuthId = "fakeauthid",
                OrderedItems = new List<ProductDto>
                {
                    productDto1, productDto2
                }
            };
        }

        private void SetFakeRepo()
        {

            fakeRepo = new FakeReviewRepository();
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
                .CreateLogger<ProductController>();
        }

        private void SetMockReviewRepo()
        {
            mockRepo = new Mock<IReviewRepository>(MockBehavior.Strict);
            mockRepo.Setup(repo => repo.NewPurchases(It.IsAny<PurchaseModel>())).ReturnsAsync(newPurchasesSucceeds).Verifiable();
        }

        private void DefaultSetup()
        {
            SetStandardPurchaseDto();
            SetMapper();
            SetLogger();
        }

        private void SetupWithFakes()
        {
            DefaultSetup();
            SetFakeRepo();
            controller = new ProductController(logger, fakeRepo, mapper);
        }

        private void SetupWithMocks()
        {
            DefaultSetup();
            SetMockReviewRepo();
            controller = new ProductController(logger, mockRepo.Object, mapper);
        }

        [Fact]
        public async void PostNewPurchases_ShouldOk()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.Post(purchaseDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Purchases);
            Assert.Equal(purchaseDto.CustomerId, fakeRepo.Purchases.CustomerId);
            Assert.Equal(purchaseDto.CustomerAuthId, fakeRepo.Purchases.CustomerAuthId);
            Assert.Equal(purchaseDto.OrderedItems.Count, fakeRepo.Purchases.OrderedItems.Count);
            for (int i = 0; i < fakeRepo.Purchases.OrderedItems.Count; i++)
            {
                Assert.Equal(purchaseDto.OrderedItems[i].ProductId, fakeRepo.Purchases.OrderedItems[i].ProductId);
            }
        }

        [Fact]
        public async void PostNewPurchases_CheckMocks()
        {
            //Arrange
            SetupWithMocks();

            //Act
            var result = await controller.Post(purchaseDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.NewPurchases(It.IsAny<PurchaseModel>()), Times.Once);
        }

        [Fact]
        public async void PostNewPurchases_RepoFails_ShouldNotFound()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.Succeeds = false;

            //Act
            var result = await controller.Post(purchaseDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.Null(fakeRepo.Purchases);
        }

        [Fact]
        public async void PostNewPurchases_RepoFails_CheckMocks()
        {
            //Arrange
            newPurchasesSucceeds = false;
            SetupWithMocks();

            //Act
            var result = await controller.Post(purchaseDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.NewPurchases(It.IsAny<PurchaseModel>()), Times.Once);
        }

        [Fact]
        public async void PostNewPurchases_NullDto_ShouldUnprocessableEntity()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.Post(null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.Null(fakeRepo.Purchases);
        }

        [Fact]
        public async void PostNewPurchases_NullDto_CheckMocks()
        {
            //Arrange
            SetupWithMocks();

            //Act
            var result = await controller.Post(null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.NewPurchases(It.IsAny<PurchaseModel>()), Times.Never);
        }

        [Fact]
        public async void PostNewPurchases_InvalidCustomerId_ShouldUnprocessableEntity()
        {
            //Arrange
            SetupWithFakes();
            purchaseDto.CustomerId = 0;

            //Act
            var result = await controller.Post(purchaseDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.Null(fakeRepo.Purchases);
        }

        [Fact]
        public async void PostNewPurchases_InvalidCustomerId_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            purchaseDto.CustomerId = 0;

            //Act
            var result = await controller.Post(purchaseDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.NewPurchases(It.IsAny<PurchaseModel>()), Times.Never);
        }

        [Fact]
        public async void PostNewPurchases_NullAuthId_ShouldUnprocessableEntity()
        {
            //Arrange
            SetupWithFakes();
            purchaseDto.CustomerAuthId = null;

            //Act
            var result = await controller.Post(purchaseDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.Null(fakeRepo.Purchases);
        }

        [Fact]
        public async void PostNewPurchases_NullAuthId_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            purchaseDto.CustomerAuthId = null;

            //Act
            var result = await controller.Post(purchaseDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.NewPurchases(It.IsAny<PurchaseModel>()), Times.Never);
        }

        [Fact]
        public async void PostNewPurchases_EmptyAuthId_ShouldUnprocessableEntity()
        {
            //Arrange
            SetupWithFakes();
            purchaseDto.CustomerAuthId = "";

            //Act
            var result = await controller.Post(purchaseDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.Null(fakeRepo.Purchases);
        }

        [Fact]
        public async void PostNewPurchases_EmptyAuthId_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            purchaseDto.CustomerAuthId = "";

            //Act
            var result = await controller.Post(purchaseDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.NewPurchases(It.IsAny<PurchaseModel>()), Times.Never);
        }

        [Fact]
        public async void PostNewPurchases_NullOrderedItems_ShouldUnprocessableEntity()
        {
            //Arrange
            SetupWithFakes();
            purchaseDto.OrderedItems = null;

            //Act
            var result = await controller.Post(purchaseDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.Null(fakeRepo.Purchases);
        }

        [Fact]
        public async void PostNewPurchases_NullOrderedItems_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            purchaseDto.OrderedItems = null;

            //Act
            var result = await controller.Post(purchaseDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.NewPurchases(It.IsAny<PurchaseModel>()), Times.Never);
        }

        [Fact]
        public async void PostNewPurchases_EmptyOrderedItems_ShouldUnprocessableEntity()
        {
            //Arrange
            SetupWithFakes();
            purchaseDto.OrderedItems = new List<ProductDto>();

            //Act
            var result = await controller.Post(purchaseDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.Null(fakeRepo.Purchases);
        }

        [Fact]
        public async void PostNewPurchases_EmptyOrderedItems_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            purchaseDto.OrderedItems = new List<ProductDto>();

            //Act
            var result = await controller.Post(purchaseDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.NewPurchases(It.IsAny<PurchaseModel>()), Times.Never);
        }
    }
}
