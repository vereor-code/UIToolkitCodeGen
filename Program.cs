using UIToolkitCodeGen;

Console.WriteLine("\n------------------");
Console.WriteLine("UI Toolkit Codegen");
Console.WriteLine("------------------\n");

const string path = @"d:\gamedev\shadowthrone\SrcUnity\Assets\game\UI";
const string outPath = @"D:\gamedev\Shadowthrone\SrcUnity\Assets\Game\UI\UIConstants.cs";
const string generatedNamespace = "Shadowthrone.Assets.Game.UI";

var ussExtractor = new UssClassExtractor();
ussExtractor.Run(path, generatedNamespace);

var extractor = new UxmlNameExtractor();
extractor.Run(path, outPath, generatedNamespace);