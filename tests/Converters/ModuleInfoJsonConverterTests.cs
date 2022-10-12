using System.Text.Json;
using FluentAssertions;

namespace KallitheaApiClient.Converters.Tests;

[TestClass()]
public class ModuleInfoJsonConverterTests
{
    [TestMethod()]
    public void Read_First()
    {
        var json = @"{ ""module"": [""<name>"", ""<version>""], ""ip_addr"": ""<ip_with_mask>"", ""ipv"": 4 }";
        var template = new { ip_addr = default(string), ipv = default(int), module = default(ModuleInfo), };
        deserializeAnonymousType(template, json)
            .Should().BeEquivalentTo(new { ip_addr = "<ip_with_mask>", ipv = 4, module = new ModuleInfo("<name>", "<version>"), });
    }

    [TestMethod()]
    public void Read_Middle()
    {
        var json = @"{ ""ip_addr"": ""<ip_with_mask>"", ""module"": [""<name>"", ""<version>""], ""ipv"": 4 }";
        var template = new { ip_addr = default(string), ipv = default(int), module = default(ModuleInfo), };
        deserializeAnonymousType(template, json)
            .Should().BeEquivalentTo(new { ip_addr = "<ip_with_mask>", ipv = 4, module = new ModuleInfo("<name>", "<version>"), });
    }

    [TestMethod()]
    public void Read_Last()
    {
        var json = @"{ ""ip_addr"": ""<ip_with_mask>"", ""ipv"": 4, ""module"": [""<name>"", ""<version>""] }";
        var template = new { ip_addr = default(string), ipv = default(int), module = default(ModuleInfo), };
        deserializeAnonymousType(template, json)
            .Should().BeEquivalentTo(new { ip_addr = "<ip_with_mask>", ipv = 4, module = new ModuleInfo("<name>", "<version>"), });
    }

    [TestMethod()]
    public void Read_Single()
    {
        var json = @"{ ""a"": [""a1"", ""a2""] }";
        var template = new { a = default(ModuleInfo), };
        deserializeAnonymousType(template, json).Should().BeEquivalentTo(new { a = new ModuleInfo("a1", "a2"), });
    }

    [TestMethod()]
    public void Read_Multiple()
    {
        var json = @"{ ""a"": [""a1"", ""a2""], ""b"": [""b1"", ""b2""], ""c"": [""c1"", ""c2""] }";
        var template = new { a = default(ModuleInfo), b = default(ModuleInfo), c = default(ModuleInfo), };
        deserializeAnonymousType(template, json)
            .Should().BeEquivalentTo(new { a = new ModuleInfo("a1", "a2"), b = new ModuleInfo("b1", "b2"), c = new ModuleInfo("c1", "c2"), });
    }

    [TestMethod()]
    public void Read_OverElements()
    {
        var json = @"{ ""a"": [""a1"", ""a2"", ""a3"", ""a4""] }";
        var template = new { a = default(ModuleInfo), };
        deserializeAnonymousType(template, json).Should().BeEquivalentTo(new { a = new ModuleInfo("a1", "a2"), });
    }

    [TestMethod()]
    public void Read_LessElements()
    {
        var json = @"{ ""a"": [""a1"" ] }";
        var template = new { a = default(ModuleInfo), };
        new Action(() => deserializeAnonymousType(template, json)).Should().Throw<JsonException>();
    }

    [TestMethod()]
    public void Write()
    {
        var data = new ModuleInfo("asd", "def");
        var json = JsonSerializer.Serialize(data);
        var element = JsonSerializer.Deserialize<JsonElement>(json);
        element.EnumerateArray().Select(e => e.GetString())
            .Should().BeEquivalentTo(new[] { "asd", "def", }, o => o.WithStrictOrdering());
    }


    private T? deserializeAnonymousType<T>(T template, string json) => JsonSerializer.Deserialize<T>(json);
}