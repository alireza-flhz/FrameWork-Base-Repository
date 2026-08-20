using FluentValidation;

namespace BaseRepository.Application.Common.Validation;

/// <summary>
/// Ready-made FluentValidation rules for common Iranian formats, wrapping the pure checks in
/// BaseRepository.Domain.Common. Usage: <c>RuleFor(x => x.Phone).PersianMobileNumber();</c>
/// Fully-qualified below on purpose - each rule method shares its name with the Domain type it
/// wraps, which would otherwise shadow that type inside this class.
/// </summary>
public static class ValidationRuleExtensions
{
    public static IRuleBuilderOptions<T, string> PersianMobileNumber<T>(this IRuleBuilder<T, string> ruleBuilder)
        => ruleBuilder
            .Must(BaseRepository.Domain.Common.PersianMobileNumber.IsValid)
            .WithMessage("'{PropertyName}' must be a valid Persian mobile number.");

    public static IRuleBuilderOptions<T, string> IranianNationalCode<T>(this IRuleBuilder<T, string> ruleBuilder)
        => ruleBuilder
            .Must(BaseRepository.Domain.Common.IranianNationalCode.IsValid)
            .WithMessage("'{PropertyName}' is not a valid Iranian national code.");
}
