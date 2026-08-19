namespace BaseRepository.Domain.Exceptions;

public sealed class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}
