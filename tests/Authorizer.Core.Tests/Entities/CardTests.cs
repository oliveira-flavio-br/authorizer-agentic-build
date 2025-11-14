using Authorizer.Core.Entities;
using Authorizer.Core.Enums;
using FluentAssertions;

namespace Authorizer.Core.Tests.Entities;

public class CardTests
{
    [Fact]
    public void Card_WhenCreated_ShouldHaveValidProperties()
    {
        // Arrange & Act
        var card = new Card
        {
            CardId = Guid.NewGuid(),
            CardNumber = "4111111111111111",
            CardholderName = "JOHN DOE",
            Status = CardStatus.Active
        };

        // Assert
        Assert.NotEqual(Guid.Empty, card.CardId);
        Assert.Equal("4111111111111111", card.CardNumber);
        Assert.Equal(CardStatus.Active, card.Status);
    }
}

