namespace AutoLuki.Domain;

public interface IHasCreatedAt
{
    DateTimeOffset CreatedAt { get; set; }
}

public interface IAuditable : IHasCreatedAt
{
    DateTimeOffset UpdatedAt { get; set; }
}
