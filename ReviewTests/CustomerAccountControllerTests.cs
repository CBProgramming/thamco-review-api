using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ReviewRepository;
using ReviewRepository.Models;
using ReviewService;
using ReviewService.Controllers;
using ReviewService.Models;
using System;
using System.Security.Claims;
using Xunit;

namespace ReviewTests
{
    public class CustomerAccountControllerTests
    {
        private CustomerDto customerDto;
        private IConfiguration config;
        private CustomerModel customerRepoModel;
        private FakeReviewRepository fakeRepo;
        private Mock<IReviewRepository> mockRepo;
        private IMapper mapper;
        private ILogger<CustomerAccountController> logger;
        private CustomerAccountController controller;
        private bool anonymiseCustomerSucceeds = true;
        private bool editCustomerSucceeds = true;
        private bool newCustomerSucceeds = true;
        private bool customerExists = true;

        private void SetStandardCustomerDto()
        {
            customerDto = new CustomerDto
            {
                CustomerId = 1,
                CustomerAuthId = "fakeauthid",
                CustomerName = "Fake Name"
            };
        }

        private void SetStandardCustomerRepoModel()
        {
            customerRepoModel = new CustomerModel
            {
                CustomerId = 1,
                CustomerAuthId = "fakeauthid",
                CustomerName = "Fake Name"
            };
        }

        private CustomerDto GetEditedDetailsDto()
        {
            return new CustomerDto
            {
                CustomerId = 1,
                CustomerAuthId = "fakeauthid",
                CustomerName = "New Name"
            };
        }

