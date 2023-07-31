using System.Buffers;
using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace KallitheaApiClient.Converters.Tests;

[TestClass()]
public class PropertySetJsonConverterTests
{
    [TestMethod()]
    public void Read_StringValue()
    {
        var json = """{ "xyz": "value1", "abc": "value2", "123": "value3" }""";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        var converter = new PropertySetJsonConverter<string>();
        var properties = converter.Read(ref reader, typeof(PropertySet<string>), JsonSerializerOptions.Default);
        properties.Should().Equal(new PropertyValue<string>[]
        {
            new("xyz", "value1"),
            new("abc", "value2"),
            new("123", "value3"),
        });
    }

    [TestMethod()]
    public void Read_ModuleInfoValue()
    {
        var json = """
        {
            "xyz": [ "name1", "value1" ],
            "abc": [ "name2", "value2" ],
            "123": [ "name3", "value3" ]
        }
        """;
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        var converter = new PropertySetJsonConverter<ModuleInfo>();
        var properties = converter.Read(ref reader, typeof(PropertySet<ModuleInfo>), JsonSerializerOptions.Default);
        properties.Should().Equal(new PropertyValue<ModuleInfo>[]
        {
            new("xyz", new("name1", "value1")),
            new("abc", new("name2", "value2")),
            new("123", new("name3", "value3")),
        });
    }

    private record TestObject(long id, string name, RepoType type);

    [TestMethod()]
    public void Read_TestObject()
    {
        var json = """
        {
            "xyz": { "id": 1, "name": "name1", "type": "git" },
            "abc": { "id": 2, "name": "name2", "type": "hg" },
            "123": { "id": 3, "name": "name3", "type": "git" }
        }
        """;
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        var converter = new PropertySetJsonConverter<TestObject>();
        var properties = converter.Read(ref reader, typeof(PropertySet<TestObject>), JsonSerializerOptions.Default);
        properties.Should().Equal(new PropertyValue<TestObject>[]
        {
            new("xyz", new(1, "name1", RepoType.git)),
            new("abc", new(2, "name2", RepoType.hg)),
            new("123", new(3, "name3", RepoType.git)),
        });
    }

    [TestMethod()]
    public void Write_StringValue()
    {
        var properties = new PropertySet<string>
        {
            new("xyz", "value1"),
            new("abc", "value2"),
            new("123", "value3"),
        };

        var buffer = new ArrayBufferWriter<byte>();
        var writer = new Utf8JsonWriter(buffer);
        var converter = new PropertySetJsonConverter<string>();
        converter.Write(writer, properties, JsonSerializerOptions.Default);
        writer.Flush();

        var json = Encoding.UTF8.GetString(buffer.WrittenSpan);
        var element = JsonSerializer.Deserialize<JsonElement>(json);
        element.GetProperty("xyz").GetString().Should().Be("value1");
        element.GetProperty("abc").GetString().Should().Be("value2");
        element.GetProperty("123").GetString().Should().Be("value3");
    }

    [TestMethod()]
    public void Write_ModuleInfoValue()
    {
        var properties = new PropertySet<ModuleInfo>
        {
            new("xyz", new("name1", "value1")),
            new("abc", new("name2", "value2")),
            new("123", new("name3", "value3")),
        };

        var buffer = new ArrayBufferWriter<byte>();
        var writer = new Utf8JsonWriter(buffer);
        var converter = new PropertySetJsonConverter<ModuleInfo>();
        converter.Write(writer, properties, JsonSerializerOptions.Default);
        writer.Flush();

        var json = Encoding.UTF8.GetString(buffer.WrittenSpan);
        var element = JsonSerializer.Deserialize<JsonElement>(json);
        element.GetProperty("xyz").EnumerateArray().Select(e => e.GetString()).Should().Equal(new[] { "name1", "value1", });
        element.GetProperty("abc").EnumerateArray().Select(e => e.GetString()).Should().Equal(new[] { "name2", "value2", });
        element.GetProperty("123").EnumerateArray().Select(e => e.GetString()).Should().Equal(new[] { "name3", "value3", });
    }
}