using System.Buffers;
using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace KallitheaApiClient.Converters.Tests;

[TestClass()]
public class PropertySetJsonConverterFactoryTests
{
    [TestMethod()]
    public void CanConvert()
    {
        var factory = new PropertySetJsonConverterFactory();
        factory.CanConvert(typeof(PropertySet<string>)).Should().BeTrue();
        factory.CanConvert(typeof(PropertySet<int>)).Should().BeTrue();
        factory.CanConvert(typeof(PropertySet<ModuleInfo>)).Should().BeTrue();
        factory.CanConvert(typeof(string)).Should().BeFalse();
        factory.CanConvert(typeof(ModuleInfo)).Should().BeFalse();
    }

    [TestMethod()]
    public void CreateConverter()
    {
        var factory = new PropertySetJsonConverterFactory();
        factory.CreateConverter(typeof(PropertySet<string>), JsonSerializerOptions.Default).Should().BeOfType<PropertySetJsonConverter<string>>();
        factory.CreateConverter(typeof(PropertySet<ModuleInfo>), JsonSerializerOptions.Default).Should().BeOfType<PropertySetJsonConverter<ModuleInfo>>();
    }

}