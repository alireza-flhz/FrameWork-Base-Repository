using BaseRepository.Domain.Common;
using Xunit;

namespace BaseRepository.Domain.UnitTests.Common;

public class ResultTests
{
    [Fact]
    public void Success_ProducesSucceededResultWithNoErrors()
    {
        var result = Result.Success();

        Assert.True(result.Succeeded);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failure_ProducesUnsucceededResultWithGivenErrors()
    {
        var result = Result.Failure("something went wrong");

        Assert.False(result.Succeeded);
        Assert.Equal("something went wrong", Assert.Single(result.Errors));
    }

    [Fact]
    public void GenericSuccess_CarriesTheValue()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.Succeeded);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void GenericFailure_HasDefaultValue()
    {
        var result = Result<string>.Failure("not found");

        Assert.False(result.Succeeded);
        Assert.Null(result.Value);
    }
}
