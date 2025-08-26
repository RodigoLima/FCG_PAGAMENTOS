
using Moq;

namespace FCGPagamentos.API.Tests.UnitTests.Services;
public class PaymentServiceTests
{
  //private readonly Mock<IPaymentRepository> _paymentRepository;
  //private readonly PaymentService _paymentService;
  public PaymentServiceTests()
  {
    //_paymentRepository = new Mock<IPaymentRepository>();
    //_paymentService = new PaymentService(_paymentRepository.Object);
  }

  [Trait("Category", "UnitTest")]
  [Trait("Module", "PaymentService")]
  [Fact(DisplayName = "CheckoutAsync_ShouldReturnAnPayment")]
  public async Task CheckoutAsync_ShouldReturnAnPayment()
  {
    //Arrange
    //var payments = PaymentFaker.FakeListOfPayment(10);
    //_paymentRepository
    //    .Setup(g => g.GetAllAsync())
    //    .ReturnsAsync(payments);

    //Act
    //var result = await _paymentService.GetAllAsync();

    //Assert
    //result.Should().NotBeEmpty();
    //result.Count().Should().Be(payments.Count());
    throw new NotImplementedException();
  }
  [Trait("Category", "UnitTest")]
  [Trait("Module", "PaymentService")]
  [Fact(DisplayName = "UpdatePaymentStatusAsync_ShouldReturnAnUpdatedPayment")]
  public async Task UpdatePaymentStatusAsync_ShouldReturnAnUpdatedPayment()
  {
    //Arrange
    //var payments = PaymentFaker.FakeListOfPayment(10);
    //_paymentRepository
    //    .Setup(g => g.GetAllAsync())
    //    .ReturnsAsync(payments);

    //Act
    //var result = await _paymentService.GetAllAsync();

    //Assert
    //result.Should().NotBeEmpty();
    //result.Count().Should().Be(payments.Count());
    throw new NotImplementedException();
  }
}
