using Synentra.Client.Models.Common;

namespace Synentra.Client.UnitTests.Models;

public sealed class PagedResultTests
{
    [Fact]
    public void HasNextPage_IsTrue_WhenMoreItemsExist()
    {
        var paged = new PagedResult<string> { Items = ["a"], Page = 1, PageSize = 10, TotalCount = 25 };

        paged.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_IsFalse_WhenOnLastPage()
    {
        var paged = new PagedResult<string> { Items = [], Page = 3, PageSize = 10, TotalCount = 25 };

        paged.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_IsTrue_WhenPageIsGreaterThanOne()
    {
        var paged = new PagedResult<string> { Items = [], Page = 2, PageSize = 10, TotalCount = 20 };

        paged.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void HasPreviousPage_IsFalse_WhenPageIsOne()
    {
        var paged = new PagedResult<string> { Items = [], Page = 1, PageSize = 10, TotalCount = 10 };

        paged.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void Items_DefaultsToEmptyList()
    {
        var paged = new PagedResult<int>();

        paged.Items.Should().BeEmpty();
    }
}
