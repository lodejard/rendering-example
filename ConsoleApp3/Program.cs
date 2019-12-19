using ConsoleApp3.Rendering;
using DevLab.JmesPath;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace ConsoleApp3
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("Example 1");
            DoExample(
                templateFolder: "samples/basic", 
                dataFile: "values.yaml",
                outputFolder: "outputs/basic");

            Console.WriteLine();
            Console.WriteLine("Example 2");
            DoExample(
                templateFolder: "samples/advanced",
                dataFile: "values.yaml",
                outputFolder: "outputs/advanced");
        }

        private static void DoExample(string templateFolder, string dataFile, string outputFolder)
        {
            if (Directory.Exists(outputFolder))
            {
                Directory.Delete(outputFolder, true);
            }

            var context = new RenderingContext();
            foreach (var templateFile in Directory.EnumerateFiles(templateFolder, "*", new EnumerationOptions { RecurseSubdirectories = true, MatchType = MatchType.Simple, ReturnSpecialDirectories = false }))
            {
                Console.WriteLine($"Template file {templateFile}");
                var templatePath = Path.GetRelativePath(templateFolder, templateFile);
                context.AddTemplate(templatePath, Load(templateFile));
            }

            var data = Load(dataFile);

            var engine = new RenderingEngine();
            engine.Render(context, data);

            foreach(var (relativePath, content) in context.Outputs)
            {
                var outputFile = Path.Combine(outputFolder, relativePath);
                Console.WriteLine($"Output file {outputFile}");
                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
                File.WriteAllText(outputFile, content.ToString());
            }
        }

        private static JToken Load(string path)
        {
            var deserializer = new DeserializerBuilder().Build();
            using var reader = File.OpenText(path);
            var content = deserializer.Deserialize(reader);
            return JToken.FromObject(content);
        }
    }
}
