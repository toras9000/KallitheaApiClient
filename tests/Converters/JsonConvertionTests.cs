using System.Text.Json;
using FluentAssertions;

namespace KallitheaApiClient.Converters.Tests;

// 次作のクラスのテストではなく System.Text.Json の動きを確認する意味のもの。

[TestClass()]
public class JsonConvertionTests
{
    [TestMethod()]
    public void Member_Serialize()
    {
        {
            var source = new Member("name", MemberType.user_group, "permission");
            var json = JsonSerializer.Serialize(source);
            var element = JsonSerializer.Deserialize<JsonElement>(json);
            element.GetProperty(nameof(Member.name)).GetString().Should().Be("name");
            element.GetProperty(nameof(Member.type)).GetString().Should().Be("user_group");
            element.GetProperty(nameof(Member.permission)).GetString().Should().Be("permission");
        }
        {
            var source = new Member("aaa", MemberType.user, "bbb");
            var json = JsonSerializer.Serialize(source);
            var element = JsonSerializer.Deserialize<JsonElement>(json);
            element.GetProperty(nameof(Member.name)).GetString().Should().Be("aaa");
            element.GetProperty(nameof(Member.type)).GetString().Should().Be("user");
            element.GetProperty(nameof(Member.permission)).GetString().Should().Be("bbb");
        }
    }

    [TestMethod()]
    public void Member_Deserialize()
    {
        {
            var json = @"{""name"":""qwe"",""type"":""user_group"",""permission"":""asd""}";
            var obj = JsonSerializer.Deserialize<Member>(json)!;
            obj.name.Should().Be("qwe");
            obj.type.Should().Be(MemberType.user_group);
            obj.permission.Should().Be("asd");
        }
    }

    [TestMethod()]
    public void GetRepoNodesArgs_Serialize()
    {
        {
            var source = new GetRepoNodesArgs("repoid", "revision", "path");
            var json = JsonSerializer.Serialize(source);
            var element = JsonSerializer.Deserialize<JsonElement>(json);
            element.GetProperty(nameof(GetRepoNodesArgs.repoid)).GetString().Should().Be("repoid");
            element.GetProperty(nameof(GetRepoNodesArgs.revision)).GetString().Should().Be("revision");
            element.GetProperty(nameof(GetRepoNodesArgs.root_path)).GetString().Should().Be("path");
            element.GetProperty(nameof(GetRepoNodesArgs.ret_type)).GetString().Should().BeNullOrEmpty();
        }
        {
            var source = new GetRepoNodesArgs("repoid", "revision", "path", NodesType.files);
            var json = JsonSerializer.Serialize(source);
            var element = JsonSerializer.Deserialize<JsonElement>(json);
            element.GetProperty(nameof(GetRepoNodesArgs.repoid)).GetString().Should().Be("repoid");
            element.GetProperty(nameof(GetRepoNodesArgs.revision)).GetString().Should().Be("revision");
            element.GetProperty(nameof(GetRepoNodesArgs.root_path)).GetString().Should().Be("path");
            element.GetProperty(nameof(GetRepoNodesArgs.ret_type)).GetString().Should().Be("files");
        }
    }

    [TestMethod()]
    public void GetRepoNodesArgs_Deserialize()
    {
        {
            var json = @"{""repoid"":""aaa"",""revision"":""bbb"",""root_path"":""ccc""}";
            var obj = JsonSerializer.Deserialize<GetRepoNodesArgs>(json)!;
            obj.repoid.Should().Be("aaa");
            obj.revision.Should().Be("bbb");
            obj.root_path.Should().Be("ccc");
            obj.ret_type.Should().BeNull();
        }
        {
            var json = @"{""repoid"":""aaa"",""revision"":""bbb"",""root_path"":""ccc"",""ret_type"":""dirs""}";
            var obj = JsonSerializer.Deserialize<GetRepoNodesArgs>(json)!;
            obj.repoid.Should().Be("aaa");
            obj.revision.Should().Be("bbb");
            obj.root_path.Should().Be("ccc");
            obj.ret_type.Should().Be(NodesType.dirs);
        }
    }


}