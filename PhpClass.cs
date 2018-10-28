using Nebula.SDK.Compiler.Abstracts;
using Nebula.SDK.Objects;

namespace Nebula.SDK.Compiler.Objects.PHP
{
    public abstract class PhpClass<T> : AbstractClass<T> where T : MainObjectNode
    {
        public PhpClass()
        {
        }

        public PhpClass(AbstractNamespace ns, T root, AbstractCompiler compiler) : base(ns, root, compiler)
        {
        }
    }
}