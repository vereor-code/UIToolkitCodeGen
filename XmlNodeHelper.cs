using System.Xml;

namespace UIToolkitCodeGen;

internal static class XmlNodeHelper
{
    public static string? Attr(this XmlNode element, string name)
    {
        if (element?.Attributes == null) return null;
        var a = element.Attributes[name];
        return a?.Value;
    }
}