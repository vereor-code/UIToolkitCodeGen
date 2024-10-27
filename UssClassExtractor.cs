using ExCSS;
using System.Globalization;

namespace UIToolkitCodeGen
{
    internal class UssClassExtractor
    {
        private readonly StylesheetParser _parser = new(includeUnknownRules: true, includeUnknownDeclarations: true);

        public void Run(string path, string generatedNamespace)
        {
            foreach (var absolutePath in Directory.GetFiles(path, "*.uss", SearchOption.AllDirectories).OrderBy(x => x))
            {
                RunStylesheet(path, absolutePath, generatedNamespace);
            }
        }

        private void RunStylesheet(string path, string absolutePath, string generatedNamespace)
        {
            var relativePath = absolutePath.Replace(".uxml", string.Empty).Replace(path, string.Empty).TrimStart(Path.DirectorySeparatorChar);

            Console.WriteLine("\t" + relativePath);

            var stylesheet = _parser.Parse(File.ReadAllText(absolutePath));

            Console.WriteLine($"\t\t{stylesheet.StyleRules.Count()} rules in sheet");

            var seenStyles = new HashSet<string>();

            var textInfo = new CultureInfo("en-AU", false).TextInfo;

            var outputText = new List<string>();

            foreach (var rule in stylesheet.StyleRules)
            {
                var sel = rule.SelectorText;

                var lastSpace = sel.LastIndexOf(' ');

                var p = lastSpace == -1 ? [sel] : sel.Split(' ', ',');

                foreach (var p2 in p)
                {
                    var clean = p2.Trim('.', ',');

                    if (clean == "VisualElement") continue;
                    if (clean.StartsWith("unity")) continue;
                    if (clean.Contains(':')) continue;

                    if (clean.Contains('.'))
                    {
                        clean = clean.Split(".", StringSplitOptions.RemoveEmptyEntries)[^1];
                    }

                    if (clean.Contains(">"))
                    { 
                        clean = clean.Split(">", StringSplitOptions.RemoveEmptyEntries)[^1].Trim();
                        // Skip .someclass > TagName type styles
                        if (!clean.StartsWith("."))
                            continue;
                    }

                    var constantName = textInfo.ToTitleCase(clean.Replace("--", "__").Replace("-", "+").Replace(".", "_dot_")).Replace("+", string.Empty).Replace(">", "_GT_");

                    if (!seenStyles.Add(constantName))
                    {
                        continue;
                    }

                    var codeLine = $"\t\tpublic const string {constantName} = \"{clean}\";";
                    outputText.Add(codeLine);
                    Console.WriteLine(codeLine);
                }
            }

            var outPath = absolutePath.Replace(".uss", "ClassConstants.cs");

            var content = $$"""
                          // ReSharper disable InconsistentNaming
                          namespace {{generatedNamespace}}
                          {
                          """;
            var className = Path.GetFileNameWithoutExtension(absolutePath);

            content += $$"""
                      
                        public static class {{className}}
                        {
                      """;

            content += "\n" + string.Join("\n", outputText.OrderBy(x => x));

            content += """
                       
                        }
                       }
                       """;

            File.WriteAllText(outPath, content);

            Console.WriteLine($"\t\tOUTPUT: {outPath}");
            Console.WriteLine();
        }
    }
}
