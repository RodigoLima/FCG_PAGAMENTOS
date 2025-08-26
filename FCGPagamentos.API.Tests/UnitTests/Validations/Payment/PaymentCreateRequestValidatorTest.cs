using Bogus;
using FCGPagamentos.Application.Domain.Payments.DTOs;
using FCGPagamentos.Application.Domain.Payments.Messages;
using FCGPagamentos.Application.Domain.Payments.Validators;
using FluentValidation.TestHelper;

namespace FCGPagamentos.API.Tests.UnitTests.Validations.Payment;

public class PaymentCreateRequestValidatorTest
{
  private PaymentCreateRequestValidator _validator;

  public PaymentCreateRequestValidatorTest()
  {
    _validator = new PaymentCreateRequestValidator();
  }

  [Trait("Category", "UnitTest")]
  [Trait("Module", "PaymentCreateRequestValidator")]
  [Fact(DisplayName = "Validate_ShouldReturnRequiredFieldForGameId")]
  public void Validate_ShouldReturnRequiredFieldForGameId()
  {
    //Arrange
    var request = new PaymentCreateRequest
    {
      GameId = new Guid(),
      UserId = new Guid(),
      Amount = 0
    };

    //Act
    var result = _validator.TestValidate(request);

    //Assert
    result.ShouldHaveValidationErrorFor(g => g.GameId)
      .WithErrorMessage(PaymentMessages.GameId.Required);
  }

  [Trait("Category", "UnitTest")]
  [Trait("Module", "PaymentCreateRequestValidator")]
  [Fact(DisplayName = "Validate_ShouldReturnRequiredFieldForUserId")]
  public void Validate_ShouldReturnRequiredFieldForUserId()
  {
    //Arrange
    var request = new PaymentCreateRequest
    {
      GameId = new Guid(),
      UserId = new Guid(),
      Amount = 0
    };

    //Act
    var result = _validator.TestValidate(request);

    //Assert
    result.ShouldHaveValidationErrorFor(g => g.GameId)
      .WithErrorMessage(PaymentMessages.GameId.Required);
  }

  [Trait("Category", "UnitTest")]
  [Trait("Module", "PaymentCreateRequestValidator")]
  [Theory(DisplayName = "Validate_ShouldReturnAmountGreaterThanZero")]
  [InlineData(null)]
  [InlineData(0.0)]
  public void Validate_ShouldReturnAmountGreaterThanZero(decimal? amount)
  {
    //Arrange
    var request = new PaymentCreateRequest
    {
      GameId = new Guid(),
      UserId = new Guid(),
      Amount = 0
    };
    if (amount != null) request.Amount = (decimal)amount;

      //Act
      var result = _validator.TestValidate(request);

      //Assert
      result.ShouldHaveValidationErrorFor(g => g.Amount)
        .WithErrorMessage(PaymentMessages.Amount.GreaterThanZero);
  }
}