        private void SetFakeRepo(CustomerModel customer)
        {
            fakeRepo = new FakeReviewRepository
            {
                Customer = customer
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
                .CreateLogger<CustomerAccountController>();
        }

        private void SetMockReviewRepo()
        {
            mockRepo = new Mock<IReviewRepository>(MockBehavior.Strict);
            mockRepo.Setup(repo => repo.AnonymiseCustomer(It.IsAny<int>()))
                .ReturnsAsync(anonymiseCustomerSucceeds && customerExists).Verifiable();
            mockRepo.Setup(repo => repo.EditCustomer(It.IsAny<CustomerModel>())).ReturnsAsync(editCustomerSucceeds).Verifiable();
            mockRepo.Setup(repo => repo.NewCustomer(It.IsAny<CustomerModel>())).ReturnsAsync(newCustomerSucceeds).Verifiable();
        }

        private void SetupApi(CustomerAccountController controller)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim("client_id","customer_account_api")
                                   }, "TestAuth"));
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        }

        private void DefaultSetup(bool withMocks = false, bool setupUser = true, bool setupApi = false)
        {
            SetStandardCustomerDto();
            SetStandardCustomerRepoModel();
            SetFakeRepo(customerRepoModel);
            SetMapper();
            SetLogger();
            SetMockReviewRepo();
        }

        private void SetupWithFakes()
        {
            DefaultSetup();
            controller = new CustomerAccountController(logger, fakeRepo, mapper);
            SetupApi(controller);
        }

        private void SetupWithMocks()
        {
            DefaultSetup();
            controller = new CustomerAccountController(logger, mockRepo.Object, mapper);
            SetupApi(controller);
        }

        [Fact]
        public async void PostNewCustomer_ShouldOk()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.Customer = null;

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerDto.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerDto.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.CustomerName, customerDto.CustomerName);
        }

        [Fact]
        public async void PostNewCustomer_VerifyMocks()
        {
            //Arrange
            SetupWithMocks();

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerModel>()), Times.Once);
        }

        [Fact]
        public async void PostNewCustomer_CustomerAlreadyExists_ShouldOkWithEditedDetails()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerDto.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerDto.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.CustomerName, customerDto.CustomerName);
        }

        [Fact]
        public async void PostNewCustomer_CustomerAlreadyExists_VerifyMocks()
        {
            //Arrange
            SetupWithMocks();

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerModel>()), Times.Once);
        }

        [Fact]
        public async void PostNewCustomer_NullCustomer_ShouldUnprocessableEntity()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.Post(null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerRepoModel.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerRepoModel.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.CustomerName, customerRepoModel.CustomerName);

        }

        [Fact]
        public async void PostNewCustomer_NullCustomer_CheckMocks()
        {
            //Arrange
            SetupWithMocks();

            //Act
            var result = await controller.Post(null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerModel>()), Times.Never);
        }

        [Fact]
        public async void PostNewCustomer_NullCustomerName_ShouldUnprocessableEntity()
        {
            //Arrange
            SetupWithFakes();
            customerDto.CustomerName = null;

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerRepoModel.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerRepoModel.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.CustomerName, customerRepoModel.CustomerName);
        }

        [Fact]
        public async void PostNewCustomer_NullCustomerName_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            customerDto.CustomerName = null;

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerModel>()), Times.Never);
        }

        [Fact]
        public async void PostNewCustomer_EmptyCustomerName_ShouldUnprocessableEntity()
        {
            //Arrange
            SetupWithFakes();
            customerDto.CustomerName = "";

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerRepoModel.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerRepoModel.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.CustomerName, customerRepoModel.CustomerName);
        }

        [Fact]
        public async void PostNewCustomer_EmptyCustomerName_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            customerDto.CustomerName = "";

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerModel>()), Times.Never);
        }

        [Fact]
        public async void EditCustomer_CustomerDoesntExist_ShouldOkCreatingNewCustomer()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.Put(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerDto.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerDto.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.CustomerName, customerDto.CustomerName);
        }

        [Fact]
        public async void EditCustomer_CustomerDoesntExist_VerifyMocks()
        {
            //Arrange
            SetupWithMocks();

            //Act
            var result = await controller.Put(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerModel>()), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerModel>()), Times.Never);
        }

        [Fact]
        public async void EditCustomer__ShouldOkWithEditedDetails()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.Put(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerDto.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerDto.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.CustomerName, customerDto.CustomerName);
        }

        [Fact]
        public async void EditCustomer_VerifyMocks()
        {
            //Arrange
            SetupWithMocks();

            //Act
            var result = await controller.Put(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerModel>()), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerModel>()), Times.Never);
        }

        [Fact]
        public async void EditCustomer_NullCustomer_ShouldUnprocessableEntity()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.Put(null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerRepoModel.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerRepoModel.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.CustomerName, customerRepoModel.CustomerName);
        }

        [Fact]
        public async void EditCustomer_NullCustomer_CheckMocks()
        {
            //Arrange
            SetupWithMocks();

            //Act
            var result = await controller.Put(null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerModel>()), Times.Never);
        }

        [Fact]
        public async void EditCustomer_NullCustomerName_ShouldUnprocessableEntity()
        {
            //Arrange
            SetupWithFakes();
            customerDto.CustomerName = null;

            //Act
            var result = await controller.Put(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerRepoModel.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerRepoModel.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.CustomerName, customerRepoModel.CustomerName);
        }

        [Fact]
        public async void EditCustomer_NullCustomerName_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            customerDto.CustomerName = null;

            //Act
            var result = await controller.Put(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerModel>()), Times.Never);
        }

        [Fact]
        public async void EditCustomer_EmptyCustomerName_ShouldUnprocessableEntity()
        {
            //Arrange
            SetupWithFakes();
            customerDto.CustomerName = "";

            //Act
            var result = await controller.Put(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerRepoModel.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerRepoModel.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.CustomerName, customerRepoModel.CustomerName);
        }

        [Fact]
        public async void EditCustomer_EmptyCustomerName_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            customerDto.CustomerName = "";

            //Act
            var result = await controller.Put(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerModel>()), Times.Never);
        }

        [Fact]
        public async void DeleteExistingCustomer_ShouldOk()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerDto.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerDto.CustomerAuthId);
            Assert.Equal("Anonymised", fakeRepo.Customer.CustomerName);
        }

        [Fact]
        public async void DeleteExistingCustomer_VerifyMocks()
        {
            //Arrange
            SetupWithMocks();

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerModel>()), Times.Never);
        }

        [Fact]
        public async void DeleteCustomer_CustomerDoesntExist_ShouldNotFound()
        {
            //Arrange
            SetupWithFakes();
            int customerId = 2;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerRepoModel.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerRepoModel.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.CustomerName, customerRepoModel.CustomerName);
        }

        [Fact]
        public async void DeleteExistingCustomer_CustomerDoesntExist_VerifyMocks()
        {
            //Arrange
            customerExists = false;
            SetupWithMocks();

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerModel>()), Times.Never);
        }

        [Fact]
        public async void DeleteExistingCustomer_RepoFails_ShouldNotFound()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.Succeeds = false;

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerRepoModel.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerRepoModel.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.CustomerName, customerRepoModel.CustomerName);
        }

        [Fact]
        public async void DeleteExistingCustomer_RepoFails_VerifyMocks()
        {
            //Arrange
            anonymiseCustomerSucceeds = false;
            SetupWithMocks();

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerModel>()), Times.Never);
        }
    }
}
