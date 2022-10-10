using FluentAssertions;

namespace KallitheaApiClient.Converters.Tests;

[TestClass()]
public class RepoGroupPermJsonConverterTests
{
    [TestMethod()]
    public void Parse()
    {
        RepoGroupPermJsonConverter.Parse("group.none").Should().Be(RepoGroupPerm.none);
        RepoGroupPermJsonConverter.Parse("group.read").Should().Be(RepoGroupPerm.read);
        RepoGroupPermJsonConverter.Parse("group.write").Should().Be(RepoGroupPerm.write);
        RepoGroupPermJsonConverter.Parse("group.admin").Should().Be(RepoGroupPerm.admin);

        new Action(() => RepoGroupPermJsonConverter.Parse("none")).Should().Throw<NotSupportedException>();
    }
}