using BaseRepository.Application.Specifications;
using BaseRepository.Domain.Entities;

namespace BaseRepository.Application.Auth;

public class UserByPhoneNumberSpecification : Specification<User>
{
    public UserByPhoneNumberSpecification(string normalizedPhoneNumber, int? excludingUserId = null)
    {
        AddCriteria(excludingUserId is int id
            ? u => u.PhoneNumber == normalizedPhoneNumber && u.Id != id
            : u => u.PhoneNumber == normalizedPhoneNumber);
    }
}
