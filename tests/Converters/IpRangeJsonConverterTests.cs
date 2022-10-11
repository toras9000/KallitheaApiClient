using System.Text.Json;
using FluentAssertions;

namespace KallitheaApiClient.Converters.Tests;

[TestClass()]
public class IpRangeJsonConverterTests
{
    [TestMethod()]
    public void Read_First()
    {
        var json = @"{ ""ip_range"": [""<start_ip>"", ""<end_ip>""], ""ip_addr"": ""<ip_with_mask>"", ""ipv"": 4 }";
        var template = new { ip_addr = default(string), ipv = default(int), ip_range = default(IpRange), };
        deserializeAnonymousType(template, json)
            .Should().BeEquivalentTo(new { ip_addr = "<ip_with_mask>", ipv = 4, ip_range = new IpRange("<start_ip>", "<end_ip>"), });
    }

    [TestMethod()]
    public void Read_Middle()
    {
        var json = @"{ ""ip_addr"": ""<ip_with_mask>"", ""ip_range"": [""<start_ip>"", ""<end_ip>""], ""ipv"": 4 }";
        var template = new { ip_addr = default(string), ipv = default(int), ip_range = default(IpRange), };
        deserializeAnonymousType(template, json)
            .Should().BeEquivalentTo(new { ip_addr = "<ip_with_mask>", ipv = 4, ip_range = new IpRange("<start_ip>", "<end_ip>"), });
    }

    [TestMethod()]
    public void Read_Last()
    {
        var json = @"{ ""ip_addr"": ""<ip_with_mask>"", ""ipv"": 4, ""ip_range"": [""<start_ip>"", ""<end_ip>""] }";
        var template = new { ip_addr = default(string), ipv = default(int), ip_range = default(IpRange), };
        deserializeAnonymousType(template, json)
            .Should().BeEquivalentTo(new { ip_addr = "<ip_with_mask>", ipv = 4, ip_range = new IpRange("<start_ip>", "<end_ip>"), });
    }

    [TestMethod()]
    public void Read_Single()
    {
        var json = @"{ ""a"": [""a1"", ""a2""] }";
        var template = new { a = default(IpRange), };
        deserializeAnonymousType(template, json).Should().BeEquivalentTo(new { a = new IpRange("a1", "a2"), });
    }

    [TestMethod()]
    public void Read_Multiple()
    {
        var json = @"{ ""a"": [""a1"", ""a2""], ""b"": [""b1"", ""b2""], ""c"": [""c1"", ""c2""] }";
        var template = new { a = default(IpRange), b = default(IpRange), c = default(IpRange), };
        deserializeAnonymousType(template, json)
            .Should().BeEquivalentTo(new { a = new IpRange("a1", "a2"), b = new IpRange("b1", "b2"), c = new IpRange("c1", "c2"), });
    }

    [TestMethod()]
    public void Read_OverElements()
    {
        var json = @"{ ""a"": [""a1"", ""a2"", ""a3"", ""a4""] }";
        var template = new { a = default(IpRange), };
        deserializeAnonymousType(template, json).Should().BeEquivalentTo(new { a = new IpRange("a1", "a2"), });
    }

    [TestMethod()]
    public void Read_LessElements()
    {
        var json = @"{ ""a"": [""a1"" ] }";
        var template = new { a = default(IpRange), };
        new Action(() => deserializeAnonymousType(template, json)).Should().Throw<JsonException>();
    }

    [TestMethod()]
    public void Write()
    {
        var data = new IpRange("asd", "def");
        var json = JsonSerializer.Serialize(data);
        var element = JsonSerializer.Deserialize<JsonElement>(json);
        element.EnumerateArray().Select(e => e.GetString())
            .Should().BeEquivalentTo(new[] { "asd", "def", }, o => o.WithStrictOrdering());
    }


    private T? deserializeAnonymousType<T>(T template, string json) => JsonSerializer.Deserialize<T>(json);
}