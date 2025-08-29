using Bogus;
using FCGPagamentos.Domain.Entites;

namespace FCGPagamentos.API.Tests.Utils;
public static class PaymentsFaker
{
  //public static List<Payment> FakeListOfPayments(int qtdToGenerate)
  //{
  //  var gameFaker = new Faker<Payment>()
  //              .RuleFor(g => g.Id, f => f.Random.Int(0, 3000))
  //              .RuleFor(g => g.CreatedAt, f => f.Date.Recent(30))
  //              .RuleFor(g => g.Name, f => f.Commerce.ProductName())
  //              .RuleFor(g => g.Description, f =>
  //              {
  //                var text = f.Lorem.Paragraphs(3);
  //                if (text.Length > 500)
  //                {
  //                  text = text.Substring(0, 500);
  //                  var lastSpace = text.LastIndexOf(' ');
  //                  if (lastSpace > 0)
  //                    text = text.Substring(0, lastSpace);
  //                }
  //                return text;
  //              })
  //              .RuleFor(g => g.Price, f => f.Random.Double(100, 500))
  //              .RuleFor(g => g.ReleasedDate, f => f.Date.Recent(30))
  //              .RuleFor(g => g.Genre, f => f.Lorem.Word());
  //  return gameFaker.Generate(qtdToGenerate);
  //}
}
