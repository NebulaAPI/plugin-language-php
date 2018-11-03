using System.Linq;
using Nebula.SDK.Compiler.Abstracts;
using Nebula.SDK.Objects;

namespace Nebula.SDK.Compiler.Objects.PHP
{
    public class PhpClientClass : PhpClass<ApiNode>
    {
        public override void Init()
        {
            Config = Compiler.ApiConfig[RootNode];

            Functions.AddRange(RootNode.SearchByType<FunctionNode>().Select(f => new AbstractFunction(f)));

            Properties.AddRange(Compiler.CompilerExtension.GetProperties());

            Constructor = Compiler.CompilerExtension.GetConstructor(RootNode.Name, Config);

            TopOfClassExtra.AddRange(Compiler.CompilerExtension.GetTopOfClassExtra(Config));
        }
    }
}