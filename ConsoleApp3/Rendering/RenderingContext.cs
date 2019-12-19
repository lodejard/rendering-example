using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ConsoleApp3.Rendering
{
    public class RenderingContext
    {
        public IDictionary<string, JToken> Templates { get; set; } = new Dictionary<string, JToken>(StringComparer.Ordinal);
        public IDictionary<string, JToken> Outputs { get; set; } = new Dictionary<string, JToken>(StringComparer.Ordinal);

        public void AddTemplate(string path, JToken content)
        {
            Templates.Add(path, content);
        }

        public void AddOutput(string path, JToken content)
        {
            Outputs.Add(path, content);
        }

    }
}
