using System.Text;
using System.Xml;

namespace UIToolkitCodeGen;

internal class UxmlNameExtractor
{
    public void Run(string path, string output, string generatedNamespace)
    {
        var structure = new GeneratedStructure() { Key = "UI" };

        foreach (var absolutePath in Directory.GetFiles(path, "*.uxml", SearchOption.AllDirectories).OrderBy(x => x))
        {
            var relativePath = absolutePath.Replace(".uxml", string.Empty).Replace(path, string.Empty).TrimStart(Path.DirectorySeparatorChar);

            var parts = relativePath.Split(Path.DirectorySeparatorChar);

            while (parts.Length > 1 && parts[^1] == parts[^2])
                parts = parts[..^1];

            Console.WriteLine("\t" + relativePath + " => " + string.Join('.', parts));

            var s = structure;

            foreach (var part in parts)
                s = s.EnsureChild(part);

            s.Names = new();

            RecursivelyGetNames(s.Names, File.ReadAllText(absolutePath));

            foreach (var g in s.Names.GroupBy(x => x.Value.Name))
            {
                var cleanName = g.First().Value.Name.Replace("ui:", string.Empty).Replace("engine:", string.Empty) + "s";
                if (cleanName.Contains("."))
                {
                    var pren = cleanName;
                    cleanName = cleanName.Split('.', StringSplitOptions.RemoveEmptyEntries)[^1];
                    Console.WriteLine($"Cleaned [{pren}] to [{cleanName}]");
                }

                s.Children ??= [];

                if (s.Children.All(x => x.Key != cleanName))
                {
                    s.Children.Add(new GeneratedStructure { Key = cleanName, Names = new() });
                }

                var myk = s.Children.First(x => x.Key == cleanName);

                foreach (var n in g.ToDictionary())
                {
                    if (myk.Names.ContainsKey(n.Key))
                        continue;

                    myk.Names.Add(n.Key, n.Value);
                }

            }

            s.Names = null;
        }

        Console.WriteLine();

        var sb = new StringBuilder();

        sb.AppendLine($$"""
                      namespace {{generatedNamespace}}
                      {
                      """);

        RecursiveAdd(structure, sb, 1);

        sb.AppendLine("}");

        File.WriteAllText(output, sb.ToString());
    }

    private void RecursiveAdd(GeneratedStructure s, StringBuilder sb, int level)
    {
        var k = string.IsNullOrEmpty(s.Key) ? "UNKNOWN" : s.Key;

        sb.AppendLine($$"""

                        {{GetTabs(level)}}public class {{k}}
                        {{GetTabs(level)}}{
                        """);

        if (s.Names != null)
        {
            foreach (var (name, node) in s.Names)
            {
                var cleanType = node.Name.Replace("ui:", string.Empty);

                var v = name.Replace(" ", "_").Replace("-", "_");

                if (v.EndsWith(cleanType))
                {
                    v = v[..v.LastIndexOf(cleanType, StringComparison.Ordinal)];
                }
                sb.AppendLine(GetTabs(level + 1) + $"public const string {v} = \"{name}\";");
            }
        }

        if (s.Children != null)
            foreach (var c in s.Children)
            {
                RecursiveAdd(c, sb, level + 1);
            }

        sb.AppendLine(GetTabs(level) + "}");
    }

    private static string GetTabs(int level)
    {
        return string.Empty.PadLeft(level, '\t');
    }

    private static void RecursivelyGetNames(Dictionary<string, XmlNode> names, string xmlContent)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlContent);

        RecursivelyGetNamesInternal(names, xmlDoc.FirstChild);
    }

    private static void RecursivelyGetNamesInternal(Dictionary<string, XmlNode> names, XmlNode node)
    {
        var thisName = node.Attr("name");

        if (!string.IsNullOrEmpty(thisName) && thisName != "VisualElement" && !node.Name.EndsWith("Template"))
        {
            if (names.TryAdd(thisName, node))
            {
                Console.WriteLine($"\t\tAdded {thisName} ({node.Name})");
            }
            else
            {
                Console.WriteLine($"\t\tAdded {thisName} NOT - IT FAILED ({node.Name})");
            }
        }

        foreach (XmlNode child in node.ChildNodes)
            RecursivelyGetNamesInternal(names, child);
    }
}