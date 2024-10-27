using System.Xml;

namespace UIToolkitCodeGen;

public class GeneratedStructure
{
    public string? Key { get; set; }

    public List<GeneratedStructure>? Children { get; set; }
    public Dictionary<string, XmlNode>? Names { get; set; }
    public GeneratedStructure? Parent { get; set; }

    public GeneratedStructure EnsureChild(string part)
    {
        if (string.IsNullOrEmpty(part))
            return this;

        if (Children == null)
            Children = new List<GeneratedStructure>();

        if (Children.All(x => x.Key != part))
        {
            Children.Add(new GeneratedStructure() { Key = part, Parent = this });
        }

        return Children.First(x => x.Key == part);
    }
}