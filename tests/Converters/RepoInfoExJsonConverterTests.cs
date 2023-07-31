using System.Text.Json;
using FluentAssertions;

namespace KallitheaApiClient.Converters.Tests;

[TestClass()]
public class RepoInfoExJsonConverterTests
{
    [TestMethod()]
    public void Read_HasExFields()
    {
        var json = """
        {
          "repo_id": 345,
          "repo_name": "repo-name",
          "repo_type": "scm-tool",
          "private": false,
          "created_on": "2023-07-31T10:40:00.390",
          "description": "repo-desc",
          "landing_rev": [ "abc", "def" ],
          "owner": "foo",
          "fork_of": null,
          "enable_statistics": true,
          "enable_downloads": false,
          "last_changeset": {
            "short_id": "abc",
            "raw_id": "abcdefghijklm",
            "revision": 999,
            "message": "commit message",
            "date": "2023-07-31T10:40:07",
            "author": "foo <foo@example.com>"
          },
          "ex_testkey1": "testvalue1",
          "members": [
            {
              "name": "default",
              "type": "user",
              "permission": "repository.read"
            }
          ],
          "ex_testkey2": "testvalue2"
        }
        """;

        var repo = JsonSerializer.Deserialize<RepoInfoEx>(json);
        Assert.IsNotNull(repo);
        repo.repo_id.Should().Be(345);
        repo.repo_name.Should().Be("repo-name");
        repo.repo_type.Should().Be("scm-tool");
        repo.@private.Should().Be(false);
        repo.created_on.Should().Be("2023-07-31T10:40:00.390");
        repo.description.Should().Be("repo-desc");
        repo.landing_rev.Should().BeEquivalentTo("abc", "def");
        repo.owner.Should().Be("foo");
        repo.clone_uri.Should().BeNull();
        repo.fork_of.Should().BeNull();
        repo.enable_statistics.Should().Be(true);
        repo.enable_downloads.Should().Be(false);
        repo.last_changeset.short_id.Should().Be("abc");
        repo.last_changeset.raw_id.Should().Be("abcdefghijklm");
        repo.last_changeset.revision.Should().Be(999);
        repo.ex_fields.Should().BeEquivalentTo(new ExtraField[]
        {
            new("testkey1", "testvalue1"),
            new("testkey2", "testvalue2"),
        });
    }

    [TestMethod()]
    public void Read_NoExFields()
    {
        var json = """
        {
          "repo_id": 345,
          "last_changeset": {
            "short_id": "abc",
            "raw_id": "abcdefghijklm",
            "revision": 999,
            "message": "commit message",
            "date": "2023-07-31T10:40:07",
            "author": "foo <foo@example.com>"
          },
          "members": [
            {
              "name": "default",
              "type": "user",
              "permission": "repository.read"
            }
          ]
        }
        """;

        var repo = JsonSerializer.Deserialize<RepoInfoEx>(json);
        Assert.IsNotNull(repo);
        repo.repo_id.Should().Be(345);
        repo.ex_fields.Should().BeNull();
    }
}