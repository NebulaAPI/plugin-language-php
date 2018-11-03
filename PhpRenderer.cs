using System.Collections.Generic;
using System.Linq;
using Nebula.SDK.Compiler.Abstracts;
using Nebula.SDK.Compiler.Objects;
using Nebula.SDK.Plugin;
using Nebula.SDK.Objects;
using Nebula.SDK.Util;

namespace Nebula.SDK.Renderers
{
    public class PhpRenderer : AbstractRenderer
    {
        public PhpRenderer(AbstractCompiler compiler, IRendererExtension renderPlugin) : base(compiler, renderPlugin)
        {
        }

        protected override string ConvertTypeName(string inputType)
        {
            switch (inputType)
            {
                case "integer": return "int";
                case "boolean": return "bool";
                case "datetime": return @"\DateTime";
                default: return inputType;
            }
        }

        protected override void RenderAbstractConstructor(AbstractConstructor ac)
        {
            
        }

        protected override string RenderAbstractDataType(AbstractDataType abstractData)
        {
            if (abstractData.Node.Generic && abstractData.Node.Name == "array")
            {
                return $"{abstractData.Node.GenericType}[]";
            }

            return ConvertTypeName(abstractData.Node.Name);
        }

        private string GetHttpMethod(TokenType functionType)
        {
            switch (functionType)
            {
                case TokenType.GetFunction: return "GET";
                case TokenType.PostFunction: return "POST";
                case TokenType.PutFunction: return "PUT";
                case TokenType.DeleteFunction: return "DELETE";
                case TokenType.PatchFunction: return "PATCH";
                default: throw new System.Exception("Unknown function method type");
            }
        }

        protected override void RenderAbstractFunction(AbstractFunction function)
        {
            var visibility = function.AccessModifier.ToString().ToLower();
            var rt = RenderAbstractDataType(function.ReturnType);
            var args = string.Join(", ", function.Arguments.Select(a => RenderAbstractVariableDefinition(a)));
            var method = GetHttpMethod(function.Node.Method);
            var fname = function.Name.ToProperCase().ToPascalCase();
            var prefix = _activeConfig.Prefix;
            var url = function.Node.Url;

            WriteIndented($"{visibility} function {fname}({args}) : {rt}");
            WriteIndented("{");
            _indentLevel++;
            WriteIndented(
                _rendererExtension.RenderAbstractFunction(
                    url,
                    prefix,
                    rt,
                    method,
                    function.Node.Args.Select(a => a.Name).ToList()
                )
            );
            _indentLevel--;
            WriteIndented("}");
        }

        protected override void RenderAbstractNamespace(AbstractNamespace ns)
        {
            _currentOutput.AddRange(ns.Imports.Select(i => $"use {i};"));
            _currentOutput.AddRange(_rendererExtension.RenderClientImports().Select(i => $"use {i};"));
            _currentOutput.Add($"namespace {ns.Name};");
        }

        protected override void RenderAbstractProperty(AbstractProperty<EntityNode> prop)
        {
            var visibility = prop.AccessModifier.ToString().ToLower();
            var rt = RenderAbstractDataType(prop.DataType);
            var fieldNamePascal = prop.Name.ToProperCase().ToPascalCase();
            var fieldNameCamel = prop.Name.ToProperCase().ToCamelCase();

            var projectName = _project.GetProperName();

            WriteIndented($"private $_{fieldNameCamel};");
            WriteIndented(RenderDocBlock(
                null,
                new Dictionary<string, string>
                {
                    { "return", prop.DataType.Node.IsEntity ? $@"\{projectName}\{_meta.Configuration.EntityLocation}\{rt}" : $"{rt}"}
                }
            ));
            WriteIndented($"public function get{fieldNamePascal}() : {rt}");
            WriteIndented("{");
            _indentLevel++;
            WriteIndented($"return $this->_{fieldNameCamel};");
            _indentLevel--;
            WriteIndented("}");
            WriteIndented(RenderDocBlock(
                null,
                new Dictionary<string, string>
                {
                    { "param", prop.DataType.Node.IsEntity ? $@"\{projectName}\{_meta.Configuration.EntityLocation}\{rt} $value" : $"{rt} $value"},
                    { "return", $"{prop.Parent.Name}"}
                }
            ));
            WriteIndented($"public function set{fieldNamePascal}({rt} $value) : {prop.Parent.Name}");
            WriteIndented("{");
            _indentLevel++;
            WriteIndented($"$this->_{fieldNameCamel} = $value;");
            WriteIndented("return $this;");
            _indentLevel--;
            WriteIndented("}");
        }

