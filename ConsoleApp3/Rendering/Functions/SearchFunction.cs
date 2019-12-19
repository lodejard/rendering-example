// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using DevLab.JmesPath;
using DevLab.JmesPath.Functions;
using Newtonsoft.Json.Linq;

namespace ConsoleApp3.Rendering.Functions
{
    public class SearchFunction : JmesPathFunction
    {
        private JmesPath _jmesPath;

        public SearchFunction(JmesPath jmesPath)
            : base("search", 2)
        {
            _jmesPath = jmesPath;
        }


        public override void Validate(params JmesPathFunctionArgument[] args)
        {
            base.Validate();
            EnsureArrayOrString(args[0]);
        }

        public override JToken Execute(params JmesPathFunctionArgument[] args)
        {
            var expression = args[0].Token.Value<string>();
            var token = args[1].Token;
            var json = token.ToString();
            var transformed = _jmesPath.Transform(json, expression);
            return JToken.Parse(transformed);
        }
    }
}
