using FluentAssertions;

namespace KallitheaApiClient.Converters.Tests;

[TestClass()]
public class RepoPermJsonConverterTests
{
    [TestMethod()]
    public void Parse()
    {
        RepoPermJsonConverter.Parse("repository.none").Should().Be(RepoPerm.none);
        RepoPermJsonConverter.Parse("repository.read").Should().Be(RepoPerm.read);
        RepoPermJsonConverter.Parse("repository.write").Should().Be(RepoPerm.write);
        RepoPermJsonConverter.Parse("repository.admin").Should().Be(RepoPerm.admin);

        new Action(() => RepoPermJsonConverter.Parse("none")).Should().Throw<NotSupportedException>();
    }
}