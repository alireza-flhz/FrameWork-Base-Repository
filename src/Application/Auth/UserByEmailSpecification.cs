using BaseRepository.Application.Specifications;
using BaseRepository.Domain.Entities;

namespace BaseRepository.Application.Auth;

public class UserByEmailSpecification : Specification<User>
{
    public UserByEmailSpecification(string normalizedEmail)
    {
        AddCriteria(u => u.Email == normalizedEmail);
    }
}
