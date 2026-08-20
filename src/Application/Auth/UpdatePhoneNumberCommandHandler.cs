using System.Threading;
using System.Threading.Tasks;
using BaseRepository.Application.Abstractions;
using BaseRepository.Application.Messaging;
using BaseRepository.Domain.Entities;
using BaseRepository.Domain.Exceptions;

namespace BaseRepository.Application.Auth;

public class UpdatePhoneNumberCommandHandler : IRequestHandler<UpdatePhoneNumberCommand, UserProfileDto>
{
    private readonly IRepository<User, int> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IPhoneNumberValidator _phoneNumberValidator;

    public UpdatePhoneNumberCommandHandler(
        IRepository<User, int> repository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IPhoneNumberValidator phoneNumberValidator)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _phoneNumberValidator = phoneNumberValidator;
    }

    public async Task<UserProfileDto> Handle(UpdatePhoneNumberCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new AuthenticationFailedException("No authenticated user.");

        var user = await _repository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            user.PhoneNumber = null;
        }
        else
        {
            var normalized = _phoneNumberValidator.ToE164(request.PhoneNumber, request.Region);

            if (await _repository.AnyAsync(new UserByPhoneNumberSpecification(normalized, excludingUserId: userId), cancellationToken))
                throw new ConflictException($"Phone number \"{normalized}\" is already in use.");

            user.PhoneNumber = normalized;
        }

        _repository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserProfileDto { UserId = user.Id, Email = user.Email, PhoneNumber = user.PhoneNumber };
    }
}