        protected override string RenderAbstractVariableDefinition(AbstractVariableDefinition variable)
        {
            return $"{RenderAbstractDataType(variable.DataType)} {variable.Name}";
        }

        protected override void RenderApiClass(AbstractClass<ApiNode> ac)
        {
            _activeConfig = ac.Config;
            RenderNode(ac.Namespace);
            WriteIndented($"class {ac.Name}Client");
            WriteIndented("{");
            _indentLevel++;
            RenderNodes<RootObject>(ac.TopOfClassExtra);
            RenderNodes<BaseProperty>(ac.Properties);
            RenderNode(ac.Constructor);
            RenderNodes<AbstractFunction>(ac.Functions);
            _indentLevel--;
            WriteIndented("}");
        }

        protected override void RenderEntityClass(AbstractClass<EntityNode> ac)
        {
            RenderNode(ac.Namespace);
            WriteIndented($"class {ac.Name}");
            WriteIndented("{");
            _indentLevel++;
            RenderNodes<BaseProperty>(ac.Properties);
            _indentLevel--;
            WriteIndented("}");
        }

        protected override void RenderGenericClass(GenericClass genericClass)
        {
            var inherits = "";
            var inheritedInterfaces = genericClass.Inheritence.Where(i => i.IsInterface);
            var inheritedClasses = genericClass.Inheritence.Where(i => !i.IsInterface);
            if (inheritedClasses.Count() > 0)
            {
                inherits += "extends ";
                inherits += string.Join(", ", inheritedClasses);
            }

            if (inheritedInterfaces.Count() > 0)
            {
                inherits += "implements ";
                inherits += string.Join(", ", inheritedInterfaces);
            }
            WriteIndented($"class {genericClass.Name} {inherits}");
            WriteIndented("{");
            _indentLevel++;
            RenderGenericProperties(genericClass.Properties);
            RenderGenericConstructor(genericClass.Constructor);
            RenderGenericFunctions(genericClass.Functions);
            _indentLevel--;
            WriteIndented("}");
        }

        protected override void RenderGenericConstructor(GenericConstructor genericConstructor)
        {
            if (genericConstructor == null)
            {
                return;
            }

            var args = string.Join(", ", genericConstructor.Arguments.Select(a => RenderGenericVariableDefinition(a)));
            WriteIndented($"{genericConstructor.AccessModifier.ToString().ToLower()} function __construct({args})");
            WriteIndented("{");
            _indentLevel++;
            WriteIndented(genericConstructor.Body);
            _indentLevel--;
            WriteIndented("}");
        }

        protected override void RenderGenericFunction(GenericFunction genericFunction)
        {
            
        }

        protected override void RenderGenericProperty(GenericProperty prop)
        {
            
        }

        protected override void RenderGenericTryCatch(GenericTryCatch tryCatch)
        {
            
        }

        protected override string RenderGenericVariableDefinition(GenericVariableDefinition variableDefinition)
        {
            return "";
        }

        protected override List<string> RenderDocBlock(string description, Dictionary<string, string> paramValues)
        {
            var output = new List<string>();
            output.Add("/**");

            if (description != null)
            {
                output.Add($" * {description}");
                output.Add(" *");
            }
            
            foreach (var param in paramValues.Keys)
            {
                output.Add($" * @{param} {paramValues[param]}");
            }
            output.Add(" */");
            return output;
        }
    }
}