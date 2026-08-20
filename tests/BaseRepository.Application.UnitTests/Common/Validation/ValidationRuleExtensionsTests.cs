using BaseRepository.Application.Common.Validation;
using FluentValidation;
using Xunit;

namespace BaseRepository.Application.UnitTests.Common.Validation;

public class ValidationRuleExtensionsTests
{
    private class PhoneModel
    {
        public string Phone { get; set; } = string.Empty;
    }

    private class PhoneValidator : AbstractValidator<PhoneModel>
    {
        public PhoneValidator() => RuleFor(x => x.Phone).PersianMobileNumber();
    }

    private class NationalCodeModel
    {
        public string NationalCode { get; set; } = string.Empty;
    }

    private class NationalCodeValidator : AbstractValidator<NationalCodeModel>
    {
        public NationalCodeValidator() => RuleFor(x => x.NationalCode).IranianNationalCode();
    }

    [Fact]
    public void PersianMobileNumberRule_WithAValidNumber_Passes()
    {
        var result = new PhoneValidator().Validate(new PhoneModel { Phone = "09123456789" });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void PersianMobileNumberRule_WithAnInvalidNumber_Fails()
    {
        var result = new PhoneValidator().Validate(new PhoneModel { Phone = "not-a-number" });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void IranianNationalCodeRule_WithAValidCode_Passes()
    {
        var result = new NationalCodeValidator().Validate(new NationalCodeModel { NationalCode = "1274327245" });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void IranianNationalCodeRule_WithAnInvalidCode_Fails()
    {
        var result = new NationalCodeValidator().Validate(new NationalCodeModel { NationalCode = "1234567890" });

        Assert.False(result.IsValid);
    }
}
