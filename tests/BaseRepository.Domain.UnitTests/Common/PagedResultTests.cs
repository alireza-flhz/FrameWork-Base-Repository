using System;
using BaseRepository.Domain.Common;
using Xunit;

namespace BaseRepository.Domain.UnitTests.Common;

public class PagedResultTests
{
    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(1, 10, 1)]
    [InlineData(10, 10, 1)]
    [InlineData(11, 10, 2)]
    [InlineData(25, 10, 3)]
    public void TotalPages_IsComputedFromTotalCountAndPageSize(int totalCount, int pageSize, int expectedTotalPages)
    {
        var page = new PagedResult<int>(Array.Empty<int>(), totalCount, pageIndex: 1, pageSize);

        Assert.Equal(expectedTotalPages, page.TotalPages);
    }

    [Fact]
    public void HasPreviousPage_IsFalseOnFirstPage()
    {
        var page = new PagedResult<int>(Array.Empty<int>(), totalCount: 30, pageIndex: 1, pageSize: 10);

        Assert.False(page.HasPreviousPage);
        Assert.True(page.HasNextPage);
    }

    [Fact]
    public void HasNextPage_IsFalseOnLastPage()
    {
        var page = new PagedResult<int>(Array.Empty<int>(), totalCount: 30, pageIndex: 3, pageSize: 10);

        Assert.True(page.HasPreviousPage);
        Assert.False(page.HasNextPage);
    }
}
