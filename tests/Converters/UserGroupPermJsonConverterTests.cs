using FluentAssertions;

namespace KallitheaApiClient.Converters.Tests;

[TestClass()]
public class UserGroupPermJsonConverterTests
{
    [TestMethod()]
    public void Parse()
    {
        UserGroupPermJsonConverter.Parse("usergroup.none").Should().Be(UserGroupPerm.none);
        UserGroupPermJsonConverter.Parse("usergroup.read").Should().Be(UserGroupPerm.read);
        UserGroupPermJsonConverter.Parse("usergroup.write").Should().Be(UserGroupPerm.write);
        UserGroupPermJsonConverter.Parse("usergroup.admin").Should().Be(UserGroupPerm.admin);

        new Action(() => UserGroupPermJsonConverter.Parse("none")).Should().Throw<NotSupportedException>();
    }
}