using Authorizer.Core.Enums;

namespace Authorizer.Core.Entities;

public class Card
{
    public Guid CardId { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string CardholderName { get; set; } = string.Empty;
    public CardStatus Status { get; set; }
}

