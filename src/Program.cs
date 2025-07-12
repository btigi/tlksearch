using Microsoft.Extensions.Configuration;
using ii.InfinityEngine.Readers;
using System.Drawing;

var config = new ConfigurationBuilder()
    .AddJsonFile("tlksearch.json", optional: false)
    .Build();

if (args.Length == 0)
{
    Console.WriteLine("Usage: Program <searchterm> <directory>");
    return;
}

var directory = config["directory"] ?? string.Empty;
if (!Path.IsPathRooted(directory))
{
    directory = Path.Combine(System.AppContext.BaseDirectory, directory);
}
if (!Directory.Exists(directory))
{
    Console.WriteLine($"Directory '{directory}' does not exist!");
    return;
}


var lineNumberForegroundColour = Color.FromName(config["LineNumberForegroundColour"] ?? "Cyan");
var lineNumberForegroundConsoleColor = FromColour(lineNumberForegroundColour);

var lineNumberBackgroundColour = Color.FromName(config["LineNumberBackgroundColour"] ?? "Black");
var lineNumberBackgroundConsoleColor = FromColour(lineNumberBackgroundColour);

var fileForegroundColour = Color.FromName(config["FileForegroundColour"] ?? "Yellow");
var fileForegroundConsoleColour = FromColour(fileForegroundColour);

var fileBackgroundColour = Color.FromName(config["FileBackgroundColour"] ?? "Black");
var fileBackgroundConsoleColour = FromColour(fileBackgroundColour);

var textForegroundColour = Color.FromName(config["TextForegroundColour"] ?? "White");
var textForegroundConsoleColour = FromColour(textForegroundColour);

var textBackgroundColour = Color.FromName(config["TextBackgroundColour"] ?? "Black");
var textBackgroundConsoleColour = FromColour(textBackgroundColour);

var matchForegroundColour = Color.FromName(config["MatchForegroundColour"] ?? "Blue");
var matchForegroundConsoleColour = FromColour(matchForegroundColour);

var matchBackgroundColour = Color.FromName(config["MatchBackgroundColour"] ?? "Black");
var matchBackgroundConsoleColour = FromColour(matchBackgroundColour);



var existingBackgroundColour = Console.BackgroundColor;
var existingForegroundColour = Console.ForegroundColor;


var searchterm = string.Join(" ", args);

var tlkReader = new TlkFileBinaryReader();

var results = new List<(int strref, string filename, string text)>();
foreach (var file in Directory.EnumerateFiles(directory, "*.tlk", SearchOption.TopDirectoryOnly))
{
    var entries = tlkReader.Read(file);
    var hits = entries.Strings.Where(w => w.Text.Contains(searchterm, StringComparison.CurrentCultureIgnoreCase));
    results.AddRange(hits.Select(s => (s.Strref, file, s.Text)));
}

foreach (var h in results)
{
    Console.BackgroundColor = lineNumberBackgroundConsoleColor;
    Console.ForegroundColor = lineNumberForegroundConsoleColor;
    Console.Write($"  [{h.strref}]  ");
    Console.BackgroundColor = fileBackgroundConsoleColour;
    Console.ForegroundColor = fileForegroundConsoleColour;
    Console.Write($"{Path.GetFileName(h.filename)} ");
    
    var text = h.text;
    var lastIndex = 0;
    var searchIndex = 0;
    
    while ((searchIndex = text.IndexOf(searchterm, lastIndex, StringComparison.CurrentCultureIgnoreCase)) != -1)
    {
        if (searchIndex > lastIndex)
        {
            Console.BackgroundColor = textBackgroundConsoleColour;
            Console.ForegroundColor = textForegroundConsoleColour;
            Console.Write(text.Substring(lastIndex, searchIndex - lastIndex));
        }
        
        Console.BackgroundColor = matchBackgroundConsoleColour;
        Console.ForegroundColor = matchForegroundConsoleColour;
        Console.Write(text.Substring(searchIndex, searchterm.Length));
        
        lastIndex = searchIndex + searchterm.Length;
    }
    
    if (lastIndex < text.Length)
    {
        Console.BackgroundColor = textBackgroundConsoleColour;
        Console.ForegroundColor = textForegroundConsoleColour;
        Console.Write(text.Substring(lastIndex));
    }
    
    Console.WriteLine();
}

Console.BackgroundColor = existingBackgroundColour;
Console.ForegroundColor = existingForegroundColour;

static ConsoleColor FromColour(Color c)
{
    int index = (c.R > 128 | c.G > 128 | c.B > 128) ? 8 : 0; // Bright bit
    index |= (c.R > 64) ? 4 : 0; // Red bit
    index |= (c.G > 64) ? 2 : 0; // Green bit
    index |= (c.B > 64) ? 1 : 0; // Blue bit
    return (System.ConsoleColor)index;
}