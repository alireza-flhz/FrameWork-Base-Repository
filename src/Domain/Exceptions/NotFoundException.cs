namespace BaseRepository.Domain.Exceptions;

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object key)
        : base($"\"{entityName}\" with key ({key}) was not found.")
    {
    }
}
