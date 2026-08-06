using Erp.Domain.Enums.Sys;

namespace Erp.UnitTests;

public class ScopeTypeTests
{
    [Fact]
    public void ScopeType_Has_Four_Tiers()
    {
        Assert.Equal(1, (int)ScopeType.Own);
        Assert.Equal(2, (int)ScopeType.Team);
        Assert.Equal(3, (int)ScopeType.Department);
        Assert.Equal(4, (int)ScopeType.All);
    }
}
