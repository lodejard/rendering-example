using ConsoleApp3.Rendering.Functions;
using DevLab.JmesPath;
using DevLab.JmesPath.Expressions;
using DevLab.JmesPath.Functions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp3.Rendering
{
    public class RenderingEngine
    {
        private readonly JmesPath jmesPath;

        public RenderingEngine()
        {
            jmesPath = new JmesPath();

            jmesPath.FunctionRepository
                .Register<ItemsFunction>()
                .Register<ToObjectFunction>()
                .Register<ZipFunction>()
                .Register<ConcatFunction>()
                .Register("search", new SearchFunction(jmesPath));
        }

        public void Render(RenderingContext context, JToken data)
        {
            foreach (var (path, template) in context.Templates)
            {
                var result = Render(context, template, data);
                if (result != null)
                {
                    context.AddOutput(path, result);
                }
            }
        }

        public JToken Render(RenderingContext context, JToken template, JToken data)
        {
            if (template.Type == JTokenType.Object)
            {
                return RenderObject(context, (JObject)template, data);
            }
            else if (template.Type == JTokenType.Array)
            {
                return RenderArray(context, (JArray)template, data);
            }
            else if (template.Type == JTokenType.String)
            {
                var templateString = template.ToString();
                if (templateString.StartsWith("{{") && templateString.EndsWith("}}"))
                {
                    return EvaluateToken(templateString[2..^2], data);
                }

                return template;
            }
            else
            {
                // return simple scalar value
                return template;
            }
        }

        public JToken RenderObject(RenderingContext context, JObject template, JToken data)
        {
            var result = new List<JToken>();
            var resultIsArray = false;
            var resultHasDirectives = false;

            foreach (var (childProperty, childTemplate) in template)
            {
                if (childProperty.StartsWith("{{#") && childProperty.EndsWith("}}"))
                {
                    resultHasDirectives = true;
                    resultIsArray |= childTemplate.Type == JTokenType.Array;

                    var parts = (childProperty[3..^2].Trim() + " ").Split(" ", 2, StringSplitOptions.None);
                    var action = parts[0];
                    var expression = parts[1].Trim();

                    if (action == "each")
                    {
                        var childData = EvaluateToken(expression, data);
                        if (childData.Type != JTokenType.Array)
                        {
                            throw new Exception("{{# each }} directive must produce an array");
                        }
                        foreach (var childContext in childData)
                        {
                            var childContent = Render(context, childTemplate, childContext);
                            if (childContent != null)
                            {
                                result.AddRange(childContent);
                            }
                        }
                    }
                    else if (action == "if" || action == "elseif")
                    {
                        if (EvaluateBool(expression, data))
                        {
                            return Render(context, childTemplate, data);
                        }
                    }
                    else if (action == "else")
                    {
                        return Render(context, childTemplate, data);
                    }
                    else if (action == "file")
                    {
                        var outputPath = EvaluateString(expression, data);
                        var outputContent = Render(context, childTemplate, data);
                        context.AddOutput(outputPath, outputContent);
                    }
                    else
                    {
                        throw new Exception("Unknown {{# }} directive");
                    }
                }
                else if (childProperty.StartsWith("{{") && childProperty.EndsWith("}}"))
                {
                    var propertyName = EvaluateString(childProperty[2..^2], data);
                    result.Add(new JProperty(propertyName, Render(context, childTemplate, data)));
                }
                else
                {
                    result.Add(new JProperty(childProperty, Render(context, childTemplate, data)));
                }
            }

            if (resultIsArray)
            {
                return new JArray(result.ToArray());
            }
            else if (resultHasDirectives && !result.Any())
            {
                return null;
            }
            else
            {
                return new JObject(result.ToArray());
            }
        }

        public JArray RenderArray(RenderingContext context, JArray template, JToken data)
        {
            var result = new JArray();
            foreach (var childTemplate in template)
            {
                result.Add(Render(context, childTemplate, data));
            }
            return result;
        }

        public JToken EvaluateToken(string expression, JToken data)
        {
            try
            {
                var jmesExpression = jmesPath.Parse(expression);
                var jmesArgument = new JmesPathArgument(data);
                return jmesExpression.Transform(jmesArgument).AsJToken();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing JMESPATH: {expression}", ex);
            }
        }

        public string EvaluateString(string expression, JToken data)
        {
            return EvaluateToken(expression, data).ToString();
        }

        public bool EvaluateBool(string expression, JToken data)
        {
            try
            {
                var jmesExpression = jmesPath.Parse(expression);
                var jmesArgument = new JmesPathArgument(data);
                return !JmesPathArgument.IsFalse(jmesExpression.Transform(jmesArgument));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing JMESPATH: {expression}", ex);
            }
        }
    }
}
