using SentinelCase.Application.Common.Interfaces;

namespace SentinelCase.UnitTests.TestDoubles;

internal sealed class FakeCurrentUser : ICurrentUser
{
    public FakeCurrentUser(string identifier)
    {
        Identifier = identifier;
    }

    public string Identifier { get; }
}
