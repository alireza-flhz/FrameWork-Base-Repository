using BaseRepository.Application.Common.Validation;
using FluentValidation;

namespace BaseRepository.Application.Auth;

public class UpdatePhoneNumberCommandValidator : AbstractValidator<UpdatePhoneNumberCommand>
{
    public UpdatePhoneNumberCommandValidator()
    {
        RuleFor(x => x.PhoneNumber!)
            .IranianMobileNumber()
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
