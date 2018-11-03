using Nebula.SDK.Compiler.Abstracts;
using Nebula.SDK.Compiler.Objects.PHP;
using Nebula.SDK.Interfaces;
using Nebula.SDK.Renderers;

namespace plugin_language_php
{
    public class PhpLanguagePlugin : ILanguagePlugin
    {
        public AbstractCompiler GetCompiler()
        {
            return new PhpCompiler();
        }

        public string GetLanguageName()
        {
            return "php";
        }

        public AbstractRenderer GetRenderer()
        {
            return null;
        }
    }
}